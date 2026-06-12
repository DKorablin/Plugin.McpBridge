using System;
using FluentAssertions;
using Plugin.McpBridge.Data;
using Xunit;

namespace Plugin.McpBridge.Tests;

public class AiProviderDtoTests
{
	[Fact]
	public void ToString_Should_ShowInvalidMarker_WhenProviderIsNotConfigured()
	{
		NetworkProviderDto sut = new NetworkProviderDto()
		{
			ProviderType = AiProviderType.OpenAI,
			Capabilities = ProviderCapabilities.Chat,
			Chat = new ChatSettings() { ModelId = null, },
		};

		String text = sut.ToString();

		text.Should().Contain("[INVALID]");
		text.Should().Contain("Chat model is required.");
		sut.IsValid.Should().BeFalse();
		sut.ValidationError.Should().Be("Chat model is required.");
	}

	[Fact]
	public void ToString_Should_ShowFriendlyDetails_WhenProviderIsConfigured()
	{
		NetworkProviderDto sut = new NetworkProviderDto()
		{
			Description = "Fast Profile",
			ProviderType = AiProviderType.OpenAI,
			Capabilities = ProviderCapabilities.Chat | ProviderCapabilities.Embeddings,
			Chat = new ChatSettings() { ModelId = "gpt-4o-mini", },
			Embeddings = new EmbeddingSettings() { ModelId = "text-embedding-3-small", },
		};

		String text = sut.ToString();

		text.Should().NotContain("[INVALID]");
		text.Should().Contain("Fast Profile - OpenAI-compatible");
		text.Should().Contain("chat: gpt-4o-mini");
		text.Should().Contain("emb: text-embedding-3-small");
		sut.IsValid.Should().BeTrue();
		sut.ValidationError.Should().BeNull();
	}

	[Fact]
	public void ValidationError_Should_BeNull_ForValidStubProvider()
	{
		StubProviderDto sut = new StubProviderDto()
		{
			ProviderType = AiProviderType.Stub,
		};

		sut.ValidationError.Should().BeNull();
		sut.IsValid.Should().BeTrue();
	}
}