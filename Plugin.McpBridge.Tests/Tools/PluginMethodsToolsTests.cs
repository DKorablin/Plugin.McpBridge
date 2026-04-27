using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Plugin.McpBridge.Tools;
using SAL.Flatbed;
using Xunit;

namespace Plugin.McpBridge.Tests.Tools
{
	public class PluginMethodsToolsTests
	{
		#region Constructor

		[Fact]
		public void Ctor_HostIsNull_ThrowsArgumentNullException()
		{
			Action act = () => _ = new PluginMethodsTools(null!);

			act.Should().Throw<ArgumentNullException>().WithParameterName("host");
		}

		#endregion

		#region MethodsList

		[Fact]
		public async Task MethodsList_PluginNotFound_ReturnsNotFoundMessage()
		{
			(IHost _, PluginSettingsTools _, PluginMethodsTools sut, ShellTools _) = TestUtils.CreateDependencies();

			String result = await sut.MethodsList(TestUtils.PluginId);

			result.Should().Be($"Plugin with ID '{TestUtils.PluginId}' was not found.");
		}

		[Fact]
		public async Task MethodsList_PluginWithNoMembers_ReturnsNoMethodsMessage()
		{
			Mock<IPluginMethodInfo> method = TestUtils.CreateMethod("Run", Array.Empty<IPluginParameterInfo>());
			IPluginDescription plugin = TestUtils.CreateMethodPlugin(method.Object);
			(IHost _, PluginSettingsTools _, PluginMethodsTools sut, ShellTools _) = TestUtils.CreateDependencies(plugin);

			String result = await sut.MethodsList(TestUtils.PluginId);

			result.Should().Contain("Run");
		}

		#endregion

		#region MethodsInvoke

		[Fact]
		public async Task MethodsInvoke_ExistingMethod_InvokesAndReturnsResult()
		{
			Mock<IPluginMethodInfo> method = TestUtils.CreateMethod("Run", Array.Empty<IPluginParameterInfo>());
			method.Setup(x => x.Invoke(It.IsAny<Object[]>())).Returns(new { status = "ok" });
			(IHost _, PluginSettingsTools _, PluginMethodsTools sut, ShellTools _) = TestUtils.CreateDependencies(TestUtils.CreateMethodPlugin(method.Object));

			Object? result = await sut.MethodsInvoke(TestUtils.PluginId, "Run", "{}");

			result.ToString().Should().Contain("ok");
			method.Verify(x => x.Invoke(It.IsAny<Object[]>()), Times.Once);
		}

		[Fact]
		public async Task MethodsInvoke_PluginNotFound_ThrowsArgumentException()
		{
			(IHost _, PluginSettingsTools _, PluginMethodsTools sut, ShellTools _) = TestUtils.CreateDependencies();

			Func<Task> act = () => sut.MethodsInvoke(TestUtils.PluginId, "Run", "{}");

			await act.Should().ThrowAsync<ArgumentException>();
		}

		[Fact]
		public async Task MethodsInvoke_MethodNotFound_ThrowsArgumentException()
		{
			Mock<IPluginMethodInfo> method = TestUtils.CreateMethod("Run", Array.Empty<IPluginParameterInfo>());
			(IHost _, PluginSettingsTools _, PluginMethodsTools sut, ShellTools _) = TestUtils.CreateDependencies(TestUtils.CreateMethodPlugin(method.Object));

			Func<Task> act = () => sut.MethodsInvoke(TestUtils.PluginId, "NonExistent", "{}");

			await act.Should().ThrowAsync<ArgumentException>();
		}

		#endregion
	}
}
