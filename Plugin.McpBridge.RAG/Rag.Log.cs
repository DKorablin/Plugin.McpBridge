using McpBridge.Shared.Logging;
using Microsoft.Extensions.Logging;

namespace Plugin.McpBridge.RAG;

internal static partial class RagLog
{
	private const Int32 LogId = LogAppIDs.Rag;

	public enum ActionType
	{
		Added,
		Unchanged,
		MetadataUpdated,
		Updated,
	}

	[LoggerMessage(EventId = LogId, Level = LogLevel.Information, Message = "Starting full RAG synchronization for {AgentsCount} agent(s).")]
	public static partial void SyncAllStarted(this ILogger logger, Int32 agentsCount);

	[LoggerMessage(EventId = LogId + 1, Level = LogLevel.Warning, Message = "RAG directory '{RagDirectory}' does not exist for agent '{AgentId}'.")]
	public static partial void RagDirectoryNotFound(this ILogger logger, String ragDirectory, Guid agentId);

	[LoggerMessage(EventId = LogId + 2, Message = "RAG sync processed file '{sourcePath}' ({sourceId}): action={actionType}")]
	public static partial void FileProcessed(this ILogger logger, LogLevel level, String sourcePath, String sourceId, ActionType actionType);

	[LoggerMessage(EventId = LogId + 3, Level = LogLevel.Information, Message = "RAG sync processed file '{SourceId}': action=Removed")]
	public static partial void FileRemoved(this ILogger logger, String sourceId);

	[LoggerMessage(EventId = LogId + 4, Level = LogLevel.Information, Message = "RAG index synchronized for '{RagDirectory}' -> '{SqlitePath}'. Added/updated: {AddedUpdatedCount}, removed: {RemovedCount}")]
	public static partial void SyncCompleted(this ILogger logger, String ragDirectory, String sqlitePath, Int32 addedUpdatedCount, Int32 removedCount);

	[LoggerMessage(EventId = LogId + 5, Level = LogLevel.Information, Message = "Attached {WatchersCount} file watcher(s) for RAG directories.")]
	public static partial void SyncCoordinatorWatchersAttached(this ILogger logger, Int32 watchersCount);

	[LoggerMessage(EventId = LogId + 6, Level = LogLevel.Debug, Message = "Disposing RAG file sync coordinator.")]
	public static partial void SyncCoordinatorDisposing(this ILogger logger);

	[LoggerMessage(EventId = LogId + 7, Level = LogLevel.Error, Message = "RAG sync failed for agent '{AgentId}'.")]
	public static partial void SyncCoordinatorFailed(this ILogger logger, Guid agentId, Exception exc);

	[LoggerMessage(EventId = LogId + 8, Level = LogLevel.Information, Message = "RAG sync hosted service started.")]
	public static partial void SyncServiceStarted(this ILogger logger);

	[LoggerMessage(EventId = LogId + 9, Level = LogLevel.Information, Message = "RAG folder watchers attached.")]
	public static partial void SyncServiceFolderWatchersAttached(this ILogger logger);

	[LoggerMessage(EventId = LogId + 10, Level = LogLevel.Information, Message = "RAG sync hosted service stopped.")]
	public static partial void SyncServiceStopped(this ILogger logger);
}