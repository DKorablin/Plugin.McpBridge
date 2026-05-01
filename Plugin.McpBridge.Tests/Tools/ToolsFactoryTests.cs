using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.AI;
using Plugin.McpBridge.Tools;
using SAL.Flatbed;
using Xunit;

namespace Plugin.McpBridge.Tests.Tools
{
	public class ToolsFactoryTests
	{
		#region Constructor

		[Fact]
		public void Ctor_TraceIsNull_ThrowsArgumentNullException()
		{
			ShellTools shell = new ShellTools();

			Action act = () => _ = new ToolsFactory(null!, shell);

			act.Should().Throw<ArgumentNullException>().WithParameterName("trace");
		}

		[Fact]
		public void Ctor_NullToolsHosts_ThrowsArgumentException()
		{
			Action act = () => _ = new ToolsFactory(TestUtils.Trace, null!);

			act.Should().Throw<ArgumentException>().WithParameterName("toolsHosts");
		}

		[Fact]
		public void Ctor_EmptyToolsHosts_ThrowsArgumentException()
		{
			Action act = () => _ = new ToolsFactory(TestUtils.Trace);

			act.Should().Throw<ArgumentException>().WithParameterName("toolsHosts");
		}

		#endregion

		#region CreateTools — permission filtering

		[Fact]
	public void CreateTools_NullExclusionList_ReturnsAllTools()
		{
			ToolsFactory sut = new ToolsFactory(TestUtils.Trace, new StubToolHost());

			IList<AITool> tools = sut.CreateTools(null, (s, e) => { }).ToList();

			tools.Should().HaveCount(2);
		}

		[Fact]
		public void CreateTools_ExclusionListWithNoMatch_ReturnsAllTools()
		{
			ToolsFactory sut = new ToolsFactory(TestUtils.Trace, new StubToolHost());

			IList<AITool> tools = sut.CreateTools(new String[] { "SystemInformation" }, (s, e) => { }).ToList();

			tools.Should().HaveCount(2);
		}

		[Fact]
		public void CreateTools_ExclusionListWithMatch_ExcludesTool()
		{
			ToolsFactory sut = new ToolsFactory(TestUtils.Trace, new StubToolHost());

			IList<AITool> tools = sut.CreateTools(new String[] { nameof(StubToolHost.NoConfirmTool) }, (s, e) => { }).ToList();

			tools.Should().HaveCount(1);
			tools[0].Should().BeAssignableTo<AIFunction>();
			((AIFunction)tools[0]).Name.Should().Be(nameof(StubToolHost.ConfirmTool));
		}

		[Fact]
		public void CreateTools_MultipleExclusions_ExcludesAll()
		{
			ToolsFactory sut = new ToolsFactory(TestUtils.Trace, new StubToolHost());

			IList<AITool> tools = sut.CreateTools(new String[] { nameof(StubToolHost.NoConfirmTool), nameof(StubToolHost.ConfirmTool) }, (s, e) => { }).ToList();

			tools.Should().BeEmpty();
		}

		[Fact]
		public void CreateTools_ToolNotInExclusionList_ReturnsTool()
		{
			ToolsFactory sut = new ToolsFactory(TestUtils.Trace, new StubToolHost());

			IList<AITool> tools = sut.CreateTools(new String[] { nameof(StubToolHost.ConfirmTool) }, (s, e) => { }).ToList();

			tools.Should().HaveCount(1);
			((AIFunction)tools[0]).Name.Should().Be(nameof(StubToolHost.NoConfirmTool));
		}

		#endregion

		#region CreateTools — confirmation wiring

		[Fact]
		public async Task CreateTools_ConfirmationRequired_WiresConfirmationHandler()
		{
			ToolsFactory sut = new ToolsFactory(TestUtils.Trace, new StubToolHost());
			Boolean handlerFired = false;
			IList<AITool> tools = sut.CreateTools(new String[] { nameof(StubToolHost.NoConfirmTool) }, (s, e) => { handlerFired = true; e.Confirm(false); }).ToList();

			await ((ToolFacade)tools[0]).InvokeAsync();

			handlerFired.Should().BeTrue();
		}

		[Fact]
		public async Task CreateTools_NoConfirmationRequired_HandlerNotWired()
		{
			ToolsFactory sut = new ToolsFactory(TestUtils.Trace, new StubToolHost());
			Boolean handlerFired = false;
			IList<AITool> tools = sut.CreateTools(new String[] { nameof(StubToolHost.ConfirmTool) }, (s, e) => { handlerFired = true; e.Confirm(true); }).ToList();

			await ((ToolFacade)tools[0]).InvokeAsync();

			handlerFired.Should().BeFalse();
		}

		#endregion

		#region CreateTools — multiple targets

		[Fact]
		public void CreateTools_MultipleTargets_ReturnsToolsFromAll()
		{
			(IHost _, PluginSettingsTools settingsTools, PluginMethodsTools methodsTools, ShellTools shellTools) = TestUtils.CreateDependencies();
			ToolsFactory sut = new ToolsFactory(TestUtils.Trace, shellTools, settingsTools, methodsTools);

			IList<AITool> tools = sut.CreateTools(Array.Empty<String>(), (s, e) => e.Confirm(true)).ToList();

			tools.Should().HaveCount(6);
		}

		[Fact]
		public void CreateTools_MultipleTargets_PartialExclusions_ReturnsSubset()
		{
			(IHost _, PluginSettingsTools settingsTools, PluginMethodsTools methodsTools, ShellTools shellTools) = TestUtils.CreateDependencies();
			ToolsFactory sut = new ToolsFactory(TestUtils.Trace, shellTools, settingsTools, methodsTools);

			IList<AITool> tools = sut.CreateTools(new String[] { nameof(ShellTools.SystemInformation), nameof(PluginSettingsTools.SettingsList) }, (s, e) => { }).ToList();

			tools.Should().HaveCount(4);
		}

		#endregion

		#region Nested types

		private sealed class StubToolHost
		{
			[Tool]
			[Description("No-confirm tool")]
			public Task<String> NoConfirmTool() => Task.FromResult("ok");

			[Tool(confirmationRequired: true)]
			[Description("Confirm tool")]
			public Task<String> ConfirmTool() => Task.FromResult("ok");
		}

		#endregion
	}
}