using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.Extensions.AI;
using Plugin.McpBridge.Agents;
using Plugin.McpBridge.AgUI.Agents;
using Plugin.McpBridge.Tests;
using Plugin.McpBridge.Workflows;

namespace Plugin.McpBridge.AgUI;

internal static class Program
{
	private static AgentFactory _agentFactory = new AgentFactory();

	private static async Task<Int32> Main(String[] args)
	{
		using(CancellationTokenSource lifetimeCts = new CancellationTokenSource())
		{
			SettingsDto settings = SettingsDto.CreateSettingsFromArgs(ref args, lifetimeCts);
			using ILoggerFactory loggerFactory = LoggerFactory.Create(Program.ConfigureLogging);
			ILogger logger = loggerFactory.CreateLogger(typeof(Program));

			var bridgeTools = await settings.FetchBridgeToolsAsync();
			logger.LogInformation("Bridge connected: {Count:N0} tools loaded from {McpServerUrl}", bridgeTools.Length, settings.McpServerUrl);

			var agentDto = settings.SelectedAgent;
			AgentHandle agent = await _agentFactory.CreateAgent(
				agentDto,
				agentDto.GetSelectedProvider(settings.AiProviders),
				settings.AiProviders,
				bridgeTools,
				settings.Instructions ?? String.Empty,
				token: lifetimeCts.Token);

			List<WorkflowHandle> workflows = new List<WorkflowHandle>();
			if(settings.WorkflowsDirectory is not null)
			{
				foreach(String workflowFile in Directory.EnumerateFiles(settings.WorkflowsDirectory, "*.json"))
				{
					logger.LogInformation("Loading workflow from {WorkflowFile}", workflowFile);
					WorkflowLoader2 loader = new WorkflowLoader2(settings, workflowFile);
					WorkflowHandle workflowHandle = await loader.BuildAsync(settings.AiProviders, bridgeTools);
					workflows.Add(workflowHandle);
				}
			}

			WebApplication app = BuildWebApp(args, settings, agent, workflows);
			logger.LogInformation("AG-UI running at {AgUiUrl}", $"{settings.UiServerUrl}/agui");
			await app.RunAsync(lifetimeCts.Token);
		}
		return 0;
	}

	private static WebApplication BuildWebApp(String[] args, SettingsDto config, AgentHandle agent, IEnumerable<WorkflowHandle> workflows)
	{
		var builder = WebApplication.CreateBuilder(args);
		((IWebHostBuilder)builder.WebHost).UseUrls(config.UiServerUrl);
		builder.Services.Configure<ConsoleLifetimeOptions>(options => options.SuppressStatusMessages = true);
		Program.ConfigureLogging(builder.Logging);

		var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

		var agentBuilder = builder
			.AddAIAgent(agent.Agent.Name!, (sp, name) => agent.Agent.AsBuilder().UseApproval(jsonOptions).Build(sp));

		FileSystemAgentSessionStore? sessionStore = null;
		if(config.SessionStorageDirectory != null)
		{// Persist sessions to disk so they survive server restarts
			sessionStore = new FileSystemAgentSessionStore(config.SessionStorageDirectory);
			// withIsolation - if true, each client gets a separate session copy that is only written back on changes. If false, all clients share the same session instance and see real-time updates, but risk interfering with each other.
			agentBuilder.WithSessionStore(sessionStore, withIsolation: false);
		}

		foreach(var workflow in workflows)
			builder.AddWorkflow(workflow.Workflow.Name!, (sp, key) => workflow.Workflow)
				.AddAsAIAgent();

		builder.Services.AddAGUI();

		var app = builder.Build();
		MapEmbeddedResource(app, "/agui", "index.html", "text/html; charset=utf-8");
		MapEmbeddedResource(app, "/agui-client.js", "agui-client.js", "application/javascript");
		MapEmbeddedResource(app, "/index.css", "index.css", "text/css");
		MapEmbeddedResource(app, "/favicon.ico", "favicon.ico", "image/x-icon");

		if(sessionStore != null)
			Program.MapHistoryEndpoints(app, sessionStore, agent.Agent.Name);
		app.MapAGUI(agentBuilder, "/agui");
		return app;
	}

	private static void ConfigureLogging(ILoggingBuilder logging)
	{
		logging.ClearProviders();
		logging.AddSimpleConsole(options =>
		{
			options.SingleLine = true;
			options.TimestampFormat = "HH:mm:ss ";
		});
	}

	static void MapEmbeddedResource(WebApplication app, String pattern, String resourceName, String contentType)
		=> app.MapGet(pattern, () =>
		{
			return Results.Stream(
				async stream =>
				{
					String fullResourceName = $"{SettingsDto.AssemblyName}.wwwroot.{resourceName}";
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
			if(!Guid.TryParse(threadId, out _))
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
}