using McpBridge.Shared.Logging;
using Microsoft.Extensions.Logging;

namespace Plugin.McpBridge;

internal static partial class PluginLog
{
	private const Int32 LogId = LogAppIDs.Plugin;

	[LoggerMessage(EventId = LogId, Level = LogLevel.Debug, Message = "Initialized workflow AssistantAgent for '{WorkflowName}'.")]
	public static partial void WorkflowAgentInitialized(this ILogger logger, String workflowName);
}