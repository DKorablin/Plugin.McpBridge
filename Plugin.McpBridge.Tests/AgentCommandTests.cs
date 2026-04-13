using System;
using FluentAssertions;
using Xunit;

namespace Plugin.McpBridge.Tests
{
	public class AgentCommandTests
	{
		private const String Prefix = "COMMAND:";
		private const String PluginId = "my-plugin";
		private const String SettingsGroup = "SETTINGS";
		private const String MethodsGroup = "METHODS";

		[Fact]
		public void None_HasIsCommandFalseAndAllPropertiesEmpty()
		{
			AgentCommand.None.IsCommand.Should().BeFalse();
			AgentCommand.None.CommandGroup.Should().BeEmpty();
			AgentCommand.None.Command.Should().BeEmpty();
			AgentCommand.None.PluginId.Should().BeEmpty();
			AgentCommand.None.MemberName.Should().BeEmpty();
			AgentCommand.None.Arguments.Should().BeNull();
		}

		[Fact]
		public void Parse_ResponseWithoutCommandPrefix_ReturnsSameNoneInstance()
		{
			AgentCommand result = AgentCommand.Parse("SETTINGS LIST my-plugin");

			result.Should().BeSameAs(AgentCommand.None);
		}

		[Fact]
		public void Parse_EmptyString_ReturnsSameNoneInstance()
		{
			AgentCommand result = AgentCommand.Parse(String.Empty);

			result.Should().BeSameAs(AgentCommand.None);
		}

		[Fact]
		public void Parse_CommandPrefixIsCaseInsensitive_ParsesCommand()
		{
			AgentCommand result = AgentCommand.Parse($"command: {SettingsGroup} LIST {PluginId}");

			result.IsCommand.Should().BeTrue();
		}

		[Fact]
		public void Parse_UnknownCommandPayload_ReturnsSameNoneInstance()
		{
			AgentCommand result = AgentCommand.Parse($"{Prefix} UNKNOWN STUFF");

			result.Should().BeSameAs(AgentCommand.None);
		}

		[Fact]
		public void Parse_SettingsList_SetsAllPropertiesCorrectly()
		{
			AgentCommand result = AgentCommand.Parse($"{Prefix} {SettingsGroup} LIST {PluginId}");

			result.IsCommand.Should().BeTrue();
			result.CommandGroup.Should().Be(SettingsGroup);
			result.Command.Should().Be("LIST");
			result.PluginId.Should().Be(PluginId);
			result.MemberName.Should().BeEmpty();
			result.Arguments.Should().BeNull();
		}

		[Fact]
		public void Parse_SettingsGet_SetsAllPropertiesCorrectly()
		{
			AgentCommand result = AgentCommand.Parse($"{Prefix} {SettingsGroup} GET {PluginId} MySetting");

			result.IsCommand.Should().BeTrue();
			result.CommandGroup.Should().Be(SettingsGroup);
			result.Command.Should().Be("GET");
			result.PluginId.Should().Be(PluginId);
			result.MemberName.Should().Be("MySetting");
			result.Arguments.Should().BeNull();
		}

		[Fact]
		public void Parse_SettingsSet_SetsAllPropertiesCorrectly()
		{
			AgentCommand result = AgentCommand.Parse($"{Prefix} {SettingsGroup} SET {PluginId} MyKey=MyValue");

			result.IsCommand.Should().BeTrue();
			result.CommandGroup.Should().Be(SettingsGroup);
			result.Command.Should().Be("SET");
			result.PluginId.Should().Be(PluginId);
			result.MemberName.Should().Be("MyKey");
			result.Arguments.Should().Be("MyValue");
		}

		[Fact]
		public void Parse_SettingsSet_EmptyRightHandSide_ArgumentsIsEmpty()
		{
			AgentCommand result = AgentCommand.Parse($"{Prefix} {SettingsGroup} SET {PluginId} MyKey=");

			result.Command.Should().Be("SET");
			result.MemberName.Should().Be("MyKey");
			result.Arguments.Should().BeEmpty();
		}

		[Fact]
		public void Parse_MethodsList_SetsAllPropertiesCorrectly()
		{
			AgentCommand result = AgentCommand.Parse($"{Prefix} {MethodsGroup} LIST {PluginId}");

			result.IsCommand.Should().BeTrue();
			result.CommandGroup.Should().Be(MethodsGroup);
			result.Command.Should().Be("LIST");
			result.PluginId.Should().Be(PluginId);
			result.MemberName.Should().BeEmpty();
			result.Arguments.Should().BeNull();
		}

		[Fact]
		public void Parse_MethodsInvoke_WithJsonArguments_SetsAllPropertiesCorrectly()
		{
			AgentCommand result = AgentCommand.Parse($"{Prefix} {MethodsGroup} INVOKE {PluginId} Execute {{}}");

			result.IsCommand.Should().BeTrue();
			result.CommandGroup.Should().Be(MethodsGroup);
			result.Command.Should().Be("INVOKE");
			result.PluginId.Should().Be(PluginId);
			result.MemberName.Should().Be("Execute");
			result.Arguments.Should().Be("{}");
		}

		[Fact]
		public void Parse_MethodsInvoke_WithoutArguments_ArgumentsIsEmpty()
		{
			AgentCommand result = AgentCommand.Parse($"{Prefix} {MethodsGroup} INVOKE {PluginId} Execute");

			result.Command.Should().Be("INVOKE");
			result.MemberName.Should().Be("Execute");
			result.Arguments.Should().BeEmpty();
		}

		[Fact]
		public void Parse_CommandGroupIsCaseInsensitive_GroupReturnedAsUpperCase()
		{
			AgentCommand result = AgentCommand.Parse($"{Prefix} settings list {PluginId}");

			result.IsCommand.Should().BeTrue();
			result.CommandGroup.Should().Be(SettingsGroup);
			result.Command.Should().Be("LIST");
		}

		[Fact]
		public void Parse_CommandWithLeadingSpaceAfterPrefix_ParsesCorrectly()
		{
			AgentCommand resultWithSpace = AgentCommand.Parse($"{Prefix}  {SettingsGroup} LIST {PluginId}");
			AgentCommand resultNoSpace = AgentCommand.Parse($"{Prefix}{SettingsGroup} LIST {PluginId}");

			resultWithSpace.IsCommand.Should().BeTrue();
			resultNoSpace.IsCommand.Should().BeTrue();
		}
	}
}
