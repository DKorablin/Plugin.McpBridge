using System;
using System.Linq;
using FluentAssertions;
using Plugin.McpBridge.Data;
using Xunit;

namespace Plugin.McpBridge.Tests;

public class AiAgentDtoTests
{
	[Fact]
	public void GetEmbeddingProvider_Should_FallBackToSelectedProvider()
	{
		NetworkProviderDto selectedProvider = new NetworkProviderDto()
		{
			ProviderType = AiProviderType.OpenAI,
			Chat = new ChatSettings() { ModelId = "gpt-4o-mini", },
			Embeddings = new EmbeddingSettings() { ModelId = "text-embedding-3-small", },
		};
		AiProviderDto[] providers = new AiProviderDto[]
		{
			selectedProvider,
			new StubProviderDto() { ProviderType = AiProviderType.Stub, Chat = new ChatSettings() { ModelId = "stub", }, },
		};
		AiAgentDto sut = new AiAgentDto()
		{
			SelectedProviderId = selectedProvider.Id,
		};

		AiProviderDto result = sut.GetEmbeddingProvider(providers);

		result.Should().BeSameAs(selectedProvider);
	}

	[Fact]
	public void GetEmbeddingProvider_Should_UseExplicitEmbeddingProvider()
	{
		NetworkProviderDto chatProvider = new NetworkProviderDto()
		{
			ProviderType = AiProviderType.OpenAI,
			Chat = new ChatSettings() { ModelId = "gpt-4o-mini", },
			Embeddings = new EmbeddingSettings() { ModelId = "text-embedding-3-small", },
		};
		NetworkProviderDto embeddingProvider = new NetworkProviderDto()
		{
			ProviderType = AiProviderType.OpenAI,
			Chat = new ChatSettings() { ModelId = "local-chat-model", },
			Embeddings = new EmbeddingSettings() { ModelId = "nomic-embed-text", },
		};
		AiProviderDto[] providers = new AiProviderDto[] { chatProvider, embeddingProvider };
		AiAgentDto sut = new AiAgentDto()
		{
			SelectedProviderId = chatProvider.Id,
			EmbeddingProviderId = embeddingProvider.Id,
		};

		AiProviderDto result = sut.GetEmbeddingProvider(providers);

		result.Should().BeSameAs(embeddingProvider);
		result.Id.Should().NotBe(sut.GetSelectedProvider(providers).Id);
	}

	[Fact]
	public void GetEmbeddingProvider_Should_FallBackToFirstProvider_WhenNoProviderIdsAreSet()
	{
		NetworkProviderDto firstProvider = new NetworkProviderDto()
		{
			ProviderType = AiProviderType.OpenAI,
			Chat = new ChatSettings() { ModelId = "gpt-4o-mini", },
			Embeddings = new EmbeddingSettings() { ModelId = "text-embedding-3-small", },
		};
		AiProviderDto[] providers = new AiProviderDto[]
		{
			firstProvider,
			new NetworkProviderDto()
			{
				ProviderType = AiProviderType.OpenAI,
				Chat = new ChatSettings() { ModelId = "another-model", },
				Embeddings = new EmbeddingSettings() { ModelId = "another-embedding-model", },
			},
		};
		AiAgentDto sut = new AiAgentDto();

		AiProviderDto result = sut.GetEmbeddingProvider(providers);

		result.Should().BeSameAs(providers[0]);
		result.Should().BeSameAs(firstProvider);
	}

	[Fact]
	public void GetEmbeddingProvider_Should_FallBackToSelectedProvider_WhenStoredEmbeddingProviderIsMissing()
	{
		NetworkProviderDto selectedProvider = new NetworkProviderDto()
		{
			ProviderType = AiProviderType.OpenAI,
			Chat = new ChatSettings() { ModelId = "gpt-4o-mini", },
			Embeddings = new EmbeddingSettings() { ModelId = "text-embedding-3-small", },
		};
		AiProviderDto[] providers = new AiProviderDto[]
		{
			selectedProvider,
			new NetworkProviderDto()
			{
				ProviderType = AiProviderType.OpenAI,
				Chat = new ChatSettings() { ModelId = "other-model", },
				Embeddings = new EmbeddingSettings() { ModelId = "other-embedding-model", },
			},
		};
		AiAgentDto sut = new AiAgentDto()
		{
			SelectedProviderId = selectedProvider.Id,
			EmbeddingProviderId = Guid.NewGuid(),
		};

		AiProviderDto result = sut.GetEmbeddingProvider(providers);

		result.Should().BeSameAs(selectedProvider);
	}
}