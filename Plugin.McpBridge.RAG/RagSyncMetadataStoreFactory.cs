namespace Plugin.McpBridge.RAGHost;

internal sealed class RagSyncMetadataStoreFactory
{
	public RagSyncMetadataStore Create(String connectionString)
		=> new RagSyncMetadataStore(connectionString);
}
