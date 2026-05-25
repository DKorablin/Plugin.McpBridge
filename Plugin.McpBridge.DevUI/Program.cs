using System.Diagnostics;
using System.Runtime.Serialization.Json;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.DevUI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Workflows;
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

			await using(AgentHandle handle = await new AgentFactory().CreateAgent(
				config.GetSelectedProvider(),
				config.ConnectionTimeout,
				bridgeTools,
				config.Instructions ?? String.Empty))
			{
				WebApplication app = BuildWebApp(remainingArgs, config, handle);
				Console.WriteLine($"DevUI running at {config.UiServerUrl}/devui");
				await app.RunAsync(lifetimeCts.Token);
			}
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

	private static WebApplication BuildWebApp(String[] args, SettingsDto config, AgentHandle handle)
	{
		WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
		((IWebHostBuilder)builder.WebHost).UseUrls(config.UiServerUrl);

		// Force the console logger to capture detailed framework traces
		builder.Logging.AddConsole();
		builder.Logging.SetMinimumLevel(LogLevel.Debug); // Shows model binding/deserialization errors

		/*builder
			.AddWorkflow("sequential-flow",
				(sp, key) => AgentWorkflowBuilder.BuildSequential(workflowName: key, agents: handle.Agent))
			.AddAsAIAgent(handle.Agent.Name);// This names the workflow wrapper so DevUI can pull its definitions*/

		WorkflowLoader loader = new WorkflowLoader(@"C:\Visual Studio Projects\C#\SAL\Plugins\Plugin.McpBridge\Plugin.McpBridge\Workflows\social-workflow.json");
		WorkflowHandle workflowHandle = loader.Build(config.AiProviders, config.ConnectionTimeout, Array.Empty<AIFunction>());
		builder.AddAIAgent(handle.Agent.Name!, (sp, name) => handle.Agent);
		builder.AddWorkflow(workflowHandle.Workflow.Name!, (sp, key) => workflowHandle.Workflow)
			.AddAsAIAgent();

		builder.Services.AddDevUI();
		builder.Services.AddOpenAIResponses();
		builder.Services.AddOpenAIConversations();

		WebApplication app = builder.Build();
		app.Lifetime.ApplicationStopped.Register(workflowHandle.Dispose);

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