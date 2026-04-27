using System;
using System.ComponentModel;
using FluentAssertions;
using Moq;
using Plugin.McpBridge.Helpers;
using SAL.Flatbed;
using Xunit;

namespace Plugin.McpBridge.Tests.Helpers
{
	public class PluginSettingsHelperTests
	{
		[Fact]
		public void Ctor_HostIsNull_ThrowsArgumentNullException()
		{
			Action act = () => _ = new PluginSettingsHelper(null!);

			act.Should().Throw<ArgumentNullException>();
		}

		[Fact]
		public void HasPluginSettings_InstanceImplementsIPluginSettings_ReturnsTrue()
		{
			Mock<IPlugin> instance = new Mock<IPlugin>();
			instance.As<IPluginSettings>();
			Mock<IPluginDescription> pluginDescription = new Mock<IPluginDescription>();
			pluginDescription.SetupGet(x => x.Instance).Returns(instance.Object);

			Boolean result = PluginSettingsHelper.HasPluginSettings(pluginDescription.Object);

			result.Should().BeTrue();
		}

		[Fact]
		public void HasPluginSettings_InstanceDoesNotImplementIPluginSettings_ReturnsFalse()
		{
			Mock<IPlugin> instance = new Mock<IPlugin>();
			Mock<IPluginDescription> pluginDescription = new Mock<IPluginDescription>();
			pluginDescription.SetupGet(x => x.Instance).Returns(instance.Object);

			Boolean result = PluginSettingsHelper.HasPluginSettings(pluginDescription.Object);

			result.Should().BeFalse();
		}

		[Fact]
		public void ListPluginSettings_PluginNotFound_ThrowsArgumentException()
		{
			PluginSettingsHelper sut = CreateSut(null);

			Action act = () => _ = sut.ListPluginSettings(MissingPluginId);

			act.Should().Throw<ArgumentException>().WithMessage($"Plugin '{MissingPluginId}' was not found.");
		}

		[Fact]
		public void ListPluginSettings_PluginDoesNotExposeSettings_ThrowsArgumentException()
		{
			Mock<IPlugin> instance = new Mock<IPlugin>();
			Mock<IPluginDescription> pluginDescription = new Mock<IPluginDescription>();
			pluginDescription.SetupGet(x => x.ID).Returns(PluginId);
			pluginDescription.SetupGet(x => x.Instance).Returns(instance.Object);

			PluginSettingsHelper sut = CreateSut(pluginDescription.Object, PluginId);

			Action act = () => _ = sut.ListPluginSettings(PluginId);

			act.Should().Throw<ArgumentException>().WithMessage($"Plugin '{PluginId}' does not expose settings*");
		}

		[Fact]
		public void ListPluginSettings_SettingsHasNoPublicProperties_ThrowsArgumentException()
		{
			PluginSettingsHelper sut = CreateSut(CreatePluginDescription(PluginId, new EmptySettings()));

			Action act = () => _ = sut.ListPluginSettings(PluginId);

			act.Should().Throw<ArgumentException>().WithMessage("*does not contain public properties*");
		}

		[Fact]
		public void ListPluginSettings_WithAnnotatedProperties_ReturnsFormattedOutput()
		{
			PluginSettingsHelper sut = CreateSut(CreatePluginDescription(PluginId, new AnnotatedSettings { Named = "hello", Count = 42 }));

			String result = sut.ListPluginSettings(PluginId);

			result.Should().Contain($"Settings for plugin '{PluginId}'");
			result.Should().Contain("Display Name [Named] = hello (String) - A description");
			result.Should().Contain("[Count] = 42 (Int32)");
		}

		[Fact]
		public void ListPluginSettings_WriteOnlyProperty_IsSkippedInOutput()
		{
			PluginSettingsHelper sut = CreateSut(CreatePluginDescription(PluginId, new WriteOnlySettings()));

			String result = sut.ListPluginSettings(PluginId);

			result.Should().NotContain(nameof(WriteOnlySettings.WriteOnly));
			result.Should().Contain("[Readable]");
		}

