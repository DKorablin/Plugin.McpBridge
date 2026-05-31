using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Plugin.McpBridge.Agents;
using Plugin.McpBridge.Data;
using Plugin.McpBridge.Tools;
using SAL.Flatbed;

namespace Plugin.McpBridge.RAG;

internal class IronMindRagAgent : AssistantAgent
{
	private const String CollectionName = "iron-mind-ai-tips";
	private TextSearchStore? _textSearchStore;

	public IronMindRagAgent(ITraceSource trace, IHost host, ToolsFactory toolsFactory, AgentFactory agentFactory)
		: base(trace, host, toolsFactory, agentFactory) { }

	protected override async Task<AgentHandle> CreateAgent(
		AiProviderDto provider,
		AIFunction[] tools,
		String instructions,
		Settings settings,
		IEnumerable<AIContextProvider>? contextProviders = null,
		CancellationToken token = default)
	{
		// set up vector store from provider on each Initialize call
		var networkSettings = (NetworkProviderDto)provider;
		var vectorStore = new InMemoryVectorStore(new() { EmbeddingGenerator = AgentFactory.CreateEmbeddingGenerator(provider) });
		this._textSearchStore = new TextSearchStore(vectorStore, CollectionName, networkSettings.EmbeddingModelDimention!.Value);
		await this._textSearchStore.UpsertDocumentsAsync(TextSearchStore.GetSampleDocuments());

		var textSearchOptions = new TextSearchProviderOptions
		{
			SearchTime = TextSearchProviderOptions.TextSearchBehavior.BeforeAIInvoke,
			CitationsPrompt = "Always cite sources at the end of your response using the format: " +
				"**Source:** [SourceName](SourceLink)",
		};

		return await base.CreateAgent(provider, tools,
			"You are Iron Mind AI, a knowledgeable personal trainer. " +
			"You MUST base your answers on the provided context documents. " +
			"Always cite your sources by name and link at the end of your response. " +
			"If the context does not contain relevant information, say so.",
			settings,
			contextProviders: new[] { new TextSearchProvider(SearchAsync, textSearchOptions) },
			token: token);  // hardcoded instructions override settings
	}

	private async Task<IEnumerable<TextSearchProvider.TextSearchResult>> SearchAsync(String text, CancellationToken ct)
	{
		var searchResults = await _textSearchStore.SearchAsync(text, 2, ct);
		return searchResults.Select(r => new TextSearchProvider.TextSearchResult
		{
			SourceName = r.SourceName,
			SourceLink = r.SourceLink,
			Text = r.Text,
			RawRepresentation = r
		});
	}
}