using System.Diagnostics;
using System.Runtime.Serialization.Json;
using Microsoft.Agents.AI.DevUI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Extensions.AI;
using Plugin.McpBridge.Agents;
using Plugin.McpBridge.Mcp;
using Plugin.McpBridge.Tests;
using Plugin.McpBridge.Workflows;

namespace Plugin.McpBridge.DevUI;

internal static class Program
{
	private static String AssemblyName => typeof(Program).Assembly.GetName().Name ?? "Plugin.McpBridge.Undefined";

	private static async Task<Int32> Main(String[] args)
	{
		SettingsDto? config = await TryLoadConfigAsync(args);
		if(config is null)
			return 1;

		using(CancellationTokenSource lifetimeCts = new CancellationTokenSource())
		{
			Int32 parentPidIndex = Array.IndexOf(args, "--parent-pid");
			if(parentPidIndex >= 0 && parentPidIndex + 1 < args.Length && Int32.TryParse(args[parentPidIndex + 1], out Int32 parentPid))
				_ = WatchParentAsync(parentPid, lifetimeCts);

			var bridgeTools = await FetchBridgeToolsAsync(config);

			var remainingArgs = parentPidIndex >= 0
				? args[1..parentPidIndex].Concat(args[(parentPidIndex + 2)..]).ToArray()
				: args[1..];

			AgentHandle agent = await new AgentFactory().CreateAgent(
				config.GetSelectedProvider() ?? throw new InvalidOperationException("No AI provider configured."),
				bridgeTools,
				config.Instructions ?? String.Empty,
				skillsDirectory: config.SkillsDirectory,
				token: lifetimeCts.Token);

			List<WorkflowHandle> workflows = new List<WorkflowHandle>();
			if(config.WorkflowsDirectory is not null)
			{
				foreach(String workflowFile in Directory.EnumerateFiles(config.WorkflowsDirectory, "*.json"))
				{
					Console.WriteLine($"Loading workflow from {workflowFile}");
					WorkflowLoader2 loader = new WorkflowLoader2(workflowFile);
					WorkflowHandle workflowHandle = await loader.BuildAsync(config.AiProviders, bridgeTools);
					workflows.Add(workflowHandle);
				}
			}

			WebApplication app = await BuildWebApp(remainingArgs, config, agent, workflows);
			Console.WriteLine($"DevUI running at {config.UiServerUrl}/devui");
			await app.RunAsync(lifetimeCts.Token);
		}
		return 0;
	}

	private static async Task<SettingsDto?> TryLoadConfigAsync(String[] args)
	{
		if(args.Length == 0)
		{
			await Console.Error.WriteLineAsync($"Usage: {AssemblyName} <config-file>");
			return null;
		}

		String configPath = args[0];
		if(!File.Exists(configPath))
		{
			await Console.Error.WriteLineAsync($"Config file not found: {configPath}");
			return null;
		}

		SettingsDto config;
		DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(SettingsDto));
		using(FileStream stream = File.OpenRead(configPath))
			config = (SettingsDto)serializer.ReadObject(stream)!;
		File.Delete(configPath);
		return config;
	}

	private static async Task<AIFunction[]> FetchBridgeToolsAsync(SettingsDto config)
	{
		if(String.IsNullOrEmpty(config.McpServerUrl))
			return Array.Empty<AIFunction>();

		AIFunction[] tools;
		using(CancellationTokenSource timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
		{
			HttpClient bridgeHttp = new HttpClient { Timeout = TimeSpan.FromSeconds(10), BaseAddress = new Uri(config.McpServerUrl) };
			tools = await McpClient.FetchAllAsync(AssemblyName, bridgeHttp, timeoutCts.Token);
		}
		Console.WriteLine($"Bridge connected: {tools.Length:N0} tools loaded from {config.McpServerUrl}");
		return tools;
	}

	private static async Task<WebApplication> BuildWebApp(String[] args, SettingsDto config, AgentHandle agent, IEnumerable<WorkflowHandle> workflows)
	{
		WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
		((IWebHostBuilder)builder.WebHost).UseUrls(config.UiServerUrl);

		// Force the console logger to capture detailed framework traces
		builder.Logging.AddConsole();
		builder.Logging.SetMinimumLevel(LogLevel.Debug); // Shows model binding/deserialization errors

		builder.AddAIAgent(agent.Agent.Name!, (sp, name) => agent.Agent);

		foreach(var workflow in workflows)
			builder.AddWorkflow(workflow.Workflow.Name!, (sp, key) => workflow.Workflow)
				.AddAsAIAgent();

		builder.Services.AddDevUI((options) =>
		{
			options.AllowRemoteAccess = false;
		});
		builder.Services.AddOpenAIResponses();
		builder.Services.AddOpenAIConversations();

		WebApplication app = builder.Build();

		app.UseDeveloperExceptionPage();

		app.MapOpenAIResponses();
		app.MapOpenAIConversations();
		app.MapDevUI();
		return app;
	}

	private static async Task WatchParentAsync(Int32 parentPid, CancellationTokenSource cts)
	{
		try
		{
			Process parent = Process.GetProcessById(parentPid);
			await parent.WaitForExitAsync(cts.Token);
			Console.WriteLine($"Parent process {parentPid} exited. Shutting down.");
		}
		catch(ArgumentException)
		{
			Console.WriteLine($"Parent process {parentPid} not found. Shutting down.");
		}
		finally
		{
			await cts.CancelAsync();
		}
	}
}