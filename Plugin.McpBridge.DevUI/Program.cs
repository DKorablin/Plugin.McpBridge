using Microsoft.Agents.AI.DevUI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.ObjectModel;
using Plugin.McpBridge.Agents;
using Plugin.McpBridge.Tests;
using Plugin.McpBridge.Workflows;

namespace Plugin.McpBridge.DevUI;

internal static class Program
{
	private static AgentFactory _agentFactory = new AgentFactory();

	private static async Task<Int32> Main(String[] args)
	{
		using(CancellationTokenSource lifetimeCts = new CancellationTokenSource())
		{
			SettingsDto settings = SettingsDto.CreateSettingsFromArgs(ref args, lifetimeCts);

			var bridgeTools = await settings.FetchBridgeToolsAsync();
			Console.WriteLine($"Bridge connected: {bridgeTools.Length:N0} tools loaded from {settings.McpServerUrl}");

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
					Console.WriteLine($"Loading workflow from {workflowFile}");
					WorkflowLoader2 loader = new WorkflowLoader2(settings, workflowFile);
					WorkflowHandle workflowHandle = await loader.BuildAsync(settings.AiProviders, bridgeTools);
					workflows.Add(workflowHandle);
				}
			}

			WebApplication app = await BuildWebApp(args, settings, agent, workflows);
			Console.WriteLine($"DevUI running at {settings.UiServerUrl}/devui");
			await app.RunAsync(lifetimeCts.Token);
		}
		return 0;
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
}