using System.Diagnostics;
using System.Text;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Extensions.AI;
using McpBridge.Core;
using Plugin.McpBridge.Data;
using Plugin.McpBridge.Events;
using Plugin.McpBridge.Tools;

namespace Plugin.McpBridge.Agents;

/// <summary>Manages the MAF AIAgent instance and drives the multi-turn agent loop.</summary>
public class AssistantAgent : IDisposable
{
	private readonly AgentFactory _agentFactory;

	public event EventHandler<AgentResponseEventArgs>? AiResponseReceived;
	public event EventHandler<AgentConfirmationEventArgs>? ConfirmationRequired;

	public String AgentName => this.Handle?.Agent.Name ?? FileSystemAgentSessionStore.DefaultAgentName;

	public String SessionScopeName { get; protected set; } = FileSystemAgentSessionStore.DefaultAgentName;

	public Boolean IsEvaluationCacheEnabled => this.Handle?.IsEvaluationCacheEnabled ?? false;

	protected IMcpTrace Trace { get; }
	protected AIFunction[] Tools { get; }
	protected AgentHandle? Handle { get; set; }
	protected AgentSession? Session { get; set; }
	protected AgentSessionStore? SessionStore { get; set; }
	protected String? ConversationId { get; set; }

	public AssistantAgent(IMcpTrace trace, ToolsFactory toolsFactory, AgentFactory agentFactory)
	{
		this.Trace = trace ?? throw new ArgumentNullException(nameof(trace));
		_ = toolsFactory ?? throw new ArgumentNullException(nameof(toolsFactory));

		this.Tools = toolsFactory.CreateTools(this.Trace).ToArray();
		this._agentFactory = agentFactory ?? throw new ArgumentNullException(nameof(agentFactory));
	}

	public virtual async Task Initialize(Settings settings, String instructions, AgentSessionStore? sessionStore = null, String? conversationId = null)
	{
		_ = settings ?? throw new ArgumentNullException(nameof(settings));
		_ = instructions ?? throw new ArgumentNullException(nameof(instructions));

		this.SessionStore = sessionStore;
		this.ConversationId = conversationId;
		this.ResetState();

		this.Handle = await this.CreateAgent(this.Tools, instructions, settings.SelectedAgent, settings.AiProviders);
		this.SessionScopeName = this.Handle.Agent.Name;

		if(!this.Handle.IsEvaluationCacheEnabled && this.SessionStore != null && this.ConversationId != null)
			this.Session = await this.SessionStore.GetSessionAsync(this.Handle.Agent, this.ConversationId, CancellationToken.None);

		this.Trace.TraceEvent(TraceEventType.Verbose, 0, $"Initialized AssistantAgent with instructions '{instructions}'.");
	}

	protected virtual async Task<AgentHandle> CreateAgent(AIFunction[] tools, String instructions, AiAgentDto agent, IEnumerable<AiProviderDto> providers, CancellationToken token = default)
		=> await this._agentFactory.CreateAgent(
			agent,
			providers,
			tools,
			instructions,
			token: token);

	public async Task InvokeMessageAsync(String message, DataContent[]? files = null, Boolean useStreaming = true, CancellationToken cancellationToken = default)
	{
		if(String.IsNullOrWhiteSpace(message))
			throw new ArgumentNullException(nameof(message), "Message cannot be null or whitespace. Provide a valid message to invoke the agent.");

		if(this.Handle == null)
			throw new InvalidOperationException("Agent is not initialized. Call Initialize or InitializeWorkflow before invoking messages.");

		String traceMessage = "< " + message;
		if(files?.Length> 0)
			traceMessage += " Attachments: " + String.Join(", ", files.Select(f => $"{f.Name} Length: {f.Data.Length}"));

		this.Trace.TraceEvent(TraceEventType.Verbose, 0, traceMessage);

		if(!this.IsEvaluationCacheEnabled && this.Session == null)
			this.Session = await this.Handle.Agent.CreateSessionAsync(cancellationToken);

		ChatMessage chatMessage = BuildUserMessage(message, files);
		if(useStreaming)
			await this.ProcessStreamingMessage(chatMessage, cancellationToken);
		else
			await this.ProcessMessage(chatMessage, this.Handle.Agent, cancellationToken);

		if(this.SessionStore != null && this.ConversationId != null && this.Session != null)
			await this.SessionStore.SaveSessionAsync(this.Handle.Agent, this.ConversationId, this.Session, cancellationToken);

		static ChatMessage BuildUserMessage(String text, DataContent[]? files = null)
		{
			if(files == null || files.Length == 0)
				return new ChatMessage(ChatRole.User, text);

			List<AIContent> contents = new List<AIContent> { new TextContent(text) };
			foreach(DataContent image in files)
				contents.Add(image);
			return new ChatMessage(ChatRole.User, contents);
		}
	}

	private void OnAiResponseReceived(AgentResponseEventArgs e)
		=> this.AiResponseReceived?.Invoke(this, e);

	private Task<Boolean> OnConfirmationRequiredAsync(AgentConfirmationEventArgs e)
	{
		this.ConfirmationRequired?.Invoke(this, e);
		return e.ConfirmationTask;
	}

