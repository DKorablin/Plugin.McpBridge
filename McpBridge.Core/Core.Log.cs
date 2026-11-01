using McpBridge.Core.Data;
using McpBridge.Core.Remoting;
using McpBridge.Shared.Logging;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Logging;

namespace McpBridge.Core;

internal static partial class CoreLog
{
	private const Int32 AgentFactoryLogId = LogAppIDs.Core;
	private const Int32 AssistantAgentLogId = LogAppIDs.Core + 100;
	private const Int32 ProcessHostLogId = LogAppIDs.Core + 200;
	private const Int32 ToolFacadeLogId = LogAppIDs.Core + 300;

	#region AgentFactory
	[LoggerMessage(EventId = AgentFactoryLogId, Level = LogLevel.Debug, Message = "Agent '{AgentRole}' created. Provider: {Provider} ({ProviderType})\n\tTools ({ToolsCount}): {Tools}\n\tEvaluation cache: {EvaluationCache}\n\tChat options: {ChatOptions}\n\tContext providers: {ContextProviders}")]
	public static partial void AgentCreated(this ILogger logger, String agentRole, AiProviderDto provider, AiProviderType providerType, Int32 toolsCount, String tools, String evaluationCache, String chatOptions, String contextProviders);

	[LoggerMessage(EventId = AgentFactoryLogId + 1, Level = LogLevel.Debug, Message = "Agent '{AgentRole}' created. Provider: {Provider} ({ProviderType})\n\tTools ({ToolsCount}): {Tools}\n\tContext providers: {ContextProviders}")]
	public static partial void CopilotAgentCreated(this ILogger logger, String agentRole, AiProviderDto provider, AiProviderType providerType, Int32 toolsCount, String tools, String contextProviders);

	[LoggerMessage(EventId = AgentFactoryLogId + 2, Level = LogLevel.Debug, Message = "RAG attached to agent '{AgentId}'. Directory: '{RagDirectory}', Store: {Store}, Embeddings: {EmbeddingProvider} (Dimension={Dimension}, TopResults={TopResults}), Extensions: {Extensions}, Tool: '{ToolName}'")]
	public static partial void RagAttached(this ILogger logger, Guid agentId, String ragDirectory, String store, AiProviderDto embeddingProvider, Int32? dimension, UInt16 topResults, String extensions, String? toolName);

	[LoggerMessage(EventId = AgentFactoryLogId + 3, Level = LogLevel.Warning, Message = "RAG NOT attached to agent '{AgentId}': directory '{RagDirectory}' is set, but embedding provider '{EmbeddingProvider}' does not support embeddings or has no Dimension/ModelId (Dimension={Dimension}, ModelId={ModelId}).")]
	public static partial void RagNotAttached(this ILogger logger, Guid agentId, String ragDirectory, AiProviderDto embeddingProvider, Int32? dimension, String? modelId);

	[LoggerMessage(EventId = AgentFactoryLogId + 4, Level = LogLevel.Error, Message = "RAG search failed for agent '{AgentId}'.")]
	public static partial void RagSearchFailed(this ILogger logger, Guid agentId, Exception exc);

	[LoggerMessage(EventId = AgentFactoryLogId + 5, Level = LogLevel.Debug, Message = "Skills attached to agent '{AgentId}'. Directory: '{SkillsDirectory}'")]
	public static partial void SkillsAttached(this ILogger logger, Guid agentId, String skillsDirectory);
	#endregion

	#region AssistantAgent
	[LoggerMessage(EventId = AssistantAgentLogId, Level = LogLevel.Debug, Message = "Initialized AssistantAgent with instructions '{Instructions}'.")]
	public static partial void AgentInitialized(this ILogger logger, String instructions);

	[LoggerMessage(EventId = AssistantAgentLogId + 1, Level = LogLevel.Debug, Message = "< {Message}")]
	public static partial void UserMessage(this ILogger logger, String message);

	[LoggerMessage(EventId = AssistantAgentLogId + 2, Level = LogLevel.Debug, Message = "< {Message} Attachments: {Attachments}")]
	public static partial void UserMessageWithAttachments(this ILogger logger, String message, String attachments);

	[LoggerMessage(EventId = AssistantAgentLogId + 3, Level = LogLevel.Debug, Message = "> {Response}")]
	public static partial void AgentResponse(this ILogger logger, AgentResponse response);

	[LoggerMessage(EventId = AssistantAgentLogId + 4, Level = LogLevel.Debug, Message = "> {Response}")]
	public static partial void AgentResponseText(this ILogger logger, String response);

	[LoggerMessage(EventId = AssistantAgentLogId + 5, Level = LogLevel.Debug, Message = "AgentId: {AgentId} Tokens: {Tokens}")]
	public static partial void TokenUsage(this ILogger logger, String? agentId, String tokens);

	[LoggerMessage(EventId = AssistantAgentLogId + 6, Level = LogLevel.Debug, Message = "GitHub -> Model used: {Model}")]
	public static partial void CopilotModelChanged(this ILogger logger, String model);

	[LoggerMessage(EventId = AssistantAgentLogId + 7, Level = LogLevel.Debug, Message = "GitHub -> Content: {Content}")]
	public static partial void CopilotSystemMessage(this ILogger logger, String? content);
	#endregion

	#region ProcessHost
	[LoggerMessage(EventId = ProcessHostLogId, Level = LogLevel.Warning, Message = "Process {ProcessType} exited with code {ExitCode:X8}")]
	public static partial void ProcessExited(this ILogger logger, ProcessType processType, Int32 exitCode);

	[LoggerMessage(EventId = ProcessHostLogId + 1, Level = LogLevel.Error, Message = "Error monitoring process {ProcessType}.")]
	public static partial void ProcessMonitoringFailed(this ILogger logger, ProcessType processType, Exception exc);

	[LoggerMessage(EventId = ProcessHostLogId + 2, Level = LogLevel.Information, Message = "{Line}")]
	public static partial void ProcessOutput(this ILogger logger, String line);

	[LoggerMessage(EventId = ProcessHostLogId + 3, Level = LogLevel.Warning, Message = "Error received from process {ProcessType}: {Line}")]
	public static partial void ProcessError(this ILogger logger, ProcessType processType, String line);
	#endregion

	#region ToolFacade
	[LoggerMessage(EventId = ToolFacadeLogId, Level = LogLevel.Debug, Message = "[tool] {ToolName} {Arguments}")]
	public static partial void ToolInvoking(this ILogger logger, String toolName, String arguments);

	[LoggerMessage(EventId = ToolFacadeLogId + 1, Level = LogLevel.Debug, Message = "[tool result] {ToolName} -> {ResultType} Elapsed: {Elapsed}")]
	public static partial void ToolCompleted(this ILogger logger, String toolName, Type? resultType, TimeSpan elapsed);

	[LoggerMessage(EventId = ToolFacadeLogId + 2, Level = LogLevel.Debug, Message = "[tool result] {ToolName} -> {ResultType} Length: {Length} Elapsed: {Elapsed}")]
	public static partial void ToolCompletedArray(this ILogger logger, String toolName, Type? resultType, Int32 length, TimeSpan elapsed);

	[LoggerMessage(EventId = ToolFacadeLogId + 3, Level = LogLevel.Error, Message = "Tool '{ToolName}' failed.")]
	public static partial void ToolFailed(this ILogger logger, String toolName, Exception exc);
	#endregion
}