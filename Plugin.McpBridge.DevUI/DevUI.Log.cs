using McpBridge.Shared.Logging;
using Microsoft.Extensions.Logging;

namespace Plugin.McpBridge.DevUI;

internal static partial class DevUILog
{
	private const Int32 LogId = LogAppIDs.DevUI;

	[LoggerMessage(EventId = LogId, Level = LogLevel.Information, Message = "Bridge connected: {toolsCount:N0} tools loaded from {mcpServerUrl}")]
	public static partial void McpBridgeConnected(this ILogger logger, Int32 toolsCount, String mcpServerUrl);

	[LoggerMessage(EventId = LogId + 1, Level = LogLevel.Information, Message = "Loading workflow from {workflowFile}")]
	public static partial void LoadingWorkflow(this ILogger logger, String workflowFile);

	[LoggerMessage(EventId = LogId + 2, Level = LogLevel.Information,Message = "DevUI running at {devUiUrl}/devui")]
	public static partial void DevUIStarted(this ILogger logger, String devUiUrl);
}