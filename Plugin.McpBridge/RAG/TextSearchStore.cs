using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;

namespace Plugin.McpBridge.RAG;

public class TextSearchStore
{
	private readonly VectorStoreCollection<String, TextSearchRecord> _collection;
	public TextSearchStore(InMemoryVectorStore vectorStore, String collectionName, Int32 dimensions)
	{
		var definition = new VectorStoreCollectionDefinition
		{
			Properties =
			[
				new VectorStoreKeyProperty(nameof(TextSearchRecord.SourceId), typeof(String)),
				new VectorStoreDataProperty(nameof(TextSearchRecord.SourceName), typeof(String)),
				new VectorStoreDataProperty(nameof(TextSearchRecord.SourceLink), typeof(String)),
				new VectorStoreDataProperty(nameof(TextSearchRecord.Text), typeof(String)),
				new VectorStoreVectorProperty(nameof(TextSearchRecord.Embedding), typeof(String), dimensions),
			]
		};
		_collection = vectorStore.GetCollection<String, TextSearchRecord>(collectionName, definition);
	}
	public async Task UpsertDocumentsAsync(IEnumerable<TextSearchDocument> documents)
	{
		await _collection.EnsureCollectionExistsAsync();
		foreach(var doc in documents)
		{
			var record = new TextSearchRecord
			{
				SourceId = doc.SourceId,
				SourceName = doc.SourceName,
				SourceLink = doc.SourceLink,
				Text = doc.Text,
				Embedding = doc.Text
			};
			await _collection.UpsertAsync(record);
		}
	}
	public async Task<IEnumerable<TextSearchDocument>> SearchAsync(String query, Int32 topK, CancellationToken cancellationToken = default)
	{
		var results = _collection.SearchAsync(query, topK, cancellationToken: cancellationToken);
		var documents = new List<TextSearchDocument>();
		await foreach(var result in results)
		{
			documents.Add(new TextSearchDocument
			{
				SourceId = result.Record.SourceId,
				SourceName = result.Record.SourceName,
				SourceLink = result.Record.SourceLink,
				Text = result.Record.Text
			});
		}
		return documents;
	}

	public static void AssertDocumentsInFolder(String folderPath)
	{
		if(!Directory.Exists(folderPath))
			throw new DirectoryNotFoundException($"The specified RAG directory does not exist: {folderPath}");

		var extensions = new[] { ".txt", ".md" };
		if(!Directory.EnumerateFiles(folderPath, "*.*", SearchOption.AllDirectories)
			.Any(file => extensions.Any(ext => file.EndsWith(ext, StringComparison.OrdinalIgnoreCase))))
			throw new DirectoryNotFoundException($"The specified folder '{folderPath}' does not exist or does not contain any supported document files (*.txt, *.md).");
	}

	public static IEnumerable<TextSearchDocument> GetDocumentsFromFolder(String folderPath)
	{
		foreach(String filePath in Directory.EnumerateFiles(folderPath, "*.*", SearchOption.AllDirectories)
			.Where(f => f.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)
					 || f.EndsWith(".md", StringComparison.OrdinalIgnoreCase)))
		{
			yield return new TextSearchDocument
			{
				SourceId = Path.GetFileNameWithoutExtension(filePath),
				SourceName = Path.GetFileNameWithoutExtension(filePath),
				SourceLink = filePath,
				Text = File.ReadAllText(filePath),
			};
		}
	}
}