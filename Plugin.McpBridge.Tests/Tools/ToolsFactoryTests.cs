using System;
using System.Linq;
using FluentAssertions;
using Plugin.McpBridge.Data;
using Plugin.McpBridge.Tools;
using SAL.Flatbed;
using Xunit;

namespace Plugin.McpBridge.Tests.Tools
{
	public class ToolsFactoryTests
	{
		#region CreateTools

		[Fact]
		public void CreateTools_TraceIsNull_ThrowsArgumentNullException()
		{
			ToolsFactory sut = CreateFactory();

			Action act = () => sut.CreateTools(null!, null).ToList();

			act.Should().Throw<ArgumentNullException>().WithParameterName("trace");
		}

		[Fact]
		public void CreateTools_AllowListIsNull_ReturnsAllDiscoveredTools()
		{
			ToolsFactory sut = CreateFactory();

			Int32 count = sut.CreateTools(TestUtils.Trace, null).Count();

			count.Should().BeGreaterThan(0);
		}

		[Fact]
		public void CreateTools_AllowListWithNoMatch_ReturnsNoTools()
		{
			ToolsFactory sut = CreateFactory();

			Int32 count = sut.CreateTools(TestUtils.Trace, new String[] { "DefinitelyNotAToolName" }).Count();

			count.Should().Be(0);
		}

		[Fact]
		public void CreateTools_AllowListWithKnownName_ReturnsOnlyThatTool()
		{
			ToolsFactory sut = CreateFactory();

			var tools = sut.CreateTools(TestUtils.Trace, new String[] { nameof(ShellTools.SystemInformation) }).ToArray();

			tools.Should().HaveCount(1);
			tools[0].Name.Should().Be(nameof(ShellTools.SystemInformation));
		}

		#endregion

		private static ToolsFactory CreateFactory(IPluginDescription? pluginDescription = null)
		{
			(IHost host, PluginSettingsTools _, PluginMethodsTools _, ShellTools _) = TestUtils.CreateDependencies(pluginDescription);
			Settings settings = new Settings();
			AiAgentDto agent = settings.SelectedAgent;
			return new ToolsFactory(host, settings, agent);
		}
	}
}