		[Fact]
		public void ReadPluginSetting_PluginNotFound_ThrowsArgumentException()
		{
			PluginSettingsHelper sut = CreateSut(null);

			Action act = () => _ = sut.ReadPluginSetting(MissingPluginId, "SomeSetting");

			act.Should().Throw<ArgumentException>().WithMessage($"Plugin '{MissingPluginId}' was not found.");
		}

		[Fact]
		public void ReadPluginSetting_SettingNotFound_ThrowsArgumentException()
		{
			PluginSettingsHelper sut = CreateSut(CreatePluginDescription(PluginId, new AnnotatedSettings()));

			Action act = () => _ = sut.ReadPluginSetting(PluginId, NonExistentSetting);

			act.Should().Throw<ArgumentException>().WithMessage($"Setting '{NonExistentSetting}' was not found for plugin '{PluginId}'.");
		}

		[Fact]
		public void ReadPluginSetting_ByPropertyName_ReturnsFormattedValue()
		{
			PluginSettingsHelper sut = CreateSut(CreatePluginDescription(PluginId, new AnnotatedSettings { Named = "test-value" }));

			Object? result = sut.ReadPluginSetting(PluginId, "Named");

			result.Should().BeOfType<String>();
			result.Should().Be("test-value");
		}

		[Fact]
		public void ReadPluginSetting_ByDisplayName_FindsAndReturnsSetting()
		{
			PluginSettingsHelper sut = CreateSut(CreatePluginDescription(PluginId, new AnnotatedSettings { Named = "found" }));

			Object? result = sut.ReadPluginSetting(PluginId, "Display Name");

			result.Should().BeOfType<String>();
			result.Should().Be("found");
		}

		[Fact]
		public void UpdatePluginSetting_PluginNotFound_ThrowsArgumentException()
		{
			PluginSettingsHelper sut = CreateSut(null);

			Action act = () => _ = sut.UpdatePluginSetting(MissingPluginId, "SomeSetting", "value");

			act.Should().Throw<ArgumentException>().WithMessage($"Plugin '{MissingPluginId}' was not found.");
		}

		[Fact]
		public void UpdatePluginSetting_SettingNotFound_ThrowsArgumentException()
		{
			PluginSettingsHelper sut = CreateSut(CreatePluginDescription(PluginId, new AnnotatedSettings()));

			Action act = () => _ = sut.UpdatePluginSetting(PluginId, NonExistentSetting, "value");

			act.Should().Throw<ArgumentException>().WithMessage($"Setting '{NonExistentSetting}' was not found for plugin '{PluginId}'.");
		}

		[Fact]
		public void UpdatePluginSetting_ReadOnlySetting_ThrowsArgumentException()
		{
			PluginSettingsHelper sut = CreateSut(CreatePluginDescription(PluginId, new ReadOnlySettings()));

			Action act = () => _ = sut.UpdatePluginSetting(PluginId, "ReadOnly", "value");

			act.Should().Throw<ArgumentException>().WithMessage($"Setting 'ReadOnly' for plugin '{PluginId}' is read-only.");
		}

		[Fact]
		public void UpdatePluginSetting_StringSetting_UpdatesValueAndReturnsReadResult()
		{
			AnnotatedSettings settings = new AnnotatedSettings { Named = "old" };
			PluginSettingsHelper sut = CreateSut(CreatePluginDescription(PluginId, settings));

			Object? result = sut.UpdatePluginSetting(PluginId, "Named", "new-value");

			settings.Named.Should().Be("new-value");
			result.Should().Be("new-value");
		}

		[Fact]
		public void UpdatePluginSetting_Int32Setting_ConvertsAndUpdatesValue()
		{
			AnnotatedSettings settings = new AnnotatedSettings { Count = 0 };
			PluginSettingsHelper sut = CreateSut(CreatePluginDescription(PluginId, settings));

			Object? result = sut.UpdatePluginSetting(PluginId, "Count", "99");

			settings.Count.Should().Be(99);
			result.Should().Be(99);
		}

