using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace Plugin.McpBridge.AgUI.Agents;

/// <summary>Approval request payload serialized into the <c>request_approval</c> AG-UI tool call arguments.</summary>
internal sealed class ApprovalRequest
{
	[JsonPropertyName("approval_id")] public required String ApprovalId { get; init; }
	[JsonPropertyName("function_name")] public required String FunctionName { get; init; }
	[JsonPropertyName("function_arguments")] public JsonElement? FunctionArguments { get; init; }
}

/// <summary>Approval response payload deserialized from the <c>request_approval</c> AG-UI tool result.</summary>
internal sealed class ApprovalResponse
{
	[JsonPropertyName("approval_id")] public required String ApprovalId { get; init; }
	[JsonPropertyName("approved")] public required Boolean Approved { get; init; }
}

/// <summary>Bidirectional agent middleware that bridges <see cref="ToolApprovalRequestContent"/> and
/// <see cref="ToolApprovalResponseContent"/> to standard AG-UI tool call / tool result events.</summary>
/// <remarks>
/// Outgoing: <see cref="ToolApprovalRequestContent"/> → <see cref="FunctionCallContent"/> named <c>request_approval</c>
/// so the AGUI adapter can stream it as a regular tool call event to the TypeScript client.
/// Incoming: <see cref="FunctionResultContent"/> named <c>request_approval</c> → <see cref="ToolApprovalResponseContent"/>
/// for the inner agent.  The interim approval messages are stripped from history to avoid
/// "tool_call must be followed by tool messages" errors in Azure OpenAI.
/// </remarks>
internal static class ApprovalMiddleware
{
	/// <summary>Installs the approval middleware on the given agent builder.</summary>
	public static AIAgentBuilder UseApproval(this AIAgentBuilder builder, JsonSerializerOptions jsonOptions)
		=> builder.Use(
			runFunc: null,
			runStreamingFunc: (messages, session, options, inner, ct)
				=> RunAsync(messages, session, options, inner, jsonOptions, ct));

	private static async IAsyncEnumerable<AgentResponseUpdate> RunAsync(
		IEnumerable<ChatMessage> messages,
		AgentSession? session,
		AgentRunOptions? options,
		AIAgent inner,
		JsonSerializerOptions jsonOptions,
		[EnumeratorCancellation] CancellationToken cancellationToken)
	{
		IEnumerable<ChatMessage> processed = ConvertApprovalResponsesToFunctionApprovals(messages, jsonOptions);

		await foreach(AgentResponseUpdate update in inner.RunStreamingAsync(processed, session, options, cancellationToken))
			foreach(AgentResponseUpdate converted in ConvertFunctionApprovalsToToolCalls(update, jsonOptions))
				yield return converted;
	}

