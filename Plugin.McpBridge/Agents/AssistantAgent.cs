using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Plugin.McpBridge.Data;
using Plugin.McpBridge.Events;
using Plugin.McpBridge.Tools;
using SAL.Flatbed;

namespace Plugin.McpBridge.Agents;

/// <summary>Manages the MAF AIAgent instance and drives the multi-turn agent loop.</summary>
internal class AssistantAgent : IDisposable
{
	private readonly ITraceSource _trace;
	private readonly IHost _host;
	private readonly ToolsFactory _toolsFactory;
	private readonly AgentFactory _agentFactory;
	private AgentHandle? _handle;
	private AgentSession? _session;

	public event EventHandler<AgentResponseEventArgs>? AiResponseReceived;
	public event EventHandler<AgentConfirmationEventArgs>? ConfirmationRequired;

	public AssistantAgent(
		ITraceSource trace,
		IHost host,
		ToolsFactory toolsFactory,
		AgentFactory? agentFactory)
	{
		this._host = host ?? throw new ArgumentNullException(nameof(host));
		this._trace = trace ?? throw new ArgumentNullException(nameof(trace));
		this._toolsFactory = toolsFactory ?? throw new ArgumentNullException(nameof(toolsFactory));
		this._agentFactory = agentFactory ?? throw new ArgumentNullException(nameof(agentFactory));
	}

	public virtual async Task Initialize(Settings settings, AiProviderDto provider, String? sessionJson = null)
	{
		_ = settings ?? throw new ArgumentNullException(nameof(settings));
		_ = provider ?? throw new ArgumentNullException(nameof(provider));

		this._session = null;
		this._handle?.Dispose();

		var tools = this._toolsFactory.CreateTools(this._trace).ToArray();
		var instructions = AgentFactory.BuildSystemInstructions(settings, settings.SelectedAgent, this._host);
		this._handle = await this.CreateAgent(provider, tools, instructions, settings.SelectedAgent);

		if(sessionJson != null)
			this._session = await this._handle.Agent.DeserializeSessionAsync(
				JsonSerializer.Deserialize<JsonElement>(sessionJson),
				cancellationToken: CancellationToken.None);

		this._trace.TraceEvent(TraceEventType.Verbose, 0, $"Initialized AssistantAgent with instructions '{instructions}'.");
	}

	protected virtual async Task<AgentHandle> CreateAgent(
		AiProviderDto provider, AIFunction[] tools, String instructions, AiAgentDto agent, CancellationToken token = default)
		=> await this._agentFactory.CreateAgent(
			agent,
			provider,
			tools,
			instructions,
			token: token);

	/// <summary>Asynchronously retrieves the current session state as a JSON string.</summary>
	/// <param name="token">
	/// A cancellation token that can be used to cancel the asynchronous operation.
	/// The default value is <see cref="CancellationToken.None"/>.
	/// </param>
	/// <returns>A JSON string representing the current session state, or <see langword="null"/> if there is no active session.</returns>
	public async Task<String?> GetSessionState(CancellationToken token = default)
	{
		if(this._handle == null || this._session == null)
			return null;

		var json = await this._handle.Agent.SerializeSessionAsync(this._session, cancellationToken: token);
		return json.ToString();
	}