		[Fact]
		public void UpdatePluginSetting_NullableIntSetting_EmptyValue_SetsNull()
		{
			NullableSettings settings = new NullableSettings { Value = 5 };
			PluginSettingsHelper sut = CreateSut(CreatePluginDescription(PluginId, settings));

			sut.UpdatePluginSetting(PluginId, "Value", "");

			settings.Value.Should().BeNull();
		}

		[Fact]
		public void UpdatePluginSetting_EnumSetting_CaseInsensitive_ConvertsAndUpdatesValue()
		{
			EnumSettings settings = new EnumSettings { Mode = TestMode.Off };
			PluginSettingsHelper sut = CreateSut(CreatePluginDescription(PluginId, settings));

			sut.UpdatePluginSetting(PluginId, "Mode", "on");

			settings.Mode.Should().Be(TestMode.On);
		}

		[Fact]
		public void UpdatePluginSetting_StringArraySetting_JsonArray_DeserializesAndUpdatesValue()
		{
			StringArraySettings settings = new StringArraySettings { Tags = Array.Empty<String>() };
			PluginSettingsHelper sut = CreateSut(CreatePluginDescription(PluginId, settings));

			sut.UpdatePluginSetting(PluginId, "Tags", "[\"alpha\",\"beta\"]");

			settings.Tags.Should().BeEquivalentTo(new String[] { "alpha", "beta" });
		}

		[Fact]
		public void UpdatePluginSetting_JsonEncodedStringSetting_DeserializesAndUpdatesValue()
		{
			AnnotatedSettings settings = new AnnotatedSettings { Named = "old" };
			PluginSettingsHelper sut = CreateSut(CreatePluginDescription(PluginId, settings));

			sut.UpdatePluginSetting(PluginId, "Named", "\"json-value\"");

			settings.Named.Should().Be("json-value");
		}

		private static PluginSettingsHelper CreateSut(IPluginDescription pluginDescription, String pluginId = PluginId)
		{
			Mock<IPluginStorage> plugins = new Mock<IPluginStorage>();
			plugins.Setup(x => x[pluginId]).Returns(pluginDescription);

			Mock<IHost> host = new Mock<IHost>();
			host.SetupGet(x => x.Plugins).Returns(plugins.Object);

			return new PluginSettingsHelper(host.Object);
		}

		private static IPluginDescription CreatePluginDescription(String pluginId, Object settingsObject)
		{
			Mock<IPlugin> instance = new Mock<IPlugin>();
			Mock<IPluginSettings> pluginSettings = instance.As<IPluginSettings>();
			pluginSettings.SetupGet(x => x.Settings).Returns(settingsObject);

			Mock<IPluginDescription> pluginDescription = new Mock<IPluginDescription>();
			pluginDescription.SetupGet(x => x.ID).Returns(pluginId);
			pluginDescription.SetupGet(x => x.Name).Returns("Plugin Name");
			pluginDescription.SetupGet(x => x.Instance).Returns(instance.Object);

			return pluginDescription.Object;
		}

		private class EmptySettings { }

		private class AnnotatedSettings
		{
			[DisplayName("Display Name")]
			[Description("A description")]
			public String Named { get; set; } = "hello";

			public Int32 Count { get; set; } = 42;
		}

		private class WriteOnlySettings
		{
			public String WriteOnly { set { } }
			public String Readable { get; set; } = "value";
		}

		private class ReadOnlySettings
		{
			public String ReadOnly { get; } = "fixed";
		}

		private class NullableSettings
		{
			public Int32? Value { get; set; }
		}

		private class StringArraySettings
		{
			public String[] Tags { get; set; } = Array.Empty<String>();
		}

		private const String PluginId = "plugin-id";
		private const String MissingPluginId = "missing-plugin";
		private const String NonExistentSetting = "NonExistent";

		private enum TestMode { Off, On }

		private class EnumSettings
		{
			public TestMode Mode { get; set; }
		}
	}
}