	/// <summary>
	/// Converts approval request and response messages in the specified sequence to function approval messages, replacing
	/// the original approval tool call and result with a single approval response message.
	/// </summary>
	/// <remarks>
	/// This method identifies and processes messages containing a 'request_approval' tool call and its corresponding result.
	/// Both the tool call and result are removed from the message history and replaced with a single approval response message.
	/// If the approval response or its required data cannot be deserialized, the original message sequence is returned.
	/// This transformation is useful for workflows that require approval steps to be represented as function approvals rather than tool calls.
	/// </remarks>
	/// <param name="messages">The sequence of chat messages to process for approval requests and responses.</param>
	/// <param name="jsonOptions">The JSON serializer options to use when deserializing approval request and response payloads.</param>
	/// <returns>
	/// A new sequence of chat messages in which approval requests and their corresponding results are replaced by function approval messages.
	/// If no approval response is found, the original messages are returned unchanged.
	/// </returns>
	private static IEnumerable<ChatMessage> ConvertApprovalResponsesToFunctionApprovals(
		IEnumerable<ChatMessage> messages, JsonSerializerOptions jsonOptions)
	{
		Dictionary<String, FunctionCallContent> approvalToolCalls = new Dictionary<String, FunctionCallContent>();
		FunctionResultContent? approvalResult = null;

		foreach(ChatMessage message in messages)
			foreach(AIContent content in message.Contents)
			{
				if(content is FunctionCallContent { Name: "request_approval" } call)
					approvalToolCalls[call.CallId] = call;
				else if(content is FunctionResultContent result && approvalToolCalls.ContainsKey(result.CallId))
					approvalResult = result;
			}

		if(approvalResult is null)
			return messages;

		if((approvalResult.Result as JsonElement?)?.Deserialize<ApprovalResponse>(jsonOptions) is not ApprovalResponse response)
			return messages;

		FunctionCallContent originalToolCall = approvalToolCalls[approvalResult.CallId];
		if(originalToolCall.Arguments?.TryGetValue("request", out Object? reqObj) != true
		   || reqObj is not String reqJson
		   || JsonSerializer.Deserialize<ApprovalRequest>(reqJson, jsonOptions) is not ApprovalRequest approvalRequest)
			return messages;

		Dictionary<String, Object?>? functionArguments = approvalRequest.FunctionArguments is { } args
			? args.Deserialize<Dictionary<String, Object?>>(jsonOptions)
			: null;

		FunctionCallContent originalFunctionCall = new FunctionCallContent(
			callId: response.ApprovalId,
			name: approvalRequest.FunctionName,
			arguments: functionArguments);

		ToolApprovalResponseContent responseContent = new ToolApprovalResponseContent(response.ApprovalId, response.Approved, originalFunctionCall);

		List<ChatMessage> newMessages = new List<ChatMessage>();
		foreach(ChatMessage message in messages)
		{
			Boolean hasApprovalResult = false;
			Boolean hasApprovalRequest = false;
			foreach(AIContent content in message.Contents)
			{
				if(content is FunctionResultContent { CallId: var rId } && rId == approvalResult.CallId)
				{
					hasApprovalResult = true;
					break;
				}
				if(content is FunctionCallContent { Name: "request_approval", CallId: var qId } && qId == approvalResult.CallId)
				{
					hasApprovalRequest = true;
					break;
				}
			}

			if(hasApprovalResult)
				newMessages.Add(new ChatMessage(ChatRole.User, [responseContent]));
			else if(!hasApprovalRequest)
				newMessages.Add(message);
			// else: skip the "request_approval" tool call — consumed, must not reach the LLM
		}
		return newMessages;
	}

	/// <summary>
	/// Converts an agent response update containing a tool approval request into a function call request for approval, or returns the original update if no approval request is present.
	/// </summary>
	/// <remarks>
	/// Use this method to transform agent responses that require user approval into a standardized function call format.
	/// If the update does not contain a tool approval request, it is returned unchanged.</remarks>
	/// <param name="update">The agent response update to inspect for a tool approval request.</param>
	/// <param name="jsonOptions">The JSON serializer options to use when serializing approval request data.</param>
	/// <returns>
	/// An enumerable containing either the original update if no approval request is found, or a new update with a function call requesting approval.
	/// </returns>
	private static IEnumerable<AgentResponseUpdate> ConvertFunctionApprovalsToToolCalls(AgentResponseUpdate update, JsonSerializerOptions jsonOptions)
	{
		ToolApprovalRequestContent? approvalRequest = null;
		foreach(AIContent content in update.Contents)
			if(content is ToolApprovalRequestContent req)
			{
				approvalRequest = req;
				break;
			}

		if(approvalRequest is null)
		{
			yield return update;
			yield break;
		}

		FunctionCallContent? functionCall = approvalRequest.ToolCall as FunctionCallContent;
		String approvalId = functionCall?.CallId ?? Guid.NewGuid().ToString("N");

		JsonElement? argsElement = functionCall?.Arguments?.Count > 0
			? JsonSerializer.SerializeToElement<IDictionary<String, Object?>>(functionCall.Arguments, jsonOptions)
			: null;

		ApprovalRequest approvalData = new ApprovalRequest
		{
			ApprovalId = approvalId,
			FunctionName = functionCall?.Name ?? String.Empty,
			FunctionArguments = argsElement,
		};

		yield return new AgentResponseUpdate(ChatRole.Assistant,
		[
			new FunctionCallContent(
				callId: approvalId,
				name: "request_approval",
				arguments: new Dictionary<String, Object?> { ["request"] = JsonSerializer.Serialize(approvalData, jsonOptions) }),
		]);
	}
}