using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Moq;
using Plugin.McpBridge.Agents;
using Plugin.McpBridge.Data;
using Plugin.McpBridge.Tests.Helpers;
using Plugin.McpBridge.Tools;
using SAL.Flatbed;

namespace Plugin.McpBridge.Tests;

internal static class TestUtils
{
	public const String PluginId = "test-plugin";
	public static readonly ITraceSource Trace = new Mock<ITraceSource>().Object;

	public static (IHost Host, PluginSettingsTools Settings, PluginMethodsTools Methods, ShellTools Shell) CreateDependencies(IPluginDescription? pluginDescription = null, TimeProvider? timeProvider = null)
	{
		Mock<IPluginStorage> mockStorage = CreateStorage(pluginDescription);
		Mock<IHost> mockHost = new Mock<IHost>();
		mockHost.SetupGet(x => x.Plugins).Returns(mockStorage.Object);

		IHost host = mockHost.Object;
		return (
			host,
			new PluginSettingsTools(host),
			new PluginMethodsTools(host),
			new ShellTools(timeProvider));
	}

	public static ToolsFactory CreateToolFactory(IPluginDescription? pluginDescription = null, TimeProvider? timeProvider = null)
	{
		(IHost host, PluginSettingsTools _, PluginMethodsTools _, ShellTools _) = CreateDependencies(pluginDescription, timeProvider);
		Settings settings = new Settings();
		AiAgentDto agent = settings.SelectedAgent;
		return new ToolsFactory(host, settings, agent);
	}

	public static AssistantAgent CreateSut(IPluginDescription? pluginDescription = null)
	{
		(IHost host, PluginSettingsTools _, PluginMethodsTools _, ShellTools _) = CreateDependencies(pluginDescription);
		Settings settings = new Settings();
		AiAgentDto agent = settings.SelectedAgent;
		ToolsFactory toolFactory = new ToolsFactory(host, settings, agent);
		return new AssistantAgent(Trace, host, toolFactory, new AgentFactory());
	}

	public static async Task<AssistantAgent> CreateInitializedSut(
		IPluginDescription? pluginDescription = null,
		TimeProvider? timeProvider = null,
		Mock<IChatClient>? mockChatClient = null)
	{
		(IHost host, PluginSettingsTools _, PluginMethodsTools _, ShellTools _) = CreateDependencies(pluginDescription, timeProvider);
		Settings settings = new Settings();
		AiAgentDto agentSettings = settings.SelectedAgent;
		ToolsFactory toolFactory = new ToolsFactory(host, settings, agentSettings);
		mockChatClient ??= new Mock<IChatClient>();

		AiProviderDto provider = new AiProviderDto { ProviderType = AiProviderType.Local };
		settings.AiProviders.Add(provider);

		AssistantAgent assistant = new AssistantAgent(Trace, host, toolFactory, new StubAgentFactory(mockChatClient.Object));
		await assistant.Initialize(settings);
		return assistant;
	}

	private static Mock<IPluginStorage> CreateStorage(IPluginDescription? pluginDescription)
	{
		Mock<IPluginStorage> storage = new Mock<IPluginStorage>();
		if(pluginDescription != null)
			storage.Setup(x => x[PluginId]).Returns(pluginDescription);
		storage.SetupGet(x => x.Count).Returns(pluginDescription == null ? 0 : 1);

		IPluginDescription[] items = pluginDescription != null ? new[] { pluginDescription } : Array.Empty<IPluginDescription>();
		storage.Setup(x => x.GetEnumerator()).Returns(() => ((IEnumerable<IPluginDescription>)items).GetEnumerator());
		return storage;
	}

	public static IPluginDescription CreateSettingsPlugin(Object settingsObj)
	{
		Mock<IPlugin> instance = new Mock<IPlugin>();
		instance.As<IPluginSettings>().SetupGet(x => x.Settings).Returns(settingsObj);

		Mock<IPluginDescription> desc = new Mock<IPluginDescription>();
		desc.SetupGet(x => x.ID).Returns(PluginId);
		desc.SetupGet(x => x.Name).Returns(PluginId);
		desc.SetupGet(x => x.Instance).Returns(instance.Object);
		return desc.Object;
	}

	public static IPluginDescription CreateMethodPlugin(IPluginMethodInfo method)
	{
		Mock<IPluginTypeInfo> typeInfo = new Mock<IPluginTypeInfo>();
		typeInfo.SetupGet(x => x.Members).Returns(new IPluginMemberInfo[] { method });

		Mock<IPluginDescription> desc = new Mock<IPluginDescription>();
		desc.SetupGet(x => x.ID).Returns(PluginId);
		desc.SetupGet(x => x.Name).Returns(PluginId);
		desc.SetupGet(x => x.Type).Returns(typeInfo.Object);
		return desc.Object;
	}

	public static Mock<IPluginMethodInfo> CreateMethod(String name, IEnumerable<IPluginParameterInfo> parameters)
	{
		Mock<IPluginMethodInfo> method = new Mock<IPluginMethodInfo>();
		method.SetupGet(x => x.Name).Returns(name);
		method.SetupGet(x => x.TypeName).Returns("System.Object");
		method.SetupGet(x => x.MemberType).Returns(MemberTypes.Method);
		method.Setup(x => x.GetParameters()).Returns(parameters);
		return method;
	}
}