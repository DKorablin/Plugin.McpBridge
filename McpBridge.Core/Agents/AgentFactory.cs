using System.ClientModel;
using System.ClientModel.Primitives;
using Azure.AI.OpenAI;
using GitHub.Copilot;
using Microsoft.Agents.AI;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation.Reporting.Storage;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.SemanticKernel.Connectors.SqliteVec;
using OpenAI;
using McpBridge.Core.Data;
using McpBridge.Core.RAG;

namespace McpBridge.Core.Agents;

/// <summary>Shared factory methods for building AI agent components, usable by both the WinForms chat path and DevUI.</summary>
public class AgentFactory
{
	public async Task<AgentHandle> CreateAgent(
		AiAgentDto agent,
		IEnumerable<AiProviderDto> providers,
		AIFunction[] tools,
		String systemInstructions,
		String? agentRole = null,
		CancellationToken token = default)
	{
		var provider = agent.GetSelectedProvider(providers);
		IEnumerable<AiProviderDto> providerCandidates = providers;
		AiProviderDto embeddingProvider = agent.GetEmbeddingProvider(providerCandidates);
		IEnumerable<AIContextProvider>? contextProviders = await this.CreateContextProviders(agent, embeddingProvider);
		return await this.CreateAgent(provider, tools, systemInstructions, agentRole, contextProviders, token);
	}

	public virtual async Task<AgentHandle> CreateAgent(
		AiProviderDto provider,
		AIFunction[] tools,
		String systemInstructions,
		String? agentRole = null,
		IEnumerable<AIContextProvider>? contextProviders = null,
		CancellationToken token = default)
	{
		_ = provider ?? throw new ArgumentNullException(nameof(provider));
		agentRole = agentRole ?? "assistant";

		switch(provider.ProviderType)
		{
		case AiProviderType.CoPilot:
			CoPilotConnectionSettings copilotConnection = GetCoPilotConnection(provider);
			CopilotClientOptions options = new CopilotClientOptions()
			{
				Connection = RuntimeConnection.ForStdio(copilotConnection.CoPilotPath),
				GitHubToken = copilotConnection.GitHubToken,
				//UseLoggedInUser = true,
			};
			CopilotClient copilotClient = new CopilotClient(options);

			await copilotClient.StartAsync(token);
			var sessionConfig = new SessionConfig
			{
				OnPermissionRequest = PermissionHandler.ApproveAll,
				Tools = tools,
				SystemMessage = new SystemMessageConfig()
				{
					Mode = SystemMessageMode.Append,
					Content = systemInstructions,
				},
			};

			return AgentHandle.FromCopilotClient(copilotClient.AsAIAgent(sessionConfig), copilotClient);
		default:
			IChatClient chatClient = AgentFactory.CreateChatClient(provider);
			Boolean isDebugMode = false;
			if(provider is NetworkProviderDto networkProvider && networkProvider.EvaluationCacheEnabled)
			{
				isDebugMode = true;
				chatClient = await AgentFactory.ConfigureEvaluationCache(chatClient, networkProvider, agentRole);
			}

			var options1 = new ChatClientAgentOptions
			{
				//"For in-memory agents, this defaults to a randomly-generated ID" — and the base class generates it as {Name}_{randomHex} so it's human-readable while still unique.
				//But in case when randomly generated ID is used, workflow agents can't recover their sessions.
				Id = agentRole,
				Name = agentRole,
				ChatOptions = new ChatOptions()
				{
					Tools = tools,
					Instructions = systemInstructions,
				},
			};

			options1.AIContextProviders = contextProviders;

			return AgentHandle.FromChatClient(chatClient.AsAIAgent(options1), chatClient, isDebugMode);
		}
	}

