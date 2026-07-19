using Microsoft.SemanticKernel.Connectors.InMemory;
using Plugin.McpBridge.Data;

namespace Plugin.McpBridge.RAG;

internal sealed class TextSearchLazyStore : TextSearchStore
{
	private readonly SemaphoreSlim _lazyLoadLock = new SemaphoreSlim(1, 1);
	private readonly String _ragDirectory;
	private readonly String[] _supportedExtensions;
	private Boolean _lazyLoadDocuments = true;

	public TextSearchLazyStore(InMemoryVectorStore vectorStore, String collectionName, Int32 dimensions, String ragDirectory, IEnumerable<String>? supportedExtensions = null, UInt16 topK = 3)
		: base(vectorStore, collectionName, dimensions, topK, supportedExtensions)
	{
		this._ragDirectory = ragDirectory;
		this._supportedExtensions = AiAgentDto.NormalizeRagSupportedExtensions(supportedExtensions);
	}

	public override async Task<IEnumerable<TextSearchDocument>> SearchAsync(String query, Int32 topK, CancellationToken cancellationToken = default)
	{
		await this.EnsureDocumentsLoadedAsync(cancellationToken);
		return await base.SearchAsync(query, topK, cancellationToken);
	}

	private async Task EnsureDocumentsLoadedAsync(CancellationToken cancellationToken)
	{
		if(!this._lazyLoadDocuments)
			return;

		await this._lazyLoadLock.WaitAsync(cancellationToken);
		try
		{
			if(!this._lazyLoadDocuments)
				return;

			IEnumerable<TextSearchDocument> documents = TextSearchStore.GetDocumentsFromFolder(this._ragDirectory, this._supportedExtensions);
			await this.UpsertDocumentsAsync(documents, cancellationToken);
			this._lazyLoadDocuments = false;
		}
		finally
		{
			this._lazyLoadLock.Release();
		}
	}
}