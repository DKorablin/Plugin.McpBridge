using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.AI;
using Moq;
using Plugin.McpBridge.Data;
using Plugin.McpBridge.Events;
using Plugin.McpBridge.Tools;
using SAL.Flatbed;
using Xunit;

namespace Plugin.McpBridge.Tests
{
	public class AssistantAgentTests
	{
		#region Constructor

		[Fact]
		public void Ctor_TraceIsNull_ThrowsArgumentNullException()
		{
			(IHost host, PluginSettingsTools _, PluginMethodsTools _, ShellTools _) = TestUtils.CreateDependencies();
			ToolsFactory factory = TestUtils.CreateToolFactory();

			Action act = () => _ = new AssistantAgent(null!, host, factory);

			act.Should().Throw<ArgumentNullException>().WithParameterName("trace");
		}

		[Fact]
		public void Ctor_HostIsNull_ThrowsArgumentNullException()
		{
			ToolsFactory factory = TestUtils.CreateToolFactory();

			Action act = () => _ = new AssistantAgent(TestUtils.Trace, null!, factory);

			act.Should().Throw<ArgumentNullException>().WithParameterName("host");
		}

		[Fact]
		public void Ctor_ToolsFactoryIsNull_ThrowsArgumentNullException()
		{
			(IHost host, PluginSettingsTools _, PluginMethodsTools _, ShellTools _) = TestUtils.CreateDependencies();

			Action act = () => _ = new AssistantAgent(TestUtils.Trace, host, null!);

			act.Should().Throw<ArgumentNullException>().WithParameterName("toolsFactory");
		}

		#endregion

		#region Initialize

		[Fact]
		public async Task Initialize_CalledTwice_ResetsSession()
		{
			Mock<IChatClient> mockClient = new Mock<IChatClient>();
			mockClient.Setup(x => x.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions?>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(new ChatResponse(new ChatMessage(ChatRole.Assistant, "ok")));

			(IHost host, PluginSettingsTools settings, PluginMethodsTools methods, ShellTools shell) = TestUtils.CreateDependencies();
			ToolsFactory factory = new ToolsFactory(TestUtils.Trace, shell, settings, methods);
			AssistantAgent sut = new AssistantAgent(TestUtils.Trace, host, factory, (s, h) => mockClient.Object);
			Settings agentSettings = new Settings(host);
			AiProviderDto provider = new AiProviderDto { ProviderType = AiProviderType.LocalOpenAICompatible };

			sut.Initialize(agentSettings, provider);
			sut.Initialize(agentSettings, provider);

			Func<Task> act = async () => await sut.InvokeMessageAsync("hello");
			await act.Should().NotThrowAsync();
		}

		#endregion

		#region InvokeMessageAsync

		[Fact]
		public async Task InvokeMessageAsync_EmptyMessage_FiresErrorResponse()
		{
			AssistantAgent sut = TestUtils.CreateSut();
			AgentResponseEventArgs? received = null;
			sut.AiResponseReceived += (s, e) => received = e;

			await sut.InvokeMessageAsync(String.Empty);

			received.Should().NotBeNull();
			received!.Response.Should().Contain("empty");
			received.IsFinal.Should().BeTrue();
		}

		[Fact]
		public async Task InvokeMessageAsync_WhitespaceMessage_FiresErrorResponse()
		{
			AssistantAgent sut = TestUtils.CreateSut();
			AgentResponseEventArgs? received = null;
			sut.AiResponseReceived += (s, e) => received = e;

			await sut.InvokeMessageAsync("   ");

			received.Should().NotBeNull();
			received!.Response.Should().Contain("empty");
		}

		[Fact]
		public async Task InvokeMessageAsync_AgentNotConfigured_FiresNotConfiguredResponse()
		{
			AssistantAgent sut = TestUtils.CreateSut();
			AgentResponseEventArgs? received = null;
			sut.AiResponseReceived += (s, e) => received = e;

			await sut.InvokeMessageAsync("hello");

			received.Should().NotBeNull();
			received!.Response.Should().Contain("not configured");
			received.IsFinal.Should().BeTrue();
		}

		[Fact]
		public async Task InvokeMessageAsync_HttpRequestException_FiresErrorResponse()
		{
			Mock<IChatClient> mockClient = new Mock<IChatClient>();
			mockClient.Setup(x => x.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions?>(), It.IsAny<CancellationToken>()))
				.ThrowsAsync(new HttpRequestException("network failure"));

			AssistantAgent sut = TestUtils.CreateInitializedSut(mockChatClient: mockClient);
			AgentResponseEventArgs? received = null;
			sut.AiResponseReceived += (s, e) => received = e;

			await sut.InvokeMessageAsync("hello");

			received.Should().NotBeNull();
			received!.Response.Should().Contain("network failure");
			received.IsFinal.Should().BeTrue();
		}

		[Fact]
		public async Task InvokeMessageAsync_OperationCancelled_FiresCancelledResponse()
		{
			Mock<IChatClient> mockClient = new Mock<IChatClient>();
			mockClient.Setup(x => x.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions?>(), It.IsAny<CancellationToken>()))
				.ThrowsAsync(new OperationCanceledException());

			AssistantAgent sut = TestUtils.CreateInitializedSut(mockChatClient: mockClient);
			AgentResponseEventArgs? received = null;
			sut.AiResponseReceived += (s, e) => received = e;

			await sut.InvokeMessageAsync("hello");

			received.Should().NotBeNull();
			received!.Response.Should().Contain("cancelled");
			received.IsFinal.Should().BeTrue();
		}

		[Fact]
		public async Task InvokeMessageAsync_SuccessfulResponse_FiresAgentResponse()
		{
			Mock<IChatClient> mockClient = new Mock<IChatClient>();
			mockClient.Setup(x => x.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions?>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(new ChatResponse(new ChatMessage(ChatRole.Assistant, "Hello, world!")));

			AssistantAgent sut = TestUtils.CreateInitializedSut(mockChatClient: mockClient);
			AgentResponseEventArgs? received = null;
			sut.AiResponseReceived += (s, e) => received = e;

			await sut.InvokeMessageAsync("hello");

			received.Should().NotBeNull();
			received!.Response.Should().Contain("Hello, world!");
			received.IsFinal.Should().BeTrue();
		}

		[Fact]
		public async Task InvokeMessageAsync_ToolConfirmationDeclined_ConfirmationEventBubbles()
		{
			Mock<IChatClient> mockClient = new Mock<IChatClient>();
			mockClient.SetupSequence(x => x.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions?>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(new ChatResponse(new ChatMessage(ChatRole.Assistant, [new FunctionCallContent("call-1", nameof(PluginSettingsTools.SettingsSet), new Dictionary<String, Object?> { ["pluginId"] = TestUtils.PluginId, ["settingName"] = "Value", ["valueJson"] = "\"x\"" })])))
				.ReturnsAsync(new ChatResponse(new ChatMessage(ChatRole.Assistant, "done")));

			AssistantAgent sut = TestUtils.CreateInitializedSut(TestUtils.CreateSettingsPlugin(new SimpleSettings()), mockChatClient: mockClient);
			Boolean confirmationFired = false;
			sut.ConfirmationRequired += (s, e) => { confirmationFired = true; e.Confirm(false); };

			await sut.InvokeMessageAsync("update setting");

			confirmationFired.Should().BeTrue();
		}

		#endregion

		#region Nested types

		private sealed class SimpleSettings
		{
			public String Value { get; set; } = String.Empty;
		}

		#endregion
	}
}
