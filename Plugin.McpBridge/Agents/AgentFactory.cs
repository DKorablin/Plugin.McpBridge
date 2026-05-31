using System.ClientModel;
using System.ClientModel.Primitives;
using Azure.AI.OpenAI;
using GitHub.Copilot.SDK;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using Plugin.McpBridge.Data;

namespace Plugin.McpBridge.Agents;

/// <summary>Shared factory methods for building AI agent components, usable by both the WinForms chat path and DevUI.</summary>
internal class AgentFactory
{
	public virtual async Task<AgentHandle> CreateAgent(
		AiProviderDto providerSettings,
		AIFunction[] tools,
		String systemInstructions,
		String? agentRole = null,
		String? skillsDirectory = null,
		IEnumerable<AIContextProvider>? contextProviders = null,
		CancellationToken token = default)
	{
		_ = providerSettings ?? throw new ArgumentNullException(nameof(providerSettings));

		switch(providerSettings.ProviderType)
		{
		case AiProviderType.CoPilot:
			CoPilotProviderDto copilotSettings = (CoPilotProviderDto)providerSettings;
			CopilotClientOptions options = new CopilotClientOptions()
			{
				CliPath = copilotSettings.CoPilotPath,
				GitHubToken = copilotSettings.GitHubToken,
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
			IChatClient chatClient = AgentFactory.CreateChatClient(providerSettings);
			var options1 = new ChatClientAgentOptions
			{
				//"For in-memory agents, this defaults to a randomly-generated ID" — and the base class generates it as {Name}_{randomHex} so it's human-readable while still unique.
				//Id = agentRole ?? "assistant",
				Name = agentRole ?? "assistant",
				ChatOptions = new ChatOptions()
				{
					Tools = tools,
					Instructions = systemInstructions,
				},
			};

			List<AIContextProvider> aiContextProviders = new List<AIContextProvider>();
			if(!String.IsNullOrWhiteSpace(skillsDirectory))
			{
				var skillsProvider = new AgentSkillsProvider(skillsDirectory);
				aiContextProviders.Add(skillsProvider);
			}

			if(contextProviders != null)
				aiContextProviders.AddRange(contextProviders);

			options1.AIContextProviders = aiContextProviders.Count == 0 ? null : aiContextProviders;

			return AgentHandle.FromChatClient(chatClient.AsAIAgent(options1), chatClient);
		}
	}

	/// <summary>Creates a raw <see cref="IChatClient"/> for the given provider using the supplied <see cref="HttpClient"/>.</summary>
	internal static IChatClient CreateChatClient(AiProviderDto providerSettings)
	{
		IChatClient chatClient;

		switch(providerSettings.ProviderType)
		{
		case AiProviderType.Stub:
			chatClient = new Tests.StubChatClient();
			break;
		case AiProviderType.Azure:
			var azureSettings = (AzureProviderDto)providerSettings;

			var httpClient1 = new HttpClient { Timeout = azureSettings.ConnectionTimeout };
			HttpClientPipelineTransport transport1 = new HttpClientPipelineTransport(httpClient1);

			chatClient = new AzureOpenAIClient(
				new Uri(providerSettings.ModelEndpointUrl!),
				new ApiKeyCredential(azureSettings.ApiKey!),
				new AzureOpenAIClientOptions { Transport = transport1, })
				.GetChatClient(azureSettings.DeploymentName)
				.AsIChatClient();
			break;
		default:
			var networkSettings = (NetworkProviderDto)providerSettings;

			var httpClient2 = new HttpClient { Timeout = networkSettings.ConnectionTimeout };
			HttpClientPipelineTransport transport2 = new HttpClientPipelineTransport(httpClient2);

			OpenAIClientOptions clientOptions = new OpenAIClientOptions { Transport = transport2 };
			if(networkSettings.ModelEndpointUrl != null)
				clientOptions.Endpoint = new Uri(networkSettings.ModelEndpointUrl);

			chatClient = new OpenAIClient(new ApiKeyCredential(networkSettings.ApiKey ?? "local-no-key"), clientOptions)
				.GetChatClient(networkSettings.ModelId)
				.AsIChatClient();
			break;
		}
		return ConfigureOptions(chatClient, providerSettings);
	}

	/// <summary>Wraps a <see cref="IChatClient"/> with token, temperature, and reasoning options from <paramref name="settings"/> and <paramref name="provider"/>.</summary>
	private static IChatClient ConfigureOptions(IChatClient chatClient, AiProviderDto provider)
		=> new ChatClientBuilder(chatClient)
			.ConfigureOptions(options =>
			{
				if(provider.MaxTokens.HasValue)
					options.MaxOutputTokens = provider.MaxTokens.Value;
				if(provider.Temperature.HasValue)
					options.Temperature = (Single)provider.Temperature.Value;
				if(provider.ReasoningOutput.HasValue || provider.ReasoningEffort.HasValue)
					options.Reasoning = new ReasoningOptions
					{
						Output = provider.ReasoningOutput ?? ReasoningOutput.None,
						Effort = provider.ReasoningEffort ?? ReasoningEffort.Medium
					};
			})
			.Build();

	/// <summary>Creates an <see cref="IEmbeddingGenerator{TInput,TEmbedding}"/> for the given provider using <see cref="NetworkProviderDto.EmbeddingModelId"/>.</summary>
	internal static IEmbeddingGenerator<String, Embedding<Single>> CreateEmbeddingGenerator(AiProviderDto providerSettings)
	{
		switch(providerSettings.ProviderType)
		{
		case AiProviderType.Azure:
			var azureSettings = (AzureProviderDto)providerSettings;

			var httpClient1 = new HttpClient { Timeout = azureSettings.ConnectionTimeout };
			HttpClientPipelineTransport transport1 = new HttpClientPipelineTransport(httpClient1);

			return new AzureOpenAIClient(
				new Uri(providerSettings.ModelEndpointUrl!),
				new ApiKeyCredential(azureSettings.ApiKey!),
				new AzureOpenAIClientOptions { Transport = transport1 })
				.GetEmbeddingClient(azureSettings.EmbeddingModelId!)
				.AsIEmbeddingGenerator();
		default:
			var networkSettings = (NetworkProviderDto)providerSettings;

			var httpClient2 = new HttpClient { Timeout = networkSettings.ConnectionTimeout };
			HttpClientPipelineTransport transport2 = new HttpClientPipelineTransport(httpClient2);

			OpenAIClientOptions clientOptions = new OpenAIClientOptions { Transport = transport2 };
			if(networkSettings.ModelEndpointUrl != null)
				clientOptions.Endpoint = new Uri(networkSettings.ModelEndpointUrl);

			return new OpenAIClient(new ApiKeyCredential(networkSettings.ApiKey ?? "local-no-key"), clientOptions)
				.GetEmbeddingClient(networkSettings.EmbeddingModelId!)
				.AsIEmbeddingGenerator();
		}
	}
}