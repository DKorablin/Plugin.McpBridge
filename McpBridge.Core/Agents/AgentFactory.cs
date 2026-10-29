using System.ClientModel;
using System.ClientModel.Primitives;
using System.Diagnostics;
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
	private readonly IMcpTrace? _trace;

	/// <param name="trace">Optional trace sink that receives a verbose summary of the features attached to every created agent.</param>
	public AgentFactory(IMcpTrace? trace = null)
		=> this._trace = trace;

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

			this.TraceAgentFeatures(agentRole, provider, tools, null, contextProviders);
			return AgentHandle.FromCopilotClient(copilotClient.AsAIAgent(sessionConfig), copilotClient);
		default:
			IChatClient chatClient = AgentFactory.CreateChatClient(provider);
			Boolean isDebugMode = false;
			String? evaluationCacheDirectory = null;
			if(provider is NetworkProviderDto networkProvider && networkProvider.EvaluationCacheEnabled)
			{
				isDebugMode = true;
				evaluationCacheDirectory = AgentFactory.GetEvaluationCacheDirectory(networkProvider, agentRole);
				chatClient = await AgentFactory.ConfigureEvaluationCache(chatClient, networkProvider, evaluationCacheDirectory);
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

			this.TraceAgentFeatures(agentRole, provider, tools, evaluationCacheDirectory, contextProviders);
			return AgentHandle.FromChatClient(chatClient.AsAIAgent(options1), chatClient, isDebugMode);
		}
	}

	/// <summary>Writes a verbose summary of the features (tools, evaluation cache, chat options, context providers) attached to a created agent.</summary>
	private void TraceAgentFeatures(String agentRole, AiProviderDto provider, AIFunction[] tools, String? evaluationCacheDirectory, IEnumerable<AIContextProvider>? contextProviders)
	{
		if(this._trace == null)
			return;

		String[] contextProviderNames = contextProviders?.Select(p => p.GetType().Name).ToArray() ?? [];
		List<String> lines = new List<String>
		{
			$"Agent '{agentRole}' created. Provider: {provider} ({provider.ProviderType})",
			$"\tTools ({tools.Length}): {(tools.Length == 0 ? "<none>" : String.Join(", ", tools.Select(t => t.Name)))}",
		};

		if(provider.ProviderType == AiProviderType.CoPilot)
		{
			if(contextProviderNames.Length > 0)
				lines.Add($"\tContext providers: NOT ATTACHED (not supported by {AiProviderType.CoPilot}): {String.Join(", ", contextProviderNames)}");
		} else
		{
			ChatSettings chat = provider.Chat;
			lines.Add($"\tEvaluation cache: {(evaluationCacheDirectory == null ? "disabled" : $"enabled ('{evaluationCacheDirectory}')")}");
			lines.Add($"\tChat options: MaxTokens={chat.MaxTokens?.ToString() ?? "<default>"}, Temperature={chat.Temperature?.ToString() ?? "<default>"}, "
				+ $"Reasoning={(chat.ReasoningOutput.HasValue || chat.ReasoningEffort.HasValue ? $"{chat.ReasoningOutput ?? ReasoningOutput.None}/{chat.ReasoningEffort ?? ReasoningEffort.Medium}" : "<default>")}");
			lines.Add($"\tContext providers: {(contextProviderNames.Length == 0 ? "<none>" : String.Join(", ", contextProviderNames))}");
		}

		this._trace.TraceEvent(TraceEventType.Verbose, 0, String.Join(Environment.NewLine, lines));
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
	private static async Task<IChatClient> ConfigureEvaluationCache(IChatClient chatClient, NetworkProviderDto provider, String storageRootPath)
	{
		if(!provider.EvaluationCacheEnabled)
			return chatClient;

		_ = Directory.CreateDirectory(storageRootPath);

		DiskBasedResponseCacheProvider cacheProvider = new DiskBasedResponseCacheProvider(storageRootPath);
		IDistributedCache cache = await cacheProvider.GetCacheAsync("interactive", provider.Id.ToString("N"));

		return new ChatClientBuilder(chatClient)
			.UseDistributedCache(cache)
			.Build();
	}

	private static String GetEvaluationCacheDirectory(NetworkProviderDto provider, String agentRole)
		=> String.IsNullOrWhiteSpace(provider.EvaluationCacheDirectory)
			? Utils.GetAgentStorageDirectory(Utils.SpecialDirectory.EvaluationCache, agentRole)
			: provider.EvaluationCacheDirectory;

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
			Boolean useSqliteStore = File.Exists(sqlitePath);
			if(useSqliteStore)
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

			this._trace?.TraceEvent(TraceEventType.Verbose, 0, $"RAG attached to agent '{agent.Id}'. Directory: '{agent.RagDirectory}'"
				+ $", Store: {(useSqliteStore ? $"SQLite ('{sqlitePath}')" : "InMemory (lazy indexing)")}"
				+ $", Embeddings: {embeddingProvider} (Dimension={embeddingProvider.Embeddings.Dimension}, TopResults={embeddingProvider.Embeddings.TopResults})"
				+ $", Extensions: {String.Join(", ", supportedExtensions)}, Tool: '{agent.RagToolName}'");

			providers.Add(new TextSearchProvider(async (text, ct) =>
				{
					try
					{
						return await textSearchStore.SearchTextAsync(text, ct);
					} catch(Exception exc)
					{
						this._trace?.TraceData(TraceEventType.Error, 0, exc);
						throw;
					}
				},
				new TextSearchProviderOptions
				{
					SearchTime = TextSearchProviderOptions.TextSearchBehavior.OnDemandFunctionCalling,
					FunctionToolName = agent.RagToolName,
					FunctionToolDescription = agent.RagToolDescription,
					CitationsPrompt = agent.RagCitationsPrompt,
				}));
		} else if(agent.RagDirectory != null)
			this._trace?.TraceEvent(TraceEventType.Warning, 0, $"RAG NOT attached to agent '{agent.Id}': directory '{agent.RagDirectory}' is set, but embedding provider '{embeddingProvider}' "
				+ $"does not support embeddings or has no Dimension/ModelId (Dimension={embeddingProvider.Embeddings?.Dimension?.ToString() ?? "<null>"}, ModelId={embeddingProvider.Embeddings?.ModelId ?? "<null>"}).");

		if(agent.SkillsDirectory != null)
		{
			providers.Add(new AgentSkillsProvider(agent.SkillsDirectory));
			this._trace?.TraceEvent(TraceEventType.Verbose, 0, $"Skills attached to agent '{agent.Id}'. Directory: '{agent.SkillsDirectory}'");
		}

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