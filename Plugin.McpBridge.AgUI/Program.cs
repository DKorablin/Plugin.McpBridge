using System.Text.Json;
using Azure;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.DurableTask;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Worker;
using Microsoft.Extensions.AI;
using Plugin.McpBridge.Agents;
using Plugin.McpBridge.AgUI.Agents;
using Plugin.McpBridge.Tests;
using Plugin.McpBridge.UI;
using Plugin.McpBridge.Workflows;

namespace Plugin.McpBridge.AgUI;

internal static class Program
{
	private static AgentFactory _agentFactory = new AgentFactory();

	private static async Task<Int32> Main(String[] args)
	{
		using(CancellationTokenSource lifetimeCts = new CancellationTokenSource())
			try
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
			} catch(OperationCanceledException) when(lifetimeCts.IsCancellationRequested)
			{
				return 0;
			}
		return 0;
	}

	private static WebApplication BuildWebApp(String[] args, SettingsDto config, AgentHandle agent, IEnumerable<WorkflowHandle> workflows)
	{
		var builder = WebApplication.CreateBuilder(args);
		((IWebHostBuilder)builder.WebHost).UseUrls(config.UiServerUrl);
		builder.Services.Configure<ConsoleLifetimeOptions>(options => options.SuppressStatusMessages = true);
		Program.ConfigureLogging(builder.Logging);

		List<WorkflowHandle> workflowList = workflows.ToList();
		Boolean useDurableWorkflowScheduler = config.DtsEmulatorProcessEnabled && workflowList.Count > 0;
		if(useDurableWorkflowScheduler)
			builder.Services.ConfigureDurableWorkflows(
				options => options.AddWorkflows(workflowList.Select(w => w.Workflow).ToArray()),
				workerBuilder: worker => worker.UseGrpc(config.DtsEmulatorEndpoint),
				clientBuilder: client => client.UseGrpc(config.DtsEmulatorEndpoint));

		var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

		var agentBuilder = builder
			.AddAIAgent(agent.Agent.Name!, (sp, name) => agent.Agent.AsBuilder().UseApproval(jsonOptions).Build(sp));

		FileSystemAgentSessionStore? sessionStore = null;
		if(config.SessionStorageDirectory != null)
		{// Persist sessions to disk so they survive server restarts
			sessionStore = new FileSystemAgentSessionStore(config.SessionStorageDirectory);
			agentBuilder.WithSessionStore(sessionStore);
		}

		foreach(var workflow in workflowList)
			if(useDurableWorkflowScheduler)
				builder.AddAIAgent(workflow.Workflow.Name!, (sp, name) => sp.GetDurableAgentProxy(workflow.Workflow.Name!));
			else
				builder.AddWorkflow(workflow.Workflow.Name!, (sp, key) => workflow.Workflow)
					.AddAsAIAgent();

		builder.Services.AddAGUI();

		var app = builder.Build();
		MapEmbeddedResource(app, "/agui", "index.html", "text/html; charset=utf-8");
		MapEmbeddedResource(app, "/agui-client.js", "agui-client.js", "application/javascript");
		MapEmbeddedResource(app, "/index.css", "index.css", "text/css");
		MapEmbeddedResource(app, "/favicon.ico", "favicon.ico", "image/x-icon");

		if(sessionStore != null)
			Program.MapHistoryEndpoints(app, sessionStore, agent.Agent.Name ?? "assistant");
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

	/// <summary>Maps GET /history/{conversationId} to read from the agent session store.</summary>
	private static void MapHistoryEndpoints(WebApplication app, FileSystemAgentSessionStore sessionStore, String agentName)
	{
		app.MapGet("/history", () =>
		{
			IEnumerable<String> sessions = sessionStore.ListSessions(agentName);
			return Results.Json(sessions);
		});

		app.MapGet("/history/{conversationId}", async (String conversationId) =>
		{
			if(!Guid.TryParse(conversationId, out _))
				return Results.BadRequest();

			List<ChatMessage> messages = new List<ChatMessage>();
			await foreach(ChatMessage message in sessionStore.ReadSessionAsync(agentName, conversationId))
				messages.Add(message);

			if(messages.Count == 0)
				return Results.NoContent();

			return Results.Json(messages);
		});

		app.MapDelete("/history/{conversationId}", (String conversationId) =>
		{
			if(!Guid.TryParse(conversationId, out _))
				return Results.BadRequest();

			return sessionStore.DeleteSession(agentName, conversationId)
				? Results.NoContent()
				: Results.NotFound();
		});
	}
}