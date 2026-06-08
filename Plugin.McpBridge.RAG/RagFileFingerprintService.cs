using System.Security.Cryptography;
using Plugin.McpBridge.RAG;

namespace Plugin.McpBridge.RAGHost;

internal sealed class RagFileFingerprintService
{
	public Dictionary<String, SyncMetadataEntry> BuildSnapshot(String ragDirectory)
	{
		Dictionary<String, SyncMetadataEntry> snapshot = new Dictionary<String, SyncMetadataEntry>(StringComparer.OrdinalIgnoreCase);
		foreach(String filePath in TextSearchStore.GetDocumentFilesFromFolder(ragDirectory))
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

	public String ComputeFileHash(String filePath)
	{
		using SHA256 sha256 = SHA256.Create();
		using FileStream stream = File.OpenRead(filePath);
		Byte[] hashBytes = sha256.ComputeHash(stream);
		return Convert.ToHexString(hashBytes);
	}
}
