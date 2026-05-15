using System.Diagnostics;
using System.Runtime.Serialization.Json;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.Extensions.AI;
using Plugin.McpBridge.Agents;
using Plugin.McpBridge.Mcp;
using Plugin.McpBridge.Tests;

namespace Plugin.McpBridge.AgUI;

internal static class Program
{
	private static String AssemblyName => typeof(Program).Assembly.GetName().Name ?? "Plugin.McpBridge.Undefined";

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
		IReadOnlyList<AITool> bridgeTools = await FetchBridgeToolsAsync(config);

		String[] remainingArgs = parentPidIndex >= 0
			? args[1..parentPidIndex].Concat(args[(parentPidIndex + 2)..]).ToArray()
			: args[1..];

		WebApplication app = BuildWebApp(remainingArgs, config, chatClient, bridgeTools);
		Console.WriteLine($"AG-UI running at {config.UiServerUrl}/agui");
		await app.RunAsync(lifetimeCts.Token);
		return 0;
	}

	private static async Task<ProcessConfig?> TryLoadConfigAsync(String[] args)
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

	private static async Task<IReadOnlyList<AITool>> FetchBridgeToolsAsync(ProcessConfig config)
	{
		if(String.IsNullOrEmpty(config.McpServerUrl))
			return Array.Empty<AIFunction>();

		using CancellationTokenSource timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
		HttpClient bridgeHttp = new HttpClient { Timeout = TimeSpan.FromSeconds(10), BaseAddress = new Uri(config.McpServerUrl) };
		IReadOnlyList<AITool> tools = await McpClient.FetchAllAsync(AssemblyName, bridgeHttp, timeoutCts.Token);
		Console.WriteLine($"Bridge connected: {tools.Count:N0} tools loaded from {config.McpServerUrl}");
		return tools;
	}

	private static WebApplication BuildWebApp(String[] args, ProcessConfig config, IChatClient chatClient, IReadOnlyList<AITool> bridgeTools)
	{
		WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
		((IWebHostBuilder)builder.WebHost).UseUrls(config.UiServerUrl);

		IHostedAgentBuilder agentBuilder = builder.AddAIAgent("assistant", config.Instructions, chatClient);
		if(bridgeTools.Count > 0)
			agentBuilder.WithAITools(bridgeTools.ToArray());

		builder.Services.AddAGUI();

		WebApplication app = builder.Build();
		app.MapGet("/agui", async (HttpContext ctx) =>
		{
			ctx.Response.ContentType = "text/html; charset=utf-8";
			using Stream stream = typeof(Program).Assembly
				.GetManifestResourceStream(AssemblyName + ".wwwroot.index.html")!;
			await stream.CopyToAsync(ctx.Response.Body);
		});
		app.MapGet("/agui-client.js", async (HttpContext ctx) =>
		{
			ctx.Response.ContentType = "application/javascript";
			using Stream stream = typeof(Program).Assembly
				.GetManifestResourceStream(AssemblyName + ".wwwroot.agui-client.js")!;
			await stream.CopyToAsync(ctx.Response.Body);
		});
		app.MapGet("/favicon.ico", async (HttpContext ctx) =>
		{
			ctx.Response.ContentType = "image/x-icon";
			using Stream stream = typeof(Program).Assembly
				.GetManifestResourceStream(AssemblyName + ".wwwroot.favicon.ico")!;
			await stream.CopyToAsync(ctx.Response.Body);
		});
		app.MapAGUI(agentBuilder, "/agui");
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
