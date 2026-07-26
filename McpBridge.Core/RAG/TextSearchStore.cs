using System.Runtime.InteropServices;
using Microsoft.Agents.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.SemanticKernel.Connectors.SqliteVec;
using Plugin.McpBridge.Agents;
using Plugin.McpBridge.Data;

namespace Plugin.McpBridge.RAG;

public class TextSearchStore
{
	public const String DefaultCollectionName = "rag-kb";
	private static Int32 _sqliteBundleInitialized;

	private static void EnsureSqliteBundleInitialized()
	{
		if(System.Threading.Interlocked.Exchange(ref _sqliteBundleInitialized, 1) != 0)
			return;

		TextSearchStore.LoadNativeSqliteLibraryIfNeeded();
		SQLitePCL.Batteries.Init();
	}

	private static void LoadNativeSqliteLibraryIfNeeded()
	{
		if(!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			return;

		String? architectureFolder = RuntimeInformation.ProcessArchitecture switch
		{
			Architecture.X64 => "win-x64",
			Architecture.X86 => "win-x86",
			Architecture.Arm64 => "win-arm64",
			Architecture.Arm => "win-arm",
			_ => null,
		};
		if(architectureFolder == null)
			return;

		String[] probeRoots =
		[
			AppContext.BaseDirectory,
			Path.GetDirectoryName(typeof(TextSearchStore).Assembly.Location) ?? AppContext.BaseDirectory,
		];
		foreach(String probeRoot in probeRoots.Distinct(StringComparer.OrdinalIgnoreCase))
		{
			String nativeDirectory = Path.Combine(probeRoot, "runtimes", architectureFolder, "native");
			if(!Directory.Exists(nativeDirectory))
				continue;

			TextSearchStore.TryLoadNativeLibrary(Path.Combine(nativeDirectory, "e_sqlite3.dll"));
			TextSearchStore.TryLoadNativeLibrary(Path.Combine(nativeDirectory, "vec0.dll"));
		}
	}

	private static void TryLoadNativeLibrary(String nativeLibraryPath)
	{
		if(!File.Exists(nativeLibraryPath))
			return;

		_ = NativeLibrary.TryLoad(nativeLibraryPath, out _);
	}

	private readonly VectorStoreCollection<String, TextSearchRecord> _collection;
	private readonly UInt16 _topK;
	private readonly String[] _supportedExtensions;

	public TextSearchStore(InMemoryVectorStore vectorStore, String collectionName, Int32 dimensions, UInt16 topK = 3, IEnumerable<String>? supportedExtensions = null)
		: this((name, definition) => vectorStore.GetCollection<String, TextSearchRecord>(name, definition), collectionName, dimensions, topK, supportedExtensions)
	{
	}

	public TextSearchStore(SqliteVectorStore vectorStore, String collectionName, Int32 dimensions, UInt16 topK = 3, IEnumerable<String>? supportedExtensions = null)
		: this((name, definition) => vectorStore.GetCollection<String, TextSearchRecord>(name, definition), collectionName, dimensions, topK, supportedExtensions)
		=> TextSearchStore.EnsureSqliteBundleInitialized();

	private TextSearchStore(Func<String, VectorStoreCollectionDefinition, VectorStoreCollection<String, TextSearchRecord>> collectionFactory, String collectionName, Int32 dimensions, UInt16 topK = 3, IEnumerable<String>? supportedExtensions = null)
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
		this._supportedExtensions = AiAgentDto.NormalizeRagSupportedExtensions(supportedExtensions);
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

	public virtual async Task<IEnumerable<TextSearchDocument>> SearchAsync(String query, Int32 topK, CancellationToken cancellationToken = default)
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
		=> Path.Combine(ragDirectory, $"rag-{agentId:N}.sqlite");

	public static Boolean IsSupportedDocumentPath(String filePath, IEnumerable<String>? supportedExtensions = null)
	{
		String extenstion = Path.GetExtension(filePath).ToLowerInvariant();
		String[] normalizedExtensions = AiAgentDto.NormalizeRagSupportedExtensions(supportedExtensions);
		return Array.Exists(normalizedExtensions, ext => ext == extenstion);
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

	public static IEnumerable<String> GetDocumentFilesFromFolder(String folderPath, IEnumerable<String>? supportedExtensions = null)
	{
		HashSet<String> normalizedExtensions = new HashSet<String>(AiAgentDto.NormalizeRagSupportedExtensions(supportedExtensions), StringComparer.OrdinalIgnoreCase);
		return Directory.EnumerateFiles(folderPath, "*.*", SearchOption.AllDirectories)
			.Where(filePath => normalizedExtensions.Contains(Path.GetExtension(filePath).ToLowerInvariant()));
	}

	public IEnumerable<String> GetDocumentFilesFromFolder(String folderPath)
		=> Directory.EnumerateFiles(folderPath, "*.*", SearchOption.AllDirectories)
			.Where(filePath => TextSearchStore.IsSupportedDocumentPath(filePath, this._supportedExtensions));

	public static void AssertDocumentsInFolder(String folderPath, IEnumerable<String>? supportedExtensions = null)
	{
		if(!Directory.Exists(folderPath))
			throw new DirectoryNotFoundException($"The specified RAG directory does not exist: {folderPath}");

		String[] normalizedExtensions = AiAgentDto.NormalizeRagSupportedExtensions(supportedExtensions);
		if(!GetDocumentFilesFromFolder(folderPath, normalizedExtensions).Any())
			throw new DirectoryNotFoundException($"The specified folder '{folderPath}' does not contain any supported document files ({String.Join(", ", normalizedExtensions)}).");
	}

	public static IEnumerable<TextSearchDocument> GetDocumentsFromFolder(String folderPath, IEnumerable<String>? supportedExtensions = null)
	{
		foreach(String filePath in GetDocumentFilesFromFolder(folderPath, supportedExtensions))
			yield return CreateDocument(folderPath, filePath);
	}

	public IEnumerable<TextSearchDocument> GetDocumentsFromFolder(String folderPath)
		=> TextSearchStore.GetDocumentsFromFolder(folderPath, this._supportedExtensions);
}