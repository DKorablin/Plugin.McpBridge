using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Plugin.McpBridge.Helpers;
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
			(IHost host, PluginSettingsHelper settingsHelper, PluginMethodsHelper methodsHelper) = TestHelpers.CreateDependencies();

			Action act = () => _ = new AssistantAgent(null!, host, settingsHelper, methodsHelper);

			act.Should().Throw<ArgumentNullException>().WithParameterName("trace");
		}

		[Fact]
		public void Ctor_HostIsNull_ThrowsArgumentNullException()
		{
			(IHost _, PluginSettingsHelper settingsHelper, PluginMethodsHelper methodsHelper) = TestHelpers.CreateDependencies();

			Action act = () => _ = new AssistantAgent(TestHelpers.Trace, null!, settingsHelper, methodsHelper);

			act.Should().Throw<ArgumentNullException>().WithParameterName("host");
		}

		[Fact]
		public void Ctor_SettingsHelperIsNull_ThrowsArgumentNullException()
		{
			(IHost host, PluginSettingsHelper _, PluginMethodsHelper methodsHelper) = TestHelpers.CreateDependencies();

			Action act = () => _ = new AssistantAgent(TestHelpers.Trace, host, null!, methodsHelper);

			act.Should().Throw<ArgumentNullException>().WithParameterName("settingsHelper");
		}

		[Fact]
		public void Ctor_MethodsHelperIsNull_ThrowsArgumentNullException()
		{
			(IHost host, PluginSettingsHelper settingsHelper, PluginMethodsHelper _) = TestHelpers.CreateDependencies();

			Action act = () => _ = new AssistantAgent(TestHelpers.Trace, host, settingsHelper, null!);

			act.Should().Throw<ArgumentNullException>().WithParameterName("methodsHelper");
		}

		#endregion

		#region SystemInformation

		[Fact]
		public async Task SystemInformation_ReturnsCurrentTimeFromProvider()
		{
			DateTimeOffset fixedTime = new DateTimeOffset(2025, 6, 15, 12, 0, 0, TimeSpan.Zero);
			FakeTimeProvider timeProvider = new FakeTimeProvider(fixedTime);
			AssistantAgent sut = TestHelpers.CreateInitializedSut(timeProvider: timeProvider);

			String result = await sut.SystemInformation();

			result.Should().Contain(timeProvider.GetLocalNow().ToString());
		}

		#endregion

		#region MethodsList

		[Fact]
		public async Task MethodsList_PluginNotFound_ReturnsNotFoundMessage()
		{
			AssistantAgent sut = TestHelpers.CreateInitializedSut();

			String result = await sut.MethodsList(TestHelpers.PluginId);

			result.Should().Be($"Plugin with ID '{TestHelpers.PluginId}' was not found.");
		}
		#endregion

		#region SettingsGet

		[Fact]
		public async Task SettingsGet_ExistingSetting_ReturnsFormattedValue()
		{
			SimpleSettings settings = new SimpleSettings { Value = "hello" };
			AssistantAgent sut = TestHelpers.CreateInitializedSut(TestHelpers.CreateSettingsPlugin(settings));

			Object? result = await sut.SettingsGet(TestHelpers.PluginId, nameof(SimpleSettings.Value));

			result.Should().BeOfType<String>();
			result.Should().Be("hello");
		}

		#endregion

		#region SettingsList

		[Fact]
		public async Task SettingsList_ExistingPlugin_ReturnsFormattedSettingsList()
		{
			SimpleSettings settings = new SimpleSettings { Value = "world" };
			AssistantAgent sut = TestHelpers.CreateInitializedSut(TestHelpers.CreateSettingsPlugin(settings));

			String result = await sut.SettingsList(TestHelpers.PluginId);

			result.Should().Contain($"Settings for plugin '{TestHelpers.PluginId}'");
			result.Should().Contain("[Value] = world");
		}

		#endregion

		#region SettingsSet

		[Fact]
		public async Task SettingsSet_ConfirmationApproved_UpdatesSettingAndReturnsResult()
		{
			SimpleSettings settings = new SimpleSettings { Value = "old" };
			AssistantAgent sut = TestHelpers.CreateInitializedSut(TestHelpers.CreateSettingsPlugin(settings));
			sut.ConfirmationRequired += (s, e) => e.Confirm(true);

			Object? result = await sut.SettingsSet(TestHelpers.PluginId, nameof(SimpleSettings.Value), "new-value");

			settings.Value.Should().Be("new-value");
			result.Should().BeOfType<String>();
			result.Should().Be("new-value");
		}

		#endregion

		#region MethodsInvoke

		[Fact]
		public async Task MethodsInvoke_ConfirmationApproved_InvokesMethodAndReturnsResult()
		{
			Mock<IPluginMethodInfo> method = TestHelpers.CreateMethod("Run", Array.Empty<IPluginParameterInfo>());
			method.Setup(x => x.Invoke(It.IsAny<Object[]>())).Returns(new { status = "ok" });
			AssistantAgent sut = TestHelpers.CreateInitializedSut(TestHelpers.CreateMethodPlugin(method.Object));
			sut.ConfirmationRequired += (s, e) => e.Confirm(true);

			Object? result = await sut.MethodsInvoke(TestHelpers.PluginId, "Run", "{}");

			result.ToString().Should().Contain("ok");
			method.Verify(x => x.Invoke(It.IsAny<Object[]>()), Times.Once);
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
