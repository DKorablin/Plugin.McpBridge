using Microsoft.Agents.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.SemanticKernel.Connectors.SqliteVec;

namespace Plugin.McpBridge.RAG;

public class TextSearchStore
{
	private static readonly String DefaultStoragePath = Path.Combine(AppContext.BaseDirectory, ".RagStore");
	private static readonly String[] SupportedExtensions = new String[] { ".txt", ".md" };
	public const String DefaultCollectionName = "rag-kb";

	private readonly VectorStoreCollection<String, TextSearchRecord> _collection;
	private readonly UInt16 _topK;

	public TextSearchStore(InMemoryVectorStore vectorStore, String collectionName, Int32 dimensions, UInt16 topK = 3)
		: this((name, definition) => vectorStore.GetCollection<String, TextSearchRecord>(name, definition), collectionName, dimensions, topK)
	{
	}

	public TextSearchStore(SqliteVectorStore vectorStore, String collectionName, Int32 dimensions, UInt16 topK = 3)
		: this((name, definition) => vectorStore.GetCollection<String, TextSearchRecord>(name, definition), collectionName, dimensions, topK)
	{
	}

	private TextSearchStore(Func<String, VectorStoreCollectionDefinition, VectorStoreCollection<String, TextSearchRecord>> collectionFactory, String collectionName, Int32 dimensions, UInt16 topK = 3)
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

		this._topK = topK;
		this._collection = collectionFactory(collectionName, definition);
	}

	public async Task UpsertDocumentsAsync(IEnumerable<TextSearchDocument> documents, CancellationToken cancellationToken = default)
	{
		await this._collection.EnsureCollectionExistsAsync(cancellationToken);
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
			await this._collection.UpsertAsync(record, cancellationToken);
		}
	}

	public async Task DeleteDocumentsAsync(IEnumerable<String> sourceIds, CancellationToken cancellationToken = default)
	{
		await this._collection.EnsureCollectionExistsAsync(cancellationToken);
		foreach(String sourceId in sourceIds)
			await this._collection.DeleteAsync(sourceId, cancellationToken);
	}

	public Task EnsureCollectionExistsAsync(CancellationToken cancellationToken = default)
		=> this._collection.EnsureCollectionExistsAsync(cancellationToken);

	public Task ResetCollectionAsync(CancellationToken cancellationToken = default)
		=> this._collection.EnsureCollectionDeletedAsync(cancellationToken);

	public async Task<IEnumerable<TextSearchDocument>> SearchAsync(String query, Int32 topK, CancellationToken cancellationToken = default)
	{
		var results = this._collection.SearchAsync(query, topK, cancellationToken: cancellationToken);
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

	public async Task<IEnumerable<TextSearchProvider.TextSearchResult>> SearchTextAsync(String query, CancellationToken cancellationToken = default)
	{
		IEnumerable<TextSearchDocument> results = await this.SearchAsync(query, this._topK, cancellationToken);
		return results.Select(r => new TextSearchProvider.TextSearchResult
		{
			SourceName = r.SourceName,
			SourceLink = r.SourceLink,
			Text = r.Text,
			RawRepresentation = r,
		});
	}

	public static String GetSqliteDatabasePath(String ragDirectory, Guid agentId)
		=> Path.Combine(DefaultStoragePath, $"rag-{agentId:N}.sqlite");

	public static Boolean IsSupportedDocumentPath(String filePath)
	{
		String extenstion = Path.GetExtension(filePath).ToLowerInvariant();
		return Array.Exists(SupportedExtensions, ext => ext == extenstion);
	}

	public static String GetSourceId(String ragDirectory, String filePath)
	{
		String relativePath = Path.GetRelativePath(ragDirectory, filePath);
		return relativePath.Replace(Path.DirectorySeparatorChar, '/').Replace(Path.AltDirectorySeparatorChar, '/');
	}

	public static TextSearchDocument CreateDocument(String ragDirectory, String filePath)
		=> new TextSearchDocument
		{
			SourceId = GetSourceId(ragDirectory, filePath),
			SourceName = Path.GetFileNameWithoutExtension(filePath),
			SourceLink = filePath,
			Text = File.ReadAllText(filePath),
		};

	public static IEnumerable<String> GetDocumentFilesFromFolder(String folderPath)
		=> Directory.EnumerateFiles(folderPath, "*.*", SearchOption.AllDirectories)
			.Where(TextSearchStore.IsSupportedDocumentPath);

	public static void AssertDocumentsInFolder(String folderPath)
	{
		if(!Directory.Exists(folderPath))
			throw new DirectoryNotFoundException($"The specified RAG directory does not exist: {folderPath}");

		if(!GetDocumentFilesFromFolder(folderPath).Any())
			throw new DirectoryNotFoundException($"The specified folder '{folderPath}' does not contain any supported document files (*.txt, *.md).");
	}

	public static IEnumerable<TextSearchDocument> GetDocumentsFromFolder(String folderPath)
	{
		foreach(String filePath in GetDocumentFilesFromFolder(folderPath))
			yield return CreateDocument(folderPath, filePath);
	}
}