	/// <summary>Creates a raw <see cref="IChatClient"/> for the given provider using the supplied <see cref="HttpClient"/>.</summary>
	private static IChatClient CreateChatClient(AiProviderDto providerSettings)
	{
		ValidateChatProvider(providerSettings);

		IChatClient chatClient;

		switch(providerSettings.ProviderType)
		{
		case AiProviderType.Stub:
			chatClient = new StubChatClient();
			break;
		case AiProviderType.Azure:
			NetworkConnectionSettings azureConnection = GetNetworkConnection(providerSettings);

			var httpClient1 = new HttpClient { Timeout = azureConnection.Timeout };
			HttpClientPipelineTransport transport1 = new HttpClientPipelineTransport(httpClient1);

			chatClient = new AzureOpenAIClient(
				new Uri(azureConnection.EndpointUrl!),
				new ApiKeyCredential(azureConnection.ApiKey!),
				new AzureOpenAIClientOptions {
					Transport = transport1,
					RetryPolicy = new ClientRetryPolicy(maxRetries: 6),
				})
				.GetChatClient(providerSettings.Chat.ModelId!)
				.AsIChatClient();
			break;
		default:
			NetworkConnectionSettings networkConnection = GetNetworkConnection(providerSettings);

			var httpClient2 = new HttpClient { Timeout = networkConnection.Timeout };
			HttpClientPipelineTransport transport2 = new HttpClientPipelineTransport(httpClient2);

			OpenAIClientOptions clientOptions = new OpenAIClientOptions { Transport = transport2 };
			if(networkConnection.EndpointUrl != null)
				clientOptions.Endpoint = new Uri(networkConnection.EndpointUrl);

			chatClient = new OpenAIClient(new ApiKeyCredential(networkConnection.ApiKey ?? "local-no-key"), clientOptions)
				.GetChatClient(providerSettings.Chat.ModelId!)
				.AsIChatClient();
			break;
		}
		return ConfigureChatOptions(chatClient, providerSettings.Chat);
	}

	private static void ValidateChatProvider(AiProviderDto providerSettings)
	{
		String? validationError = providerSettings.GetValidationError(ProviderCapabilities.Chat);
		if(validationError != null)
			throw new InvalidOperationException($"Provider '{providerSettings}' is invalid for chat: {validationError}");
	}

	/// <summary>Wraps a <see cref="IChatClient"/> with a disk-backed response cache when <see cref="NetworkProviderDto.EvaluationCacheEnabled"/> is set.</summary>
	private static async Task<IChatClient> ConfigureEvaluationCache(IChatClient chatClient, NetworkProviderDto provider, String agentRole)
	{
		if(!provider.EvaluationCacheEnabled)
			return chatClient;

		String storageRootPath = String.IsNullOrWhiteSpace(provider.EvaluationCacheDirectory)
			? Utils.GetAgentStorageDirectory(Utils.SpecialDirectory.EvaluationCache, agentRole)
			: provider.EvaluationCacheDirectory;

		_ = Directory.CreateDirectory(storageRootPath);

		DiskBasedResponseCacheProvider cacheProvider = new DiskBasedResponseCacheProvider(storageRootPath);
		IDistributedCache cache = await cacheProvider.GetCacheAsync("interactive", provider.Id.ToString("N"));

		return new ChatClientBuilder(chatClient)
			.UseDistributedCache(cache)
			.Build();
	}

	/// <summary>Wraps a <see cref="IChatClient"/> with token, temperature, and reasoning options from <paramref name="settings"/> and <paramref name="settings"/>.</summary>
	private static IChatClient ConfigureChatOptions(IChatClient chatClient, ChatSettings settings)
		=> new ChatClientBuilder(chatClient)
			.ConfigureOptions(options =>
			{
				if(settings.MaxTokens.HasValue)
					options.MaxOutputTokens = settings.MaxTokens.Value;
				if(settings.Temperature.HasValue)
					options.Temperature = (Single)settings.Temperature.Value;
				if(settings.ReasoningOutput.HasValue || settings.ReasoningEffort.HasValue)
					options.Reasoning = new ReasoningOptions
					{
						Output = settings.ReasoningOutput ?? ReasoningOutput.None,
						Effort = settings.ReasoningEffort ?? ReasoningEffort.Medium
					};
			})
			.Build();

