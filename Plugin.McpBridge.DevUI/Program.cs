using System.Diagnostics;
using System.Runtime.Serialization.Json;
using Microsoft.Agents.AI.DevUI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Extensions.AI;
using Plugin.McpBridge.Agents;
using Plugin.McpBridge.DevUI.Mcp;
using Plugin.McpBridge.Tests;

namespace Plugin.McpBridge.DevUI;

internal static class Program
{
	private static async Task<Int32> Main(String[] args)
	{
		ProcessConfig? config = await TryLoadConfigAsync(args);
		if(config is null)
			return 1;

		using CancellationTokenSource lifetimeCts = new CancellationTokenSource();
		Int32 parentPidIndex = Array.IndexOf(args, "--parent-pid");
		if(parentPidIndex >= 0 && parentPidIndex + 1 < args.Length && Int32.TryParse(args[parentPidIndex + 1], out Int32 parentPid))
			_ = WatchParentAsync(parentPid, lifetimeCts);

		IChatClient chatClient = BuildChatClient(config);
		IReadOnlyList<AIFunction> bridgeTools = await FetchBridgeToolsAsync(config);

		String[] remainingArgs = parentPidIndex >= 0
			? args[1..parentPidIndex].Concat(args[(parentPidIndex + 2)..]).ToArray()
			: args[1..];

		WebApplication app = BuildWebApp(remainingArgs, config, chatClient, bridgeTools);
		Console.WriteLine($"DevUI running at {config.UiServerUrl}/devui");
		await app.RunAsync(lifetimeCts.Token);
		return 0;
	}

	private static async Task<ProcessConfig?> TryLoadConfigAsync(String[] args)
	{
		if(args.Length == 0)
		{
			await Console.Error.WriteLineAsync("Usage: Plugin.McpBridge.DevUI <config-file>");
			return null;
		}

		String configPath = args[0];
		if(!File.Exists(configPath))
		{
			await Console.Error.WriteLineAsync($"Config file not found: {configPath}");
			return null;
		}

		ProcessConfig config;
		DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ProcessConfig));
		using(FileStream stream = File.OpenRead(configPath))
			config = (ProcessConfig)serializer.ReadObject(stream)!;
		File.Delete(configPath);
		return config;
	}

	private static IChatClient BuildChatClient(ProcessConfig config)
	{
		HttpClient httpClient = new HttpClient { Timeout = config.ConnectionTimeout };
		IChatClient raw = AgentFactory.CreateChatClient(config.Provider, httpClient);
		return AgentFactory.ConfigureOptions(raw, config.MaxTokens, config.Provider);
	}

	private static async Task<IReadOnlyList<AIFunction>> FetchBridgeToolsAsync(ProcessConfig config)
	{
		if(String.IsNullOrEmpty(config.McpServerUrl))
			return Array.Empty<AIFunction>();

		using CancellationTokenSource timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
		HttpClient bridgeHttp = new HttpClient { Timeout = TimeSpan.FromSeconds(10), BaseAddress = new Uri(config.McpServerUrl) };
		IReadOnlyList<AIFunction> tools = await ToolsMcpClient.FetchAllAsync(config.McpServerUrl, bridgeHttp, timeoutCts.Token);
		Console.WriteLine($"Bridge connected: {tools.Count} tools loaded from {config.McpServerUrl}");
		return tools;
	}

	private static WebApplication BuildWebApp(String[] args, ProcessConfig config, IChatClient chatClient, IReadOnlyList<AIFunction> bridgeTools)
	{
		WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
		((IWebHostBuilder)builder.WebHost).UseUrls(config.UiServerUrl);

		IHostedAgentBuilder agentBuilder = builder.AddAIAgent("assistant", config.Instructions, chatClient);
		if(bridgeTools.Count > 0)
			agentBuilder.WithAITools(bridgeTools.Cast<AITool>().ToArray());

		builder.Services.AddDevUI();
		builder.Services.AddOpenAIResponses();
		builder.Services.AddOpenAIConversations();

		WebApplication app = builder.Build();
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