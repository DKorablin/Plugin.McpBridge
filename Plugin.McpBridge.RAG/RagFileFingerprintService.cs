using McpBridge.Core.RAG;

namespace Plugin.McpBridge.RAGHost;

internal sealed class RagFileFingerprintService
{
	public Dictionary<String, SyncMetadataEntry> BuildSnapshot(String ragDirectory, IEnumerable<String>? supportedExtensions)
	{
		Dictionary<String, SyncMetadataEntry> snapshot = new Dictionary<String, SyncMetadataEntry>(StringComparer.OrdinalIgnoreCase);
		foreach(String filePath in TextSearchStore.GetDocumentFilesFromFolder(ragDirectory, supportedExtensions))
		{
			FileInfo file = new FileInfo(filePath);
			String sourceId = TextSearchStore.GetSourceId(ragDirectory, filePath);
			snapshot[sourceId] = new SyncMetadataEntry
			{
				SourceId = sourceId,
				SourceLink = filePath,
				LastWriteUtcTicks = file.LastWriteTimeUtc.Ticks,
				Length = file.Length,
			};
		}
		return snapshot;
	}
}