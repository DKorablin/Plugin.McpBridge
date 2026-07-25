using Plugin.McpBridge.Data;
using Microsoft.Extensions.Logging;
using Plugin.McpBridge.RAG;
using Plugin.McpBridge.Core.Remoting;

namespace Plugin.McpBridge.RAGHost;

internal sealed class RagFileSyncCoordinator : IDisposable
{
	private readonly TimeSpan _debounceInterval;
	private readonly SettingsDto _settings;
	private readonly RagIndexSyncService _syncService;
	private readonly ILogger<RagFileSyncCoordinator> _logger;
	private readonly Dictionary<Guid, CancellationTokenSource> _pendingSync = new Dictionary<Guid, CancellationTokenSource>();
	private readonly List<FileSystemWatcher> _watchers = new List<FileSystemWatcher>();

	public RagFileSyncCoordinator(RagSyncOptions options, SettingsDto settings, RagIndexSyncService syncService, ILogger<RagFileSyncCoordinator> logger)
	{
		this._debounceInterval = options.DebounceInterval;
		this._settings = settings;
		this._syncService = syncService;
		this._logger = logger;
	}

	public void AttachFolderWatchers(CancellationToken lifetimeToken)
	{
		Int32 watchersCount = 0;
		foreach(AiAgentDto agent in this._settings.AiAgents)
		{
			if(agent.RagDirectory == null)
				continue;

			if(!Directory.Exists(agent.RagDirectory))
				continue;

			FileSystemWatcher watcher = new FileSystemWatcher(agent.RagDirectory)
			{
				IncludeSubdirectories = true,
				NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime,
				Filter = "*.*",
				EnableRaisingEvents = true,
			};

			watcher.Created += (_, e) => this.QueueSync(agent, e.FullPath, lifetimeToken);
			watcher.Changed += (_, e) => this.QueueSync(agent, e.FullPath, lifetimeToken);
			watcher.Deleted += (_, e) => this.QueueSync(agent, e.FullPath, lifetimeToken);
			watcher.Renamed += (_, e) => this.QueueSync(agent, e.FullPath, lifetimeToken);
			this._watchers.Add(watcher);
			watchersCount++;
		}
		this._logger.LogInformation("Attached {WatchersCount} file watcher(s) for RAG directories.", watchersCount);

		lifetimeToken.Register(this.Dispose);
	}

	public void Dispose()
	{
		this._logger.LogDebug("Disposing RAG file sync coordinator.");
		foreach(CancellationTokenSource cts in this._pendingSync.Values)
		{
			cts.Cancel();
			cts.Dispose();
		}
		this._pendingSync.Clear();

		foreach(FileSystemWatcher watcher in this._watchers)
			watcher.Dispose();
		this._watchers.Clear();
	}

	private void QueueSync(AiAgentDto agent, String changedPath, CancellationToken cancellationToken)
	{
		if(!TextSearchStore.IsSupportedDocumentPath(changedPath, agent.RagSupportedExtensions))
			return;

		lock(this._pendingSync)
		{
			if(this._pendingSync.TryGetValue(agent.Id, out CancellationTokenSource? previousCts))
			{
				previousCts.Cancel();
				previousCts.Dispose();
			}

			CancellationTokenSource debounceCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			this._pendingSync[agent.Id] = debounceCts;
			_ = Task.Run(async () =>
			{
				try
				{
					await Task.Delay(this._debounceInterval, debounceCts.Token);
					await this._syncService.SyncAgentAsync(agent, debounceCts.Token);
				}
				catch(OperationCanceledException)
				{
					// Debounced/canceled by subsequent events.
				}
				catch(Exception ex)
				{
					this._logger.LogError(ex, "RAG sync failed for agent {AgentId}.", agent.Id);
				}
				finally
				{
					lock(this._pendingSync)
						if(this._pendingSync.TryGetValue(agent.Id, out CancellationTokenSource? current) && Object.ReferenceEquals(current, debounceCts))
							this._pendingSync.Remove(agent.Id);
					debounceCts.Dispose();
				}
			}, debounceCts.Token);
		}
	}
}