using McpBridge.Shared.Logging;

namespace Plugin.McpBridge.AgUI;

internal static partial class AgUILog
{
	private const Int32 LogId = LogAppIDs.AgUI;

	[LoggerMessage(EventId = LogId, Level = LogLevel.Information, Message = "Bridge connected: {toolsCount:N0} tools loaded from {mcpServerUrl}")]
	public static partial void McpBridgeConnected(this ILogger logger, Int32 toolsCount, String mcpServerUrl);

	[LoggerMessage(EventId = LogId + 1, Level = LogLevel.Information, Message = "Loading workflow from {workflowFile}")]
	public static partial void LoadingWorkflow(this ILogger logger, String workflowFile);

	[LoggerMessage(EventId = LogId + 2, Level = LogLevel.Information, Message = "AG-UI running at {agUiUrl}/agui")]
	public static partial void AgUIStarted(this ILogger logger, String agUiUrl);
}