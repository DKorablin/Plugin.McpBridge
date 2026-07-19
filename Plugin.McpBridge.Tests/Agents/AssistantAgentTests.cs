using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.AI;
using Moq;
using Plugin.McpBridge.Agents;
using Plugin.McpBridge.Data;
using Plugin.McpBridge.Events;
using Plugin.McpBridge.Tests.Helpers;
using Plugin.McpBridge.Tools;
using SAL.Flatbed;
using Xunit;

namespace Plugin.McpBridge.Tests.Agents
{
	public class AssistantAgentTests
	{
		#region Constructor

		[Fact]
		public void Ctor_TraceIsNull_ThrowsArgumentNullException()
		{
			(IHost host, PluginSettingsTools _, PluginMethodsTools _, ShellTools _) = TestUtils.CreateDependencies();
			ToolsFactory factory = TestUtils.CreateToolFactory();

			Action act = () => _ = new AssistantAgent(null!, host, factory, new AgentFactory());

			act.Should().Throw<ArgumentNullException>().WithParameterName("trace");
		}

		[Fact]
		public void Ctor_HostIsNull_ThrowsArgumentNullException()
		{
			ToolsFactory factory = TestUtils.CreateToolFactory();

			Action act = () => _ = new AssistantAgent(TestUtils.Trace, null!, factory, new AgentFactory());

			act.Should().Throw<ArgumentNullException>().WithParameterName("host");
		}

		[Fact]
		public void Ctor_ToolsFactoryIsNull_ThrowsArgumentNullException()
		{
			(IHost host, PluginSettingsTools _, PluginMethodsTools _, ShellTools _) = TestUtils.CreateDependencies();

			Action act = () => _ = new AssistantAgent(TestUtils.Trace, host, null!, new AgentFactory());

			act.Should().Throw<ArgumentNullException>().WithParameterName("toolsFactory");
		}

		#endregion

		#region Initialize

		[Fact]
		public async Task Initialize_CalledTwice_ResetsSession()
		{
			Mock<IChatClient> mockClient = new Mock<IChatClient>();
			mockClient.Setup(x => x.GetStreamingResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions?>(), It.IsAny<CancellationToken>()))
				.Returns(AssistantAgentTests.StreamingUpdates(
					new ChatResponseUpdate { Role = ChatRole.Assistant, Contents = [new TextContent("ok")] },
					new ChatResponseUpdate { Role = ChatRole.Assistant, Contents = [], FinishReason = ChatFinishReason.Stop }));

			(IHost host, PluginSettingsTools _, PluginMethodsTools _, ShellTools _) = TestUtils.CreateDependencies();
			Settings agentSettings = new Settings();
			ToolsFactory factory = new ToolsFactory(host, agentSettings, agentSettings.SelectedAgent);
			AssistantAgent sut = new AssistantAgent(TestUtils.Trace, host, factory, new StubAgentFactory(mockClient.Object));
			AiProviderDto provider = new AiProviderDto { ProviderType = AiProviderType.Local };
			agentSettings.AiProviders.Add(provider);

			await sut.Initialize(agentSettings);
			await sut.Initialize(agentSettings);

			Func<Task> act = async () => await sut.InvokeMessageAsync("hello");
			await act.Should().NotThrowAsync();
		}

		#endregion

		#region InvokeMessageAsync

		[Fact]
		public async Task InvokeMessageAsync_EmptyMessage_FiresErrorResponse()
		{
			AssistantAgent sut = TestUtils.CreateSut();

			Func<Task> act = async () => await sut.InvokeMessageAsync(String.Empty);

			await act.Should().ThrowAsync<ArgumentNullException>();
		}

		[Fact]
		public async Task InvokeMessageAsync_WhitespaceMessage_FiresErrorResponse()
		{
			AssistantAgent sut = TestUtils.CreateSut();

			Func<Task> act = async () => await sut.InvokeMessageAsync("   ");

			await act.Should().ThrowAsync<ArgumentNullException>();
		}

		[Fact]
		public async Task InvokeMessageAsync_AgentNotConfigured_FiresNotConfiguredResponse()
		{
			AssistantAgent sut = TestUtils.CreateSut();

			Func<Task> act = async () => await sut.InvokeMessageAsync("hello");

			await act.Should().ThrowAsync<InvalidOperationException>();
		}

