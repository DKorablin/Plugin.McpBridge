using System.Security.Cryptography;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.SqliteVec;
using Plugin.McpBridge.Agents;
using Plugin.McpBridge.Core.Remoting;
using Plugin.McpBridge.Data;
using Plugin.McpBridge.RAG;

namespace Plugin.McpBridge.RAGHost;

internal sealed class RagIndexSyncService
{
	private readonly SettingsDto _settings;
	private readonly RagSyncMetadataStoreFactory _metadataStoreFactory;
	private readonly RagFileFingerprintService _fingerprintService;
	private readonly ILogger<RagIndexSyncService> _logger;

	public RagIndexSyncService(SettingsDto settings, RagSyncMetadataStoreFactory metadataStoreFactory, RagFileFingerprintService fingerprintService, ILogger<RagIndexSyncService> logger)
	{
		this._settings = settings;
		this._metadataStoreFactory = metadataStoreFactory;
		this._fingerprintService = fingerprintService;
		this._logger = logger;
	}

	public async Task SyncAllAsync(CancellationToken cancellationToken)
	{
		this._logger.LogInformation("Starting full RAG synchronization for {AgentsCount} agent(s).", this._settings.AiAgents.Count);
		foreach(AiAgentDto agent in this._settings.AiAgents)
			await this.SyncAgentAsync(agent, cancellationToken);
	}

	public async Task SyncAgentAsync(AiAgentDto agent, CancellationToken cancellationToken)
	{
		if(agent.RagDirectory == null)
			return;

		if(!Directory.Exists(agent.RagDirectory))
		{
			this._logger.LogWarning("RAG directory '{RagDirectory}' does not exist for agent '{AgentId}'.", agent.RagDirectory, agent.Id);
			return;
		}

		AiProviderDto provider = agent.GetEmbeddingProvider(this._settings.AiProviders);
		if(!provider.SupportsCapability(ProviderCapabilities.Embeddings)
			|| provider.Embeddings.Dimension == null
			|| String.IsNullOrWhiteSpace(provider.Embeddings.ModelId))
			return;

		String[] supportedExtensions = agent.RagSupportedExtensions;

		String sqlitePath = TextSearchStore.GetSqliteDatabasePath(agent.RagDirectory, agent.Id);
		Directory.CreateDirectory(Path.GetDirectoryName(sqlitePath)!);

		String connectionString = new SqliteConnectionStringBuilder { DataSource = sqlitePath }.ToString();
		RagSyncMetadataStore metadataStore = this._metadataStoreFactory.Create(connectionString);
		await metadataStore.EnsureSchemaAsync(cancellationToken);
		SqliteVectorStore vectorStore = new SqliteVectorStore(connectionString, new SqliteVectorStoreOptions
		{
			EmbeddingGenerator = AgentFactory.CreateEmbeddingGenerator(provider),
		});

		TextSearchStore searchStore = new TextSearchStore(vectorStore, TextSearchStore.DefaultCollectionName, provider.Embeddings.Dimension.Value, supportedExtensions: supportedExtensions);
		await searchStore.EnsureCollectionExistsAsync(cancellationToken);

		Dictionary<String, SyncMetadataEntry> existingMetadata = await metadataStore.LoadAsync(cancellationToken);
		Dictionary<String, SyncMetadataEntry> currentSnapshot = this._fingerprintService.BuildSnapshot(agent.RagDirectory, supportedExtensions);

		List<TextSearchDocument> upserts = new List<TextSearchDocument>();
		List<SyncMetadataEntry> metadataUpserts = new List<SyncMetadataEntry>();
		foreach(KeyValuePair<String, SyncMetadataEntry> current in currentSnapshot)
		{
			if(!existingMetadata.TryGetValue(current.Key, out SyncMetadataEntry? existing))
			{
				current.Value.SourceHash = ComputeFileHash(current.Value.SourceLink);
				TextSearchDocument document = TextSearchStore.CreateDocument(agent.RagDirectory, current.Value.SourceLink);
				upserts.Add(document);
				metadataUpserts.Add(current.Value);
				this._logger.LogInformation("RAG sync processed file '{SourcePath}' ({SourceId}): action=Added", current.Value.SourceLink, current.Key);
				continue;
			}

			Boolean sameWriteMarker = existing.LastWriteUtcTicks == current.Value.LastWriteUtcTicks
				&& existing.Length == current.Value.Length;
			if(sameWriteMarker && !String.IsNullOrWhiteSpace(existing.SourceHash))
			{
				this._logger.LogInformation("RAG sync processed file '{SourcePath}' ({SourceId}): action=Unchanged", current.Value.SourceLink, current.Key);
				continue;
			}

			String currentHash = ComputeFileHash(current.Value.SourceLink);
			current.Value.SourceHash = currentHash;

			if(String.IsNullOrWhiteSpace(existing.SourceHash) || String.Equals(existing.SourceHash, currentHash, StringComparison.OrdinalIgnoreCase))
			{
				metadataUpserts.Add(current.Value);
				this._logger.LogInformation("RAG sync processed file '{SourcePath}' ({SourceId}): action=MetadataUpdated", current.Value.SourceLink, current.Key);
				continue;
			}

			TextSearchDocument changedDocument = TextSearchStore.CreateDocument(agent.RagDirectory, current.Value.SourceLink);
			upserts.Add(changedDocument);
			metadataUpserts.Add(current.Value);
			this._logger.LogInformation("RAG sync processed file '{SourcePath}' ({SourceId}): action=Updated", current.Value.SourceLink, current.Key);
		}

		List<String> deletes = existingMetadata.Keys
			.Where(sourceId => !currentSnapshot.ContainsKey(sourceId))
			.ToList();

		foreach(String sourceId in deletes)
			this._logger.LogInformation("RAG sync processed file '{SourceId}': action=Removed", sourceId);

		if(upserts.Count > 0)
			await searchStore.UpsertDocumentsAsync(upserts, cancellationToken);
		if(deletes.Count > 0)
			await searchStore.DeleteDocumentsAsync(deletes, cancellationToken);
		if(metadataUpserts.Count > 0)
			await metadataStore.UpsertAsync(metadataUpserts, cancellationToken);
		if(deletes.Count > 0)
			await metadataStore.DeleteAsync(deletes, cancellationToken);

		this._logger.LogInformation("RAG index synchronized for '{RagDirectory}' -> '{SqlitePath}'. Added/updated: {AddedUpdatedCount}, removed: {RemovedCount}", agent.RagDirectory, sqlitePath, upserts.Count, deletes.Count);
	}

	private static String ComputeFileHash(String filePath)
	{
		using SHA256 sha256 = SHA256.Create();
		using FileStream stream = File.OpenRead(filePath);
		Byte[] hashBytes = sha256.ComputeHash(stream);
		return Convert.ToHexString(hashBytes);
	}
}