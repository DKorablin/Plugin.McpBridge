using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using Moq.AutoMock;
using SAL.Flatbed;
using SAL.Windows;
using Xunit;

namespace Plugin.McpBridge.Tests;

public class PanelChatTests
{
	private readonly AutoMocker _mocker;

	public PanelChatTests()
	{
		_mocker = new AutoMocker();
	}

	[Fact]
	[Trait("Category", "Smoke")]
	public void PanelChat_Should_ConstructSuccessfully()
	{
		// Setup ISettingsProvider mock so LoadAssemblyParameters does not throw
		Mock<ISettingsProvider> settingsProviderMock = _mocker.GetMock<ISettingsProvider>();

		// Setup IPluginStorage mock with an empty plugin collection and a settings provider
		Mock<IPluginStorage> pluginStorageMock = _mocker.GetMock<IPluginStorage>();
		pluginStorageMock.As<IEnumerable<IPluginDescription>>()
			.Setup(x => x.GetEnumerator())
			.Returns(new List<IPluginDescription>().GetEnumerator());
		pluginStorageMock
			.Setup(x => x.Settings(It.IsAny<IPlugin>()))
			.Returns(settingsProviderMock.Object);

		// Setup IHostWindows mock
		Mock<IHostWindows> hostWindowsMock = _mocker.GetMock<IHostWindows>();
		hostWindowsMock.SetupGet(h => h.Plugins).Returns(pluginStorageMock.Object);

		// Create Plugin with the mocked IHostWindows and ITraceSource
		ITraceSource traceMock = _mocker.GetMock<ITraceSource>().Object;
		Plugin plugin = new Plugin(hostWindowsMock.Object, traceMock);

		WindowTestFactory.TestWindowControl testWindow = WindowTestFactory.CreateTestWindow(plugin);

		// Act
		using(PanelChat form = new PanelChat() { Parent = testWindow, })
		{
			form.CreateControl(); // triggers initialization

			// Assert
			form.IsHandleCreated.Should().BeTrue();
			testWindow.Caption.Should().Be("Undefinded");
		}
	}
}