	private async Task ProcessMessage(ChatMessage message, AIAgent agent, CancellationToken token)
	{
		AgentResponse response = await agent.RunAsync(message, this.Session, null, token);
		while(response.FinishReason == ChatFinishReason.ToolCalls)
		{
			ToolApprovalRequestContent request = (ToolApprovalRequestContent)response.Messages[^1].Contents[0];
			if(request.ToolCall is FunctionCallContent function)
			{
				Boolean approved = await this.OnConfirmationRequiredAsync(new AgentConfirmationEventArgs(function));
				ToolApprovalResponseContent approvalResponse = request.CreateResponse(approved);
				ChatMessage approvalMessage = new ChatMessage(ChatRole.User, [approvalResponse]);
				response = await agent.RunAsync(approvalMessage, this.Session, null, token);
			} else
				break;
		}

		this.Trace.TraceEvent(TraceEventType.Verbose, 0, "> " + response.ToString());
		if(response.Usage != null)
			this.Trace.TraceEvent(TraceEventType.Verbose, 0, $"AgentId: {response.AgentId} Tokens: {String.Join(Environment.NewLine, Utils.ParseTokenUsageCount(response.Usage))}");

		ChatMessage finalMessage = new ChatMessage(ChatRole.Assistant, response.Text) {AuthorName = response.AgentId, CreatedAt = response.CreatedAt};
		this.OnAiResponseReceived(new AgentResponseEventArgs(finalMessage, true));
	}

	private async Task ProcessStreamingMessage(ChatMessage message, CancellationToken cancellationToken)
	{
		while(true)
		{
			IAsyncEnumerable<AgentResponseUpdate> stream = this.Handle.Agent.RunStreamingAsync(message, this.Session, null, cancellationToken);
			ToolApprovalResponseContent? approvalResponse = await this.HandleStreamingResponseAsync(stream, cancellationToken);

			if(approvalResponse == null)
			{
				this.OnAiResponseReceived(new AgentResponseEventArgs(null, true));
				break;
			}

			message = new ChatMessage(ChatRole.User, [approvalResponse]);
		}
	}

	private async Task<ToolApprovalResponseContent?> HandleStreamingResponseAsync(IAsyncEnumerable<AgentResponseUpdate> stream, CancellationToken cancellationToken)
	{
		Boolean hasReasoning = false;

		StringBuilder responseCache = new StringBuilder();
		ToolApprovalResponseContent? approvalResponse = null;

		await foreach(AgentResponseUpdate update in stream.WithCancellation(cancellationToken))
		{
			if(update.Contents == null)
				continue;
			if(update.Contents.Count == 0 && update.FinishReason == ChatFinishReason.Stop)
			{
				ChatMessage message = new ChatMessage(ChatRole.Assistant, responseCache.ToString()) { AuthorName = update.AuthorName, CreatedAt = update.CreatedAt };
				this.Trace.TraceEvent(TraceEventType.Verbose, 0, "> " + message.Text);
				this.OnAiResponseReceived(new AgentResponseEventArgs(message, false));
				responseCache.Clear();
				continue;
			}
			foreach(AIContent content in update.Contents)
			{
				if(content is TextReasoningContent reasoningContent && !String.IsNullOrEmpty(reasoningContent.Text))
				{
					if(!hasReasoning)
					{
						hasReasoning = true;
						ChatMessage thinkingMessage = new ChatMessage(ChatRole.Assistant, "> *Thinking...*\n\n") { AuthorName = update.AuthorName, CreatedAt = update.CreatedAt };
						this.OnAiResponseReceived(new AgentResponseEventArgs(thinkingMessage, false));
					}
					ChatMessage reasoningMessage = new ChatMessage(ChatRole.Assistant, reasoningContent.Text) { AuthorName = update.AuthorName, CreatedAt = update.CreatedAt };
					this.OnAiResponseReceived(new AgentResponseEventArgs(reasoningMessage, false));
				} else if(content is TextContent textContent && !String.IsNullOrEmpty(textContent.Text))
					responseCache.Append(textContent.Text);
				else if(content is UsageContent usageContent)
					this.Trace.TraceEvent(TraceEventType.Verbose, 0, $"AuthorName: {update.AuthorName} Tokens: {String.Join(Environment.NewLine, Utils.ParseTokenUsageCount(usageContent.Details))}");
				else if(content is ToolApprovalRequestContent request)
				{
					if(request.ToolCall is FunctionCallContent function)
					{
						Boolean approved = await this.OnConfirmationRequiredAsync(new AgentConfirmationEventArgs(function));
						approvalResponse = request.CreateResponse(approved);
					} else
						break;
				} else if(content.RawRepresentation is GitHub.Copilot.SessionModelChangeEvent newModel && newModel.Data.NewModel != null)
					this.Trace.TraceEvent(TraceEventType.Verbose, 0, $"GitHub -> Model used: {newModel.Data.NewModel}");
				else if(content.RawRepresentation is GitHub.Copilot.SystemMessageEvent systemMessage)
					this.Trace.TraceEvent(TraceEventType.Verbose, 0, $"GitHub -> Content: {systemMessage.Data.Content}");
			}
		}

		if(responseCache.Length > 0)
		{
			ChatMessage message = new ChatMessage(ChatRole.Assistant, responseCache.ToString()) { AuthorName = this.AgentName, CreatedAt = DateTimeOffset.UtcNow };
			this.Trace.TraceEvent(TraceEventType.Verbose, 0, "> " + message.Text);
			this.OnAiResponseReceived(new AgentResponseEventArgs(message, false));
		}

		return approvalResponse;
	}

	/// <inheritdoc/>
	public void Dispose()
		=> this.Handle?.Dispose();

	protected void ResetState()
	{
		this.Session = null;
		this.SessionScopeName = FileSystemAgentSessionStore.DefaultAgentName;
		this.Handle?.Dispose();
		this.Handle = null;
	}
}