		[Fact]
		public async Task InvokeMessageAsync_HttpRequestException_FiresErrorResponse()
		{
			Mock<IChatClient> mockClient = new Mock<IChatClient>();
			mockClient.Setup(x => x.GetStreamingResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions?>(), It.IsAny<CancellationToken>()))
				.Returns(AssistantAgentTests.StreamingThrows<ChatResponseUpdate>(new HttpRequestException("network failure")));

			AssistantAgent sut = await TestUtils.CreateInitializedSut(mockChatClient: mockClient);

			Func<Task> act = async () => await sut.InvokeMessageAsync("hello");

			await act.Should().ThrowAsync<HttpRequestException>().WithMessage("*network failure*");
		}

		[Fact]
		public async Task InvokeMessageAsync_OperationCancelled_FiresCancelledResponse()
		{
			Mock<IChatClient> mockClient = new Mock<IChatClient>();
			mockClient.Setup(x => x.GetStreamingResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions?>(), It.IsAny<CancellationToken>()))
				.Returns(AssistantAgentTests.StreamingThrows<ChatResponseUpdate>(new OperationCanceledException()));

			AssistantAgent sut = await TestUtils.CreateInitializedSut(mockChatClient: mockClient);

			Func<Task> act = async () => await sut.InvokeMessageAsync("hello");

			await act.Should().ThrowAsync<OperationCanceledException>();
		}

		[Fact]
		public async Task InvokeMessageAsync_SuccessfulResponse_FiresAgentResponse()
		{
			Mock<IChatClient> mockClient = new Mock<IChatClient>();
			mockClient.Setup(x => x.GetStreamingResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions?>(), It.IsAny<CancellationToken>()))
				.Returns(AssistantAgentTests.StreamingUpdates(
					new ChatResponseUpdate { Role = ChatRole.Assistant, Contents = [new TextContent("Hello, world!")] },
					new ChatResponseUpdate { Role = ChatRole.Assistant, Contents = [], FinishReason = ChatFinishReason.Stop }));

			AssistantAgent sut = await TestUtils.CreateInitializedSut(mockChatClient: mockClient);
			List<AgentResponseEventArgs> received = new List<AgentResponseEventArgs>();
			sut.AiResponseReceived += (s, e) => received.Add(e);

			await sut.InvokeMessageAsync("hello");

			received.Should().NotBeNull();
			received.Should().ContainSingle(x => !x.IsFinal && x.Message!.Text!.Contains("Hello, world!"));
			received.Should().ContainSingle(x => x.IsFinal && x.Message == null);
		}

		[Fact]
		public async Task InvokeMessageAsync_ToolConfirmationDeclined_ConfirmationEventBubbles()
		{
			Mock<IChatClient> mockClient = new Mock<IChatClient>();
			ToolApprovalRequestContent approvalRequest = new ToolApprovalRequestContent(
				"approval-1",
				new FunctionCallContent("call-1", nameof(PluginSettingsTools.SettingsSet), new Dictionary<String, Object?> { ["pluginId"] = TestUtils.PluginId, ["settingName"] = "Value", ["valueJson"] = "\"x\"" }));

			mockClient.SetupSequence(x => x.GetStreamingResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions?>(), It.IsAny<CancellationToken>()))
				.Returns(AssistantAgentTests.StreamingUpdates(new ChatResponseUpdate { Role = ChatRole.Assistant, Contents = [approvalRequest] }))
				.Returns(AssistantAgentTests.StreamingUpdates(
					new ChatResponseUpdate { Role = ChatRole.Assistant, Contents = [new TextContent("done")] },
					new ChatResponseUpdate { Role = ChatRole.Assistant, Contents = [], FinishReason = ChatFinishReason.Stop }));

			AssistantAgent sut = await TestUtils.CreateInitializedSut(TestUtils.CreateSettingsPlugin(new SimpleSettings()), mockChatClient: mockClient);
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

		private static async IAsyncEnumerable<T> StreamingThrows<T>(Exception exception)
		{
			await Task.Yield();
			throw exception;
			yield break;
		}

		private static async IAsyncEnumerable<ChatResponseUpdate> StreamingUpdates(params ChatResponseUpdate[] updates)
		{
			foreach(ChatResponseUpdate update in updates)
			{
				yield return update;
				await Task.Yield();
			}
		}

		#endregion
	}
}