	private async Task<AIContextProvider[]?> CreateContextProviders(AiAgentDto agent, AiProviderDto embeddingProvider)
	{
		List<AIContextProvider> providers = new List<AIContextProvider>();

		if(agent.RagDirectory != null
			&& embeddingProvider.SupportsCapability(ProviderCapabilities.Embeddings)
			&& embeddingProvider.Embeddings.Dimension != null
			&& !String.IsNullOrWhiteSpace(embeddingProvider.Embeddings.ModelId))
		{
			String[] supportedExtensions = agent.RagSupportedExtensions;
			TextSearchStore.AssertDocumentsInFolder(agent.RagDirectory, supportedExtensions);

			IEmbeddingGenerator<String, Embedding<Single>> embeddingGenerator = AgentFactory.CreateEmbeddingGenerator(embeddingProvider);
			TextSearchStore textSearchStore;
			String sqlitePath = TextSearchStore.GetSqliteDatabasePath(agent.RagDirectory, agent.Id);
			if(File.Exists(sqlitePath))
			{
				String connectionString = new SqliteConnectionStringBuilder { DataSource = sqlitePath }.ToString();
				SqliteVectorStore sqliteStore = new SqliteVectorStore(connectionString, new SqliteVectorStoreOptions
				{
					EmbeddingGenerator = embeddingGenerator,
				});
				textSearchStore = new TextSearchStore(sqliteStore, TextSearchStore.DefaultCollectionName, embeddingProvider.Embeddings.Dimension.Value, embeddingProvider.Embeddings.TopResults, supportedExtensions);
				await textSearchStore.EnsureCollectionExistsAsync();
			} else
			{
				InMemoryVectorStore vectorStore = new InMemoryVectorStore(new() { EmbeddingGenerator = embeddingGenerator });
				textSearchStore = new TextSearchLazyStore(vectorStore, TextSearchStore.DefaultCollectionName, embeddingProvider.Embeddings.Dimension.Value, agent.RagDirectory, supportedExtensions, embeddingProvider.Embeddings.TopResults);
			}

			providers.Add(new TextSearchProvider((text, ct) => textSearchStore.SearchTextAsync(text, ct), new TextSearchProviderOptions
			{
				SearchTime = TextSearchProviderOptions.TextSearchBehavior.OnDemandFunctionCalling,
				FunctionToolName = agent.RagToolName,
				FunctionToolDescription = agent.RagToolDescription,
				CitationsPrompt = agent.RagCitationsPrompt,
			}));
		}
		if(agent.SkillsDirectory != null)
			providers.Add(new AgentSkillsProvider(agent.SkillsDirectory));

		return providers.Count == 0 ? null : providers.ToArray();
	}

	/// <summary>Creates an <see cref="IEmbeddingGenerator{TInput,TEmbedding}"/> for the given provider using <see cref="EmbeddingSettings.ModelId"/>.</summary>
	public static IEmbeddingGenerator<String, Embedding<Single>> CreateEmbeddingGenerator(AiProviderDto providerSettings)
	{
		ValidateEmbeddingProvider(providerSettings);

		switch(providerSettings.ProviderType)
		{
		case AiProviderType.Azure:
			NetworkConnectionSettings azureConnection = GetNetworkConnection(providerSettings);

			var httpClient1 = new HttpClient { Timeout = azureConnection.Timeout };
			HttpClientPipelineTransport transport1 = new HttpClientPipelineTransport(httpClient1);

			return new AzureOpenAIClient(
				new Uri(azureConnection.EndpointUrl!),
				new ApiKeyCredential(azureConnection.ApiKey!),
				new AzureOpenAIClientOptions {
					Transport = transport1,
					RetryPolicy = new ClientRetryPolicy(maxRetries: 6),
				})
				.GetEmbeddingClient(providerSettings.Embeddings.ModelId!)
				.AsIEmbeddingGenerator();
		default:
			NetworkConnectionSettings networkConnection = GetNetworkConnection(providerSettings);

			var httpClient2 = new HttpClient { Timeout = networkConnection.Timeout };
			HttpClientPipelineTransport transport2 = new HttpClientPipelineTransport(httpClient2);

			OpenAIClientOptions clientOptions = new OpenAIClientOptions { Transport = transport2 };
			if(networkConnection.EndpointUrl != null)
				clientOptions.Endpoint = new Uri(networkConnection.EndpointUrl);

			return new OpenAIClient(new ApiKeyCredential(networkConnection.ApiKey ?? "local-no-key"), clientOptions)
				.GetEmbeddingClient(providerSettings.Embeddings.ModelId!)
				.AsIEmbeddingGenerator();
		}
	}

	private static void ValidateEmbeddingProvider(AiProviderDto providerSettings)
	{
		String? validationError = providerSettings.GetValidationError(ProviderCapabilities.Embeddings);
		if(validationError != null)
			throw new InvalidOperationException($"Provider '{providerSettings}' is invalid for embeddings: {validationError}");
	}

	private static NetworkConnectionSettings GetNetworkConnection(AiProviderDto providerSettings)
		=> providerSettings.Connection as NetworkConnectionSettings
			?? throw new InvalidOperationException($"Provider '{providerSettings}' requires network connection settings.");

	private static CoPilotConnectionSettings GetCoPilotConnection(AiProviderDto providerSettings)
		=> providerSettings.Connection as CoPilotConnectionSettings
			?? throw new InvalidOperationException($"Provider '{providerSettings}' requires Copilot connection settings.");

}