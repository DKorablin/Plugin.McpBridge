using System;
using System.Threading.Tasks;
using FluentAssertions;
using Plugin.McpBridge.Tools;
using SAL.Flatbed;
using Xunit;

namespace Plugin.McpBridge.Tests.Tools
{
	public class PluginSettingsToolsTests
	{
		#region Constructor

		[Fact]
		public void Ctor_HostIsNull_ThrowsArgumentNullException()
		{
			Action act = () => _ = new PluginSettingsTools(null!);

			act.Should().Throw<ArgumentNullException>().WithParameterName("host");
		}

		#endregion

		#region SettingsList

		[Fact]
		public async Task SettingsList_ExistingPlugin_ReturnsFormattedList()
		{
			SimpleSettings settings = new SimpleSettings { Value = "world" };
			(IHost _, PluginSettingsTools sut, PluginMethodsTools _, ShellTools _) = TestUtils.CreateDependencies(TestUtils.CreateSettingsPlugin(settings));

			String result = await sut.SettingsList(TestUtils.PluginId);

			result.Should().Contain($"Settings for plugin '{TestUtils.PluginId}'");
			result.Should().Contain("[Value] = world");
		}

		[Fact]
		public async Task SettingsList_PluginNotFound_ThrowsArgumentException()
		{
			(IHost _, PluginSettingsTools sut, PluginMethodsTools _, ShellTools _) = TestUtils.CreateDependencies();

			Func<Task> act = () => sut.SettingsList(TestUtils.PluginId);

			await act.Should().ThrowAsync<ArgumentException>();
		}

		[Fact]
		public async Task SettingsList_PluginWithNoSettings_ThrowsArgumentException()
		{
			IPluginDescription noSettingsPlugin = TestUtils.CreateMethodPlugin(
				TestUtils.CreateMethod("Run", Array.Empty<IPluginParameterInfo>()).Object);
			(IHost _, PluginSettingsTools sut, PluginMethodsTools _, ShellTools _) = TestUtils.CreateDependencies(noSettingsPlugin);

			Func<Task> act = () => sut.SettingsList(TestUtils.PluginId);

			await act.Should().ThrowAsync<ArgumentException>();
		}

		#endregion

		#region SettingsGet

		[Fact]
		public async Task SettingsGet_ExistingSetting_ReturnsValue()
		{
			SimpleSettings settings = new SimpleSettings { Value = "hello" };
			(IHost _, PluginSettingsTools sut, PluginMethodsTools _, ShellTools _) = TestUtils.CreateDependencies(TestUtils.CreateSettingsPlugin(settings));

			Object? result = await sut.SettingsGet(TestUtils.PluginId, nameof(SimpleSettings.Value));

			result.Should().Be("hello");
		}

		[Fact]
		public async Task SettingsGet_UnknownSettingName_ThrowsArgumentException()
		{
			SimpleSettings settings = new SimpleSettings { Value = "hello" };
			(IHost _, PluginSettingsTools sut, PluginMethodsTools _, ShellTools _) = TestUtils.CreateDependencies(TestUtils.CreateSettingsPlugin(settings));

			Func<Task> act = () => sut.SettingsGet(TestUtils.PluginId, "NonExistent");

			await act.Should().ThrowAsync<ArgumentException>();
		}

		#endregion

		#region SettingsSet

		[Fact]
		public async Task SettingsSet_ValidSetting_UpdatesValueAndReturnsNewValue()
		{
			SimpleSettings settings = new SimpleSettings { Value = "old" };
			(IHost _, PluginSettingsTools sut, PluginMethodsTools _, ShellTools _) = TestUtils.CreateDependencies(TestUtils.CreateSettingsPlugin(settings));

			Object? result = await sut.SettingsSet(TestUtils.PluginId, nameof(SimpleSettings.Value), "\"new-value\"");

			settings.Value.Should().Be("new-value");
			result.Should().Be("new-value");
		}

		[Fact]
		public async Task SettingsSet_UnknownSettingName_ThrowsArgumentException()
		{
			SimpleSettings settings = new SimpleSettings { Value = "old" };
			(IHost _, PluginSettingsTools sut, PluginMethodsTools _, ShellTools _) = TestUtils.CreateDependencies(TestUtils.CreateSettingsPlugin(settings));

			Func<Task> act = () => sut.SettingsSet(TestUtils.PluginId, "NonExistent", "\"value\"");

			await act.Should().ThrowAsync<ArgumentException>();
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
