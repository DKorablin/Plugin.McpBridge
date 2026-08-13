using System.ComponentModel;
using FluentAssertions;
using McpBridge.Core.Data;
using Xunit;

namespace Plugin.McpBridge.Tests;

public class ProviderCapabilitiesTests
{
	[Fact]
	public void CoPilotProvider_Should_NormalizeCapabilitiesToChat()
	{
		CoPilotProviderDto sut = new CoPilotProviderDto()
		{
			ProviderType = AiProviderType.CoPilot,
			Capabilities = ProviderCapabilities.Embeddings,
		};

		sut.Capabilities.Should().Be(ProviderCapabilities.Chat);
		sut.SupportsCapability(ProviderCapabilities.Chat).Should().BeTrue();
		sut.SupportsCapability(ProviderCapabilities.Embeddings).Should().BeFalse();
	}

	[Fact]
	public void NetworkProvider_CanBeEmbeddingOnly()
	{
		NetworkProviderDto sut = new NetworkProviderDto()
		{
			ProviderType = AiProviderType.Local,
			Capabilities = ProviderCapabilities.Embeddings,
			Embeddings = new EmbeddingSettings() { ModelId = "nomic-embed-text", },
		};

		sut.SupportsCapability(ProviderCapabilities.Chat).Should().BeFalse();
		sut.SupportsCapability(ProviderCapabilities.Embeddings).Should().BeTrue();
	}

	[Fact]
	public void PropertyGrid_Should_AllowBoth_When_ChatAndEmbeddings()
	{
		NetworkProviderDto sut = new NetworkProviderDto()
		{
			ProviderType = AiProviderType.Local,
			Capabilities = ProviderCapabilities.Chat | ProviderCapabilities.Embeddings,
		};

		PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(sut);
		properties[nameof(AiProviderDto.Chat)]!.IsReadOnly.Should().BeFalse();
		properties[nameof(AiProviderDto.Embeddings)]!.IsReadOnly.Should().BeFalse();
	}
}