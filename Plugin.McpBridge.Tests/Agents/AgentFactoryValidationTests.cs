using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.AI;
using McpBridge.Core.Agents;
using McpBridge.Core.Data;
using Xunit;

namespace Plugin.McpBridge.Tests.Agents;

public class AgentFactoryValidationTests
{
	[Fact]
	public async Task CreateAgent_Should_Throw_WhenChatCapabilityDisabled()
	{
		AgentFactory sut = new AgentFactory();
		NetworkProviderDto provider = new NetworkProviderDto()
		{
			ProviderType = AiProviderType.OpenAI,
			Capabilities = ProviderCapabilities.Embeddings,
			Embeddings = new EmbeddingSettings() { ModelId = "text-embedding-3-small", },
		};

		Func<Task> act = async () => await sut.CreateAgent(provider, Array.Empty<AIFunction>(), "system");

		await act.Should().ThrowAsync<InvalidOperationException>()
			.WithMessage("*Chat capability is disabled.*");
	}

	[Fact]
	public async Task CreateAgent_Should_Throw_WhenChatModelIdIsMissing()
	{
		AgentFactory sut = new AgentFactory();
		NetworkProviderDto provider = new NetworkProviderDto()
		{
			ProviderType = AiProviderType.OpenAI,
			Capabilities = ProviderCapabilities.Chat | ProviderCapabilities.Embeddings,
			Embeddings = new EmbeddingSettings() { ModelId = "text-embedding-3-small", },
		};

		Func<Task> act = async () => await sut.CreateAgent(provider, Array.Empty<AIFunction>(), "system");

		await act.Should().ThrowAsync<InvalidOperationException>()
			.WithMessage("*Chat model is required.*");
	}

	[Fact]
	public void CreateEmbeddingGenerator_Should_Throw_WhenEmbeddingCapabilityDisabled()
	{
		NetworkProviderDto provider = new NetworkProviderDto()
		{
			ProviderType = AiProviderType.OpenAI,
			Capabilities = ProviderCapabilities.Chat,
			Chat = new ChatSettings() { ModelId = "gpt-4o-mini", },
		};

		Action act = () => _ = AgentFactory.CreateEmbeddingGenerator(provider);

		act.Should().Throw<InvalidOperationException>()
			.WithMessage("*Embeddings capability is disabled.*");
	}
}