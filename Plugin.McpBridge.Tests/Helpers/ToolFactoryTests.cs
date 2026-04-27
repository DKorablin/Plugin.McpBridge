using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.AI;
using Plugin.McpBridge.Helpers;
using Plugin.McpBridge.Tools;
using SAL.Flatbed;
using Xunit;

namespace Plugin.McpBridge.Tests.Helpers
{
	public class ToolFactoryTests
	{
		#region Constructor

		[Fact]
		public void Ctor_TraceIsNull_ThrowsArgumentNullException()
		{
			ShellTools shell = new ShellTools();

			Action act = () => _ = new ToolFactory(null!, shell);

			act.Should().Throw<ArgumentNullException>().WithParameterName("trace");
		}

		[Fact]
		public void Ctor_NullToolsHosts_ThrowsArgumentException()
		{
			Action act = () => _ = new ToolFactory(TestUtils.Trace, null!);

			act.Should().Throw<ArgumentException>().WithParameterName("toolsHosts");
		}

		[Fact]
		public void Ctor_EmptyToolsHosts_ThrowsArgumentException()
		{
			Action act = () => _ = new ToolFactory(TestUtils.Trace);

			act.Should().Throw<ArgumentException>().WithParameterName("toolsHosts");
		}

		#endregion

		#region CreateTools — permission filtering

		[Fact]
		public void CreateTools_NoPermissions_ReturnsEmpty()
		{
			ToolFactory sut = new ToolFactory(TestUtils.Trace, new StubToolHost());

			IList<AITool> tools = sut.CreateTools(Settings.Tools.SystemInformation, (s, e) => { }).ToList();

			tools.Should().BeEmpty();
		}

		[Fact]
		public void CreateTools_MatchingPermission_ReturnsTool()
		{
			ToolFactory sut = new ToolFactory(TestUtils.Trace, new StubToolHost());

			IList<AITool> tools = sut.CreateTools(Settings.Tools.SettingsList, (s, e) => { }).ToList();

			tools.Should().HaveCount(1);
			tools[0].Should().BeAssignableTo<AIFunction>();
			((AIFunction)tools[0]).Name.Should().Be(nameof(StubToolHost.NoConfirmTool));
		}

		[Fact]
		public void CreateTools_MultiplePermissions_ReturnsAllMatchingTools()
		{
			ToolFactory sut = new ToolFactory(TestUtils.Trace, new StubToolHost());
			Settings.Tools permissions = Settings.Tools.SettingsList | Settings.Tools.SettingsSet;

			IList<AITool> tools = sut.CreateTools(permissions, (s, e) => { }).ToList();

			tools.Should().HaveCount(2);
		}

		[Fact]
		public void CreateTools_PermissionNotGranted_ExcludesTool()
		{
			ToolFactory sut = new ToolFactory(TestUtils.Trace, new StubToolHost());

			IList<AITool> tools = sut.CreateTools(Settings.Tools.SettingsSet, (s, e) => { }).ToList();

			tools.Should().HaveCount(1);
			((AIFunction)tools[0]).Name.Should().Be(nameof(StubToolHost.ConfirmTool));
		}

		#endregion

		#region CreateTools — confirmation wiring

		[Fact]
		public async Task CreateTools_ConfirmationRequired_WiresConfirmationHandler()
		{
			ToolFactory sut = new ToolFactory(TestUtils.Trace, new StubToolHost());
			Boolean handlerFired = false;
			IList<AITool> tools = sut.CreateTools(Settings.Tools.SettingsSet, (s, e) => { handlerFired = true; e.Confirm(false); }).ToList();

			await ((ToolFacade)tools[0]).InvokeAsync();

			handlerFired.Should().BeTrue();
		}

		[Fact]
		public async Task CreateTools_NoConfirmationRequired_HandlerNotWired()
		{
			ToolFactory sut = new ToolFactory(TestUtils.Trace, new StubToolHost());
			Boolean handlerFired = false;
			IList<AITool> tools = sut.CreateTools(Settings.Tools.SettingsList, (s, e) => { handlerFired = true; e.Confirm(true); }).ToList();

			await ((ToolFacade)tools[0]).InvokeAsync();

			handlerFired.Should().BeFalse();
		}

		#endregion

		#region CreateTools — multiple targets

		[Fact]
		public void CreateTools_MultipleTargets_ReturnsToolsFromAll()
		{
			(IHost _, PluginSettingsTools settingsTools, PluginMethodsTools methodsTools, ShellTools shellTools) = TestUtils.CreateDependencies();
			ToolFactory sut = new ToolFactory(TestUtils.Trace, shellTools, settingsTools, methodsTools);
			Settings.Tools allPermissions = Settings.Tools.SystemInformation | Settings.Tools.SettingsList | Settings.Tools.SettingsGet | Settings.Tools.SettingsSet | Settings.Tools.MethodsList | Settings.Tools.MethodsInvoke;

			IList<AITool> tools = sut.CreateTools(allPermissions, (s, e) => e.Confirm(true)).ToList();

			tools.Should().HaveCount(6);
		}

		[Fact]
		public void CreateTools_MultipleTargets_PartialPermissions_ReturnsSubset()
		{
			(IHost _, PluginSettingsTools settingsTools, PluginMethodsTools methodsTools, ShellTools shellTools) = TestUtils.CreateDependencies();
			ToolFactory sut = new ToolFactory(TestUtils.Trace, shellTools, settingsTools, methodsTools);

			IList<AITool> tools = sut.CreateTools(Settings.Tools.SystemInformation | Settings.Tools.SettingsList, (s, e) => { }).ToList();

			tools.Should().HaveCount(2);
		}

		#endregion

		#region Nested types

		private sealed class StubToolHost
		{
			[Tool(Settings.Tools.SettingsList)]
			[Description("No-confirm tool")]
			public Task<String> NoConfirmTool() => Task.FromResult("ok");

			[Tool(Settings.Tools.SettingsSet, confirmationRequired: true)]
			[Description("Confirm tool")]
			public Task<String> ConfirmTool() => Task.FromResult("ok");
		}

		#endregion
	}
}
