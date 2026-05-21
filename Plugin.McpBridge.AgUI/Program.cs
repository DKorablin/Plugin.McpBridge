using System.Diagnostics;
using System.Runtime.Serialization.Json;
using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.Extensions.AI;
using Plugin.McpBridge.Agents;
using Plugin.McpBridge.AgUI.Agents;
using Plugin.McpBridge.Mcp;
using Plugin.McpBridge.Tests;

namespace Plugin.McpBridge.AgUI;

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

			await using AgentHandle handle = await new AgentFactory().CreateAgent(
				config.GetSelectedProvider(),
				config.ConnectionTimeout,
				bridgeTools,
				config.Instructions ?? String.Empty);
			WebApplication app = BuildWebApp(remainingArgs, config, handle);
			Console.WriteLine($"AG-UI running at {config.UiServerUrl}/agui");
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

	private static WebApplication BuildWebApp(String[] args, SettingsDto config, AgentHandle handle)
	{
		var builder = WebApplication.CreateBuilder(args);
		((IWebHostBuilder)builder.WebHost).UseUrls(config.UiServerUrl);

		// Force the console logger to capture detailed framework traces
		builder.Logging.AddConsole();
		builder.Logging.SetMinimumLevel(LogLevel.Debug); // Shows model binding/deserialization errors

		var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
		var sessionStore = new FileSystemAgentSessionStore(jsonOptions);
		var agentBuilder = builder
			.AddAIAgent(handle.Agent.Name, (sp, name) => handle.Agent.AsBuilder().UseApproval(jsonOptions).Build(sp))
			.WithSessionStore(sessionStore);// Persist sessions to disk so they survive server restarts

		builder.Services.AddAGUI();

		var app = builder.Build();
		MapEmbeddedResource(app, "/agui", "index.html", "text/html; charset=utf-8");
		MapEmbeddedResource(app, "/agui-client.js", "agui-client.js", "application/javascript");
		MapEmbeddedResource(app, "/index.css", "index.css", "text/css");
		MapEmbeddedResource(app, "/favicon.ico", "favicon.ico", "image/x-icon");

		Program.MapHistoryEndpoints(app, sessionStore, handle.Agent.Name);
		app.MapAGUI(agentBuilder, "/agui");
		return app;
	}

	static void MapEmbeddedResource(WebApplication app, String pattern, String resourceName, String contentType)
		=> app.MapGet(pattern, () =>
		{
			return Results.Stream(
				async stream =>
				{
					String fullResourceName = $"{AssemblyName}.wwwroot.{resourceName}";
					using(Stream? resStream = typeof(Program).Assembly.GetManifestResourceStream(fullResourceName))
					{
						if(resStream == null)
							throw new FileNotFoundException($"Embedded resource '{resourceName}' not found.");
						await resStream.CopyToAsync(stream);
					}
				},
				contentType);
		});

	/// <summary>Maps GET /history/{threadId} to read from the agent session store.</summary>
	private static void MapHistoryEndpoints(WebApplication app, FileSystemAgentSessionStore sessionStore, String agentName)
	{
		app.MapGet("/history/{threadId}", async (String threadId) =>
		{
			if(!Program.IsValidThreadId(threadId))
				return Results.BadRequest();

			JsonElement? root = await sessionStore.ReadChatHistory(agentName, threadId);
			if(root == null)
				return Results.NotFound();

			if(!root.Value.TryGetProperty("stateBag", out JsonElement stateBag) ||
				!stateBag.TryGetProperty("InMemoryChatHistoryProvider", out JsonElement historyState) ||
				!historyState.TryGetProperty("messages", out JsonElement messagesElement))
				return Results.BadRequest();

			return Results.Json(messagesElement);
		});
	}

	/// <summary>Returns <see langword="true"/> when <paramref name="value"/> is a well-formed UUID/GUID.</summary>
	private static Boolean IsValidThreadId(String value) => Guid.TryParse(value, out _);

	private static async Task WatchParentAsync(Int32 parentPid, CancellationTokenSource cts)
	{
		try
		{
			Process parent = Process.GetProcessById(parentPid);
			await parent.WaitForExitAsync(cts.Token);
			Console.WriteLine($"Parent process {parentPid:N0} exited. Shutting down.");
		}
		catch(ArgumentException)
		{
			Console.WriteLine($"Parent process {parentPid:N0} not found. Shutting down.");
		}
		finally
		{
			await cts.CancelAsync();
		}
	}
}