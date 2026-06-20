using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.Extensions.AI;
using Moq;
using Moq.AutoMock;
using Plugin.McpBridge.Data;
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

	[Fact(Timeout = 5000)]
	[Trait("Category", "Smoke")]
	public async Task PanelChat_Should_ConstructSuccessfully()
	{
		Plugin plugin = this.CreatePlugin();

		WindowTestFactory.TestWindowControl testWindow = WindowTestFactory.CreateTestWindow(plugin);

		// Act
		using(PanelChat form = new PanelChat() { Parent = testWindow, })
		{
			form.CreateControl(); // triggers initialization

			// Assert
			form.IsHandleCreated.Should().BeTrue();
			testWindow.Caption.Should().Be("Undefined");
		}

		await Task.CompletedTask;
	}

	[Fact(Timeout = 5000)]
	[Trait("Category", "Smoke")]
	public async Task PanelChat_SendMenu_Should_ShowWorkflows_WhenConfigured()
	{
		Plugin plugin = this.CreatePlugin();
		Guid providerId = this.ConfigureStubProvider(plugin);
		String tempDirectory = this.CreateWorkflowDirectory(providerId, out _);
		try
		{
			plugin.Settings.WorkflowsDirectory = tempDirectory;

			WindowTestFactory.TestWindowControl testWindow = WindowTestFactory.CreateTestWindow(plugin);
			PanelChat panel = new PanelChat() { Parent = testWindow, };
			panel.CreateControl();

			ToolStripSplitButton sendButton = PanelChatTests.GetPrivateField<ToolStripSplitButton>(panel, "tsbnSend");
			PanelChatTests.InvokePrivateMethod(panel, "tsbnSend_DropDownOpening", sendButton, EventArgs.Empty);

			List<String> itemTexts = sendButton.DropDownItems.Cast<ToolStripItem>().Select(i => i.Text).ToList();
			itemTexts.Should().Contain("Workflows");
			itemTexts.Should().Contain("Workflow One");
		} finally
		{
			Directory.Delete(tempDirectory, recursive: true);
		}

		await Task.CompletedTask;
	}

	[Fact(Timeout = 15000)]
	[Trait("Category", "Smoke")]
	public async Task PanelChat_Should_ProcessMessage_UsingSelectedWorkflow()
	{
		Plugin plugin = this.CreatePlugin();
		Guid providerId = this.ConfigureStubProvider(plugin);
		String tempDirectory = this.CreateWorkflowDirectory(providerId, out _);
		try
		{
			plugin.Settings.WorkflowsDirectory = tempDirectory;

			WindowTestFactory.TestWindowControl testWindow = WindowTestFactory.CreateTestWindow(plugin);
			PanelChat panel = new PanelChat() { Parent = testWindow, };
			panel.CreateControl();

			ToolStripSplitButton sendButton = PanelChatTests.GetPrivateField<ToolStripSplitButton>(panel, "tsbnSend");
			PanelChatTests.InvokePrivateMethod(panel, "tsbnSend_DropDownOpening", sendButton, EventArgs.Empty);

			ToolStripMenuItem workflowItem = sendButton.DropDownItems
				.Cast<ToolStripItem>()
				.OfType<ToolStripMenuItem>()
				.First(i => i.Text == "Workflow One");
			workflowItem.PerformClick();

			Task getAgentTask = (Task)PanelChatTests.InvokePrivateMethod(panel, "GetAgent")!;
			await getAgentTask;

			Object? assistantAgent = panel.GetType().GetField("_agent", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(panel);
			assistantAgent.Should().NotBeNull();

			PropertyInfo? isWorkflowModeProperty = assistantAgent!.GetType().GetProperty("IsWorkflowMode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			isWorkflowModeProperty.Should().NotBeNull();
			Boolean isWorkflowMode = (Boolean)isWorkflowModeProperty!.GetValue(assistantAgent)!;
			isWorkflowMode.Should().BeTrue();

			PropertyInfo? sessionScopeNameProperty = assistantAgent.GetType().GetProperty("SessionScopeName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			sessionScopeNameProperty.Should().NotBeNull();
			String sessionScopeName = (String)sessionScopeNameProperty!.GetValue(assistantAgent)!;
			sessionScopeName.Should().Be("Workflow One");

			testWindow.Caption.Should().Contain("Workflow One");
			testWindow.Caption.Should().Contain("Workflow");
		} finally
		{
			Directory.Delete(tempDirectory, recursive: true);
		}
	}

	private Plugin CreatePlugin()
	{
		Mock<ISettingsProvider> settingsProviderMock = _mocker.GetMock<ISettingsProvider>();

		Mock<IPluginStorage> pluginStorageMock = _mocker.GetMock<IPluginStorage>();
		pluginStorageMock.As<IEnumerable<IPluginDescription>>()
			.Setup(x => x.GetEnumerator())
			.Returns(new List<IPluginDescription>().GetEnumerator());
		pluginStorageMock
			.Setup(x => x.Settings(It.IsAny<IPlugin>()))
			.Returns(settingsProviderMock.Object);

		Mock<IHostWindows> hostWindowsMock = _mocker.GetMock<IHostWindows>();
		hostWindowsMock.SetupGet(h => h.Plugins).Returns(pluginStorageMock.Object);

		ITraceSource traceMock = _mocker.GetMock<ITraceSource>().Object;
		return new Plugin(hostWindowsMock.Object, traceMock);
	}

	private Guid ConfigureStubProvider(Plugin plugin)
	{
		AiProviderDto provider = new AiProviderDto
		{
			ProviderType = AiProviderType.Stub,
			Description = "Stub Provider",
		};
		plugin.Settings.AiProviders.Add(provider);
		plugin.Settings.SelectedAgent.SelectedProviderId = provider.Id;
		return provider.Id;
	}

	private String CreateWorkflowDirectory(Guid providerId, out String workflowPath)
	{
		String directory = Path.Combine(Path.GetTempPath(), "Plugin.McpBridge.Tests", Guid.NewGuid().ToString());
		Directory.CreateDirectory(directory);

		workflowPath = Path.Combine(directory, "workflow-one.json");
		String workflowJson = $$"""
{
  "name": "Workflow One",
  "pattern": "Sequential",
  "nodes": [
    {
      "name": "Worker",
      "kind": "Agent",
      "providerId": "{{providerId}}"
    }
  ]
}
""";
		File.WriteAllText(workflowPath, workflowJson);
		return directory;
	}

	private static T GetPrivateField<T>(Object instance, String fieldName)
		where T : class
	{
		FieldInfo? field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
		field.Should().NotBeNull();
		return (field!.GetValue(instance) as T)!;
	}

	private static Object? InvokePrivateMethod(Object instance, String methodName, params Object[] arguments)
	{
		MethodInfo? method = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
		method.Should().NotBeNull();
		return method!.Invoke(instance, arguments);
	}
}
