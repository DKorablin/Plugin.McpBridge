using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Plugin.McpBridge.RAG;

namespace Plugin.McpBridge.RAGHost;

internal sealed class RagSyncHostedService : BackgroundService
{
	private readonly RagIndexSyncService _syncService;
	private readonly RagFileSyncCoordinator _fileSync;
	private readonly ILogger<RagSyncHostedService> _logger;

	public RagSyncHostedService(RagIndexSyncService syncService, RagFileSyncCoordinator fileSync, ILogger<RagSyncHostedService> logger)
	{
		this._syncService = syncService;
		this._fileSync = fileSync;
		this._logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		this._logger.SyncServiceStarted();
		await this._syncService.SyncAllAsync(stoppingToken);
		this._fileSync.AttachFolderWatchers(stoppingToken);
		this._logger.SyncServiceFolderWatchersAttached();

		try
		{
			await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
		}
		catch(OperationCanceledException)
		{
			this._logger.SyncServiceStopped();
		}
	}
}