	public async Task InvokeMessageAsync(String message, DataContent[]? files = null, CancellationToken cancellationToken = default)
	{
		if(String.IsNullOrWhiteSpace(message))
		{
			this.OnAiResponseReceived(new AgentResponseEventArgs("Message is empty.", true));
			return;
		}

		this._trace.TraceEvent(TraceEventType.Verbose, 0, "< " + message);

		if(this._handle == null)
		{
			this.OnAiResponseReceived(new AgentResponseEventArgs("AI is not configured. Add LLM configuration options in plugin settings.", true));
			return;
		}

		if(this._session == null)
			this._session = await this._handle.Agent.CreateSessionAsync(cancellationToken);

		try
		{
			ChatMessage chatMessage = AssistantAgent.BuildUserMessage(message, files);
			await this.ProcessMessage(chatMessage, this._handle.Agent, cancellationToken);

			/*IAsyncEnumerable <AgentResponseUpdate> stream = this._agent.RunStreamingAsync(AssistantAgent.BuildUserMessage(message, images), this._session, null, cancellationToken);
			await this.HandleStreamingResponseAsync(stream, cancellationToken);*/
		} catch(HttpRequestException exc)
		{
			this._trace.TraceData(TraceEventType.Error, 0, exc);
			this.OnAiResponseReceived(new AgentResponseEventArgs($"AI request failed: {exc.Message}", true));
		} catch(OperationCanceledException)
		{
			this.OnAiResponseReceived(new AgentResponseEventArgs("Operation was cancelled.", true));
		} catch(Exception exc)
		{
			this._trace.TraceData(TraceEventType.Error, 0, exc);
			throw;
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
		AgentResponse response = await agent.RunAsync(message, this._session, null, token);
		while(response.FinishReason == ChatFinishReason.ToolCalls)
		{
			ToolApprovalRequestContent request = (ToolApprovalRequestContent)response.Messages[^1].Contents[0];
			if(request.ToolCall is FunctionCallContent function)
			{
				Boolean approved = await this.OnConfirmationRequiredAsync(new AgentConfirmationEventArgs(function));
				ToolApprovalResponseContent approvalResponse = request.CreateResponse(approved);
				ChatMessage approvalMessage = new ChatMessage(ChatRole.User, [approvalResponse]);
				response = await agent.RunAsync(approvalMessage, this._session, null, token);
			}
		}

		this._trace.TraceEvent(TraceEventType.Verbose, 0, "> " + response.ToString());
		String aiResponse = response.Text;
		if(response.Usage != null)
			this._trace.TraceEvent(TraceEventType.Verbose, 0, $"Tokens: {String.Join(Environment.NewLine, Utils.ParseTokenUsageCount(response.Usage))}");

		this.OnAiResponseReceived(new AgentResponseEventArgs(aiResponse, true));
	}

	private async Task HandleStreamingResponseAsync(IAsyncEnumerable<AgentResponseUpdate> stream, CancellationToken cancellationToken)
	{
		StringBuilder textBuilder = new StringBuilder();
		Boolean hasReasoning = false;
		UsageDetails? usage = null;

		await foreach(AgentResponseUpdate update in stream.WithCancellation(cancellationToken))
		{
			if(update.Contents == null)
				continue;
			foreach(AIContent content in update.Contents)
			{
				if(content is TextReasoningContent reasoningContent && !String.IsNullOrEmpty(reasoningContent.Text))
				{
					if(!hasReasoning)
					{
						hasReasoning = true;
						this.OnAiResponseReceived(new AgentResponseEventArgs("> *Thinking...*\n\n", false));
					}
					this.OnAiResponseReceived(new AgentResponseEventArgs(reasoningContent.Text, false));
				}
				else if(content is TextContent textContent && !String.IsNullOrEmpty(textContent.Text))
					textBuilder.Append(textContent.Text);
				else if(content is UsageContent usageContent)
					usage = usageContent.Details;
			}
		}

		String aiResponse = textBuilder.ToString();
		this._trace.TraceEvent(TraceEventType.Verbose, 0, "> " + aiResponse);
		if(usage != null)
			this._trace.TraceEvent(TraceEventType.Verbose, 0, $"Tokens: {String.Join(Environment.NewLine, Utils.ParseTokenUsageCount(usage))}");

		if(hasReasoning)
			this.OnAiResponseReceived(new AgentResponseEventArgs("\n\n---\n\n", false));
		this.OnAiResponseReceived(new AgentResponseEventArgs(aiResponse, true));
	}

	private static ChatMessage BuildUserMessage(String text, DataContent[]? images = null)
	{
		if(images == null || images.Length == 0)
			return new ChatMessage(ChatRole.User, text);

		List<AIContent> contents = new List<AIContent> { new TextContent(text) };
		foreach(DataContent image in images)
			contents.Add(image);
		return new ChatMessage(ChatRole.User, contents);
	}

	/// <inheritdoc/>
	public void Dispose()
		=> this._handle?.Dispose();
}