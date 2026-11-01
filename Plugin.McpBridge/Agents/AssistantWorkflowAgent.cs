using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.Logging;
using McpBridge.Core;
using McpBridge.Core.Tools;
using McpBridge.Core.Workflows;
using McpBridge.Core.Agents;

namespace Plugin.McpBridge.Agents;

internal class AssistantWorkflowAgent : AssistantAgent
{
	public AssistantWorkflowAgent(ILogger logger, ToolsFactory toolsFactory, AgentFactory agentFactory)
		: base(logger, toolsFactory, agentFactory)
	{
	}

	public async Task InitializeWorkflow(
		Settings settings,
		WorkflowFactoryItem workflow,
		AgentSessionStore? sessionStore = null,
		String? conversationId = null,
		CancellationToken token = default)
	{
		_ = settings ?? throw new ArgumentNullException(nameof(settings));
		_ = workflow ?? throw new ArgumentNullException(nameof(workflow));

		this.SessionStore = sessionStore;
		this.ConversationId = conversationId;
		this.ResetState();

		WorkflowLoader2 loader = new WorkflowLoader2(settings, workflow.WorkflowPath, this.Logger);
		WorkflowHandle workflowHandle = await loader.BuildAsync(settings.AiProviders, this.Tools, token);
		var agent = workflowHandle.Workflow.AsAIAgent(name: workflow.Name);
		this.Handle = AgentHandle.FromWorkflow(agent, null, workflowHandle.IsEvaluationCacheEnabled);

		this.SessionScopeName = workflow.Name;

		if(!this.Handle.IsEvaluationCacheEnabled && this.SessionStore != null && this.ConversationId != null)
			this.Session = await this.SessionStore.GetOrCreateSessionAsync(this.Handle.Agent, new AgentSessionStoreKey(this.ConversationId, null), token);

		this.Logger.WorkflowAgentInitialized(workflow.Name);
	}
}