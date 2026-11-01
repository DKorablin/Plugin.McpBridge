using McpBridge.Shared.Logging;
using Microsoft.Extensions.Logging;

namespace Plugin.McpBridge;

internal static partial class PluginLog
{
	private const Int32 LogId = LogAppIDs.Plugin;

	[LoggerMessage(EventId = LogId, Level = LogLevel.Debug, Message = "Initialized workflow AssistantAgent for '{WorkflowName}'.")]
	public static partial void WorkflowAgentInitialized(this ILogger logger, String workflowName);

	[LoggerMessage(EventId = LogId + 1, Level = LogLevel.Error, Message = "Failed to initialize agent.")]
	public static partial void AgentInitializationFailed(this ILogger logger, Exception exception);

	[LoggerMessage(EventId = LogId + 2, Level = LogLevel.Error, Message = "Chat request failed.")]
	public static partial void ChatRequestFailed(this ILogger logger, Exception exception);

	[LoggerMessage(EventId = LogId + 3, Level = LogLevel.Information, Message = "MCP server started at {McpServerUrl}")]
	public static partial void McpServerStarted(this ILogger logger, String mcpServerUrl);

	[LoggerMessage(EventId = LogId + 4, Level = LogLevel.Error, Message = "MCP request failed.")]
	public static partial void McpRequestFailed(this ILogger logger, Exception exception);
}