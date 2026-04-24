using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using FluentAssertions;
using Microsoft.Extensions.AI;
using Moq;
using Plugin.McpBridge.Helpers;
using SAL.Flatbed;
using Xunit;

namespace Plugin.McpBridge.Tests
{
	public class AssistantAgentTests
	{
		private const String PluginId = "test-plugin";

		#region Constructor

		[Fact]
		public void Ctor_TraceIsNull_ThrowsArgumentNullException()
		{
			(TraceSource _, IHost host, PluginSettingsHelper settingsHelper, PluginMethodsHelper methodsHelper) = this.CreateDependencies();

			Action act = () => _ = new AssistantAgent(null!, host, settingsHelper, methodsHelper);

			act.Should().Throw<ArgumentNullException>().WithParameterName("trace");
		}

		[Fact]
		public void Ctor_HostIsNull_ThrowsArgumentNullException()
		{
			(TraceSource trace, IHost _, PluginSettingsHelper settingsHelper, PluginMethodsHelper methodsHelper) = this.CreateDependencies();

			Action act = () => _ = new AssistantAgent(trace, null!, settingsHelper, methodsHelper);

			act.Should().Throw<ArgumentNullException>().WithParameterName("host");
		}

		[Fact]
		public void Ctor_SettingsHelperIsNull_ThrowsArgumentNullException()
		{
			(TraceSource trace, IHost host, PluginSettingsHelper _, PluginMethodsHelper methodsHelper) = this.CreateDependencies();

			Action act = () => _ = new AssistantAgent(trace, host, null!, methodsHelper);

			act.Should().Throw<ArgumentNullException>().WithParameterName("settingsHelper");
		}

		[Fact]
		public void Ctor_MethodsHelperIsNull_ThrowsArgumentNullException()
		{
			(TraceSource trace, IHost host, PluginSettingsHelper settingsHelper, PluginMethodsHelper _) = this.CreateDependencies();

			Action act = () => _ = new AssistantAgent(trace, host, settingsHelper, null!);

			act.Should().Throw<ArgumentNullException>().WithParameterName("methodsHelper");
		}

		#endregion

		#region SystemInformation

		[Fact]
		public async System.Threading.Tasks.Task SystemInformation_ReturnsCurrentTimeFromProvider()
		{
			DateTimeOffset fixedTime = new DateTimeOffset(2025, 6, 15, 12, 0, 0, TimeSpan.Zero);
			FakeTimeProvider timeProvider = new FakeTimeProvider(fixedTime);
			AssistantAgent sut = this.CreateInitializedSut(timeProvider: timeProvider);

			String result = await sut.SystemInformation();

			result.Should().Contain(timeProvider.GetLocalNow().ToString());
		}

		#endregion

		#region MethodsList

		[Fact]
		public async System.Threading.Tasks.Task MethodsList_PluginNotFound_ReturnsNotFoundMessage()
		{
			AssistantAgent sut = this.CreateInitializedSut();

			String result = await sut.MethodsList(PluginId);

			result.Should().Be($"Plugin with ID '{PluginId}' was not found.");
		}

		[Fact]
		public async System.Threading.Tasks.Task MethodsList_ResultExceedsLimit_ConfirmedByUser_ReturnsTruncatedResult()
		{
			AssistantAgent sut = this.CreateInitializedSut(maxToolResultLength: 5);
			sut.ConfirmationRequired += (Object? s, AgentConfirmationEventArgs e) => e.Confirm(true);

			String result = await sut.MethodsList(PluginId);

			result.Should().Contain("[Result truncated:");
		}

		[Fact]
		public async System.Threading.Tasks.Task MethodsList_ResultExceedsLimit_DeclinedByUser_ThrowsArgumentException()
		{
			AssistantAgent sut = this.CreateInitializedSut(maxToolResultLength: 5);
			sut.ConfirmationRequired += (Object? s, AgentConfirmationEventArgs e) => e.Confirm(false);

			Func<System.Threading.Tasks.Task> act = () => sut.MethodsList(PluginId);

			await act.Should().ThrowAsync<ArgumentException>()
				.WithMessage("Operation declined by user due to result length.");
		}

		#endregion

		#region SettingsGet

		[Fact]
		public async System.Threading.Tasks.Task SettingsGet_ExistingSetting_ReturnsFormattedValue()
		{
			SimpleSettings settings = new SimpleSettings { Value = "hello" };
			AssistantAgent sut = this.CreateInitializedSut(CreateSettingsPlugin(settings));

			String result = await sut.SettingsGet(PluginId, nameof(SimpleSettings.Value));

			result.Should().Contain("[Value] = hello");
			result.Should().Contain("(String)");
		}

		#endregion

		#region SettingsList

		[Fact]
		public async System.Threading.Tasks.Task SettingsList_ExistingPlugin_ReturnsFormattedSettingsList()
		{
			SimpleSettings settings = new SimpleSettings { Value = "world" };
			AssistantAgent sut = this.CreateInitializedSut(CreateSettingsPlugin(settings));

			String result = await sut.SettingsList(PluginId);

			result.Should().Contain($"Settings for plugin '{PluginId}'");
			result.Should().Contain("[Value] = world");
		}

		#endregion

		#region SettingsSet

		[Fact]
		public async System.Threading.Tasks.Task SettingsSet_ConfirmationDeclined_ThrowsArgumentException()
		{
			AssistantAgent sut = this.CreateSut();
			sut.ConfirmationRequired += (Object? s, AgentConfirmationEventArgs e) => e.Confirm(false);

			Func<System.Threading.Tasks.Task> act = () => sut.SettingsSet(PluginId, nameof(SimpleSettings.Value), "new-value");

			await act.Should().ThrowAsync<ArgumentException>()
				.WithMessage("Operation declined by user.");
		}

		[Fact]
		public async System.Threading.Tasks.Task SettingsSet_ConfirmationApproved_UpdatesSettingAndReturnsResult()
		{
			SimpleSettings settings = new SimpleSettings { Value = "old" };
			AssistantAgent sut = this.CreateInitializedSut(CreateSettingsPlugin(settings));
			sut.ConfirmationRequired += (Object? s, AgentConfirmationEventArgs e) => e.Confirm(true);

			String result = await sut.SettingsSet(PluginId, nameof(SimpleSettings.Value), "new-value");

			settings.Value.Should().Be("new-value");
			result.Should().Contain("[Value] = new-value");
		}

		#endregion

		#region MethodsInvoke

		[Fact]
		public async System.Threading.Tasks.Task MethodsInvoke_ConfirmationDeclined_ThrowsArgumentException()
		{
			AssistantAgent sut = this.CreateSut();
			sut.ConfirmationRequired += (Object? s, AgentConfirmationEventArgs e) => e.Confirm(false);

			Func<System.Threading.Tasks.Task> act = () => sut.MethodsInvoke(PluginId, "Run", "{}");

			await act.Should().ThrowAsync<ArgumentException>()
				.WithMessage("Operation declined by user.");
		}

		[Fact]
		public async System.Threading.Tasks.Task MethodsInvoke_ConfirmationApproved_InvokesMethodAndReturnsResult()
		{
			Mock<IPluginMethodInfo> method = CreateMethod("Run", Array.Empty<IPluginParameterInfo>());
			method.Setup(x => x.Invoke(It.IsAny<Object[]>())).Returns(new { status = "ok" });
			AssistantAgent sut = this.CreateInitializedSut(CreateMethodPlugin(method.Object));
			sut.ConfirmationRequired += (Object? s, AgentConfirmationEventArgs e) => e.Confirm(true);

			String result = await sut.MethodsInvoke(PluginId, "Run", "{}");

			result.Should().Contain("ok");
			method.Verify(x => x.Invoke(It.IsAny<Object[]>()), Times.Once);
		}

		#endregion

		#region Factory helpers

		private (TraceSource Trace, IHost Host, PluginSettingsHelper SettingsHelper, PluginMethodsHelper MethodsHelper) CreateDependencies(IPluginDescription? pluginDescription = null)
		{
			Mock<IPluginStorage> mockStorage = CreateStorage(pluginDescription);
			Mock<IHost> mockHost = new Mock<IHost>();
			mockHost.SetupGet(x => x.Plugins).Returns(mockStorage.Object);

			IHost host = mockHost.Object;
			return (
				new TraceSource("test"),
				host,
				new PluginSettingsHelper(host),
				new PluginMethodsHelper(host));
		}

		/// <summary>Creates a bare agent without calling Initialize — _maxToolResultLength stays 0.</summary>
		private AssistantAgent CreateSut(IPluginDescription? pluginDescription = null)
		{
			(TraceSource trace, IHost host, PluginSettingsHelper settingsHelper, PluginMethodsHelper methodsHelper) = this.CreateDependencies(pluginDescription);
			return new AssistantAgent(trace, host, settingsHelper, methodsHelper);
		}

		/// <summary>Creates an agent with Initialize called so _maxToolResultLength is set.</summary>
		private AssistantAgent CreateInitializedSut(
			IPluginDescription? pluginDescription = null,
			Int32 maxToolResultLength = Int32.MaxValue,
			TimeProvider? timeProvider = null)
		{
			(TraceSource trace, IHost host, PluginSettingsHelper settingsHelper, PluginMethodsHelper methodsHelper) = this.CreateDependencies(pluginDescription);

			Mock<IChatClient> mockChatClient = new Mock<IChatClient>();

			Settings settings = new Settings
			{
				ProviderType = AiProviderType.LocalOpenAICompatible,
				MaxToolResultLength = maxToolResultLength,
			};

			AssistantAgent agent = new AssistantAgent(trace, host, settingsHelper, methodsHelper, (s, h) => mockChatClient.Object, timeProvider);
			agent.Initialize(settings);
			return agent;
		}

		private static Mock<IPluginStorage> CreateStorage(IPluginDescription? pluginDescription)
		{
			Mock<IPluginStorage> storage = new Mock<IPluginStorage>();
			if(pluginDescription != null)
				storage.Setup(x => x[PluginId]).Returns(pluginDescription);
			storage.SetupGet(x => x.Count).Returns(0);
			return storage;
		}

		private static IPluginDescription CreateSettingsPlugin(Object settingsObj)
		{
			Mock<IPlugin> instance = new Mock<IPlugin>();
			instance.As<IPluginSettings>().SetupGet(x => x.Settings).Returns(settingsObj);

			Mock<IPluginDescription> desc = new Mock<IPluginDescription>();
			desc.SetupGet(x => x.ID).Returns(PluginId);
			desc.SetupGet(x => x.Name).Returns(PluginId);
			desc.SetupGet(x => x.Instance).Returns(instance.Object);
			return desc.Object;
		}

		private static IPluginDescription CreateMethodPlugin(IPluginMethodInfo method)
		{
			Mock<IPluginTypeInfo> typeInfo = new Mock<IPluginTypeInfo>();
			typeInfo.SetupGet(x => x.Members).Returns(new IPluginMemberInfo[] { method });

			Mock<IPluginDescription> desc = new Mock<IPluginDescription>();
			desc.SetupGet(x => x.ID).Returns(PluginId);
			desc.SetupGet(x => x.Name).Returns(PluginId);
			desc.SetupGet(x => x.Type).Returns(typeInfo.Object);
			return desc.Object;
		}

		private static Mock<IPluginMethodInfo> CreateMethod(String name, IEnumerable<IPluginParameterInfo> parameters)
		{
			Mock<IPluginMethodInfo> method = new Mock<IPluginMethodInfo>();
			method.SetupGet(x => x.Name).Returns(name);
			method.SetupGet(x => x.TypeName).Returns("System.Object");
			method.SetupGet(x => x.MemberType).Returns(MemberTypes.Method);
			method.Setup(x => x.GetParameters()).Returns(parameters);
			return method;
		}

		#endregion

		#region Nested types

		private sealed class FakeTimeProvider : TimeProvider
		{
			private readonly DateTimeOffset _utcNow;

			public FakeTimeProvider(DateTimeOffset utcNow) => this._utcNow = utcNow;

			public override DateTimeOffset GetUtcNow() => this._utcNow;
		}

		private sealed class SimpleSettings
		{
			public String Value { get; set; } = String.Empty;
		}

		#endregion
	}
}
