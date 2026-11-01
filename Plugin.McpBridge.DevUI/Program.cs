using Microsoft.Agents.AI.DevUI;
using Microsoft.Agents.AI.DurableTask;
using Microsoft.Agents.AI.Hosting;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Worker;
using McpBridge.Core.Agents;
using McpBridge.Core.Logging;
using McpBridge.Core.Remoting;
using McpBridge.Core.Workflows;
using Microsoft.Extensions.Logging.Console;

namespace Plugin.McpBridge.DevUI;

internal static class Program
{
	private static async Task<Int32> Main(String[] args)
	{
		using(CancellationTokenSource lifetimeCts = new CancellationTokenSource())
			try
			{
				SettingsDto settings = SettingsDto.CreateSettingsFromArgs(ref args, lifetimeCts);
				using ILoggerFactory loggerFactory = LoggerFactory.Create(logging => Program.ConfigureLogging(logging, settings));
				ILogger logger = loggerFactory.CreateLogger(typeof(Program));
				ILogger agentLogger = loggerFactory.CreateLogger<AgentFactory>();
				AgentFactory agentFactory = new AgentFactory(agentLogger);

				var bridgeTools = await settings.FetchBridgeToolsAsync();
				logger.McpBridgeConnected(bridgeTools.Length, settings.McpServerUrl);

				var agentDto = settings.SelectedAgent;
				AgentHandle agent = await agentFactory.CreateAgent(
					agentDto,
					settings.AiProviders,
					bridgeTools,
					settings.Instructions ?? String.Empty,
					token: lifetimeCts.Token);

				List<WorkflowHandle> workflows = new List<WorkflowHandle>();
				if(settings.WorkflowsDirectory is not null)
				{
					foreach(String workflowFile in Directory.EnumerateFiles(settings.WorkflowsDirectory, "*.json"))
					{
						logger.LoadingWorkflow(workflowFile);
						WorkflowLoader2 loader = new WorkflowLoader2(settings, workflowFile, agentLogger);
						WorkflowHandle workflowHandle = await loader.BuildAsync(settings.AiProviders, bridgeTools);
						workflows.Add(workflowHandle);
					}
				}

				WebApplication app = await BuildWebApp(args, settings, agent, workflows);
				logger.DevUIStarted(settings.UiServerUrl);
				await app.RunAsync(lifetimeCts.Token);
			} catch(OperationCanceledException) when(lifetimeCts.IsCancellationRequested)
			{
				return 0;
			}
		return 0;
	}

	private static async Task<WebApplication> BuildWebApp(String[] args, SettingsDto config, AgentHandle agent, IReadOnlyList<WorkflowHandle> workflows)
	{
		WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
		((IWebHostBuilder)builder.WebHost).UseUrls(config.UiServerUrl);
		builder.Services.Configure<ConsoleLifetimeOptions>(options => options.SuppressStatusMessages = true);
		Program.ConfigureLogging(builder.Logging, config);

		Boolean useDurableWorkflowScheduler = config.DtsEmulatorProcessEnabled && workflows.Count > 0;
		if(useDurableWorkflowScheduler)
			builder.Services.ConfigureDurableWorkflows(
				options => options.AddWorkflows(workflows.Select(w => w.Workflow).ToArray()),
				workerBuilder: worker => worker.UseGrpc(config.DtsEmulatorEndpoint),
				clientBuilder: client => client.UseGrpc(config.DtsEmulatorEndpoint));

		builder.AddAIAgent(agent.Agent.Name!, (sp, name) => agent.Agent);

		foreach(var workflow in workflows)
			if(useDurableWorkflowScheduler)
				builder.AddAIAgent(workflow.Workflow.Name!, (sp, name) => sp.GetDurableAgentProxy(workflow.Workflow.Name!));
			else
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
		app.Use(async (context, next) =>
		{
			using IDisposable? correlation = CorrelationContext.BeginRequest(config.CorrelationIdEnabled && HttpMethods.IsPost(context.Request.Method));
			await next(context);
		});

		app.MapOpenAIResponses();
		app.MapOpenAIConversations();
		app.MapDevUI();
		return app;
	}

	private static void ConfigureLogging(ILoggingBuilder logging, SettingsDto settings)
	{
		logging.ClearProviders();
		if(settings.CorrelationIdEnabled)
			logging.AddConsole(options => options.FormatterName = CorrelationConsoleFormatter.FormatterName)
				.AddConsoleFormatter<CorrelationConsoleFormatter, ConsoleFormatterOptions>();
		else
			logging.AddSimpleConsole(options =>
			{
				options.SingleLine = true;
				options.TimestampFormat = "HH:mm:ss ";
			});
	}
}