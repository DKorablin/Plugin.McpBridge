using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.AI;
using Moq;
using Plugin.McpBridge.Tools;
using SAL.Flatbed;

namespace Plugin.McpBridge.Tests;

internal static class TestUtils
{
	public const String PluginId = "test-plugin";
	public static readonly TraceSource Trace = new TraceSource("test");

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
		(IHost _, PluginSettingsTools settings, PluginMethodsTools methods, ShellTools shell) = CreateDependencies(pluginDescription, timeProvider);
		return new ToolsFactory(Trace, shell, settings, methods);
	}

	public static AssistantAgent CreateSut(IPluginDescription? pluginDescription = null)
	{
		(IHost host, PluginSettingsTools settings, PluginMethodsTools methods, ShellTools shell) = CreateDependencies(pluginDescription);
		ToolsFactory toolFactory = new ToolsFactory(Trace, shell, settings, methods);
		return new AssistantAgent(Trace, host, toolFactory);
	}

	public static AssistantAgent CreateInitializedSut(
		IPluginDescription? pluginDescription = null,
		TimeProvider? timeProvider = null,
		Mock<IChatClient>? mockChatClient = null)
	{
		(IHost host, PluginSettingsTools settingsTools, PluginMethodsTools methodsTools, ShellTools shellTools) = CreateDependencies(pluginDescription, timeProvider);
		ToolsFactory toolFactory = new ToolsFactory(Trace, shellTools, settingsTools, methodsTools);
		mockChatClient ??= new Mock<IChatClient>();

		Settings settings = new Settings
		{
			ProviderType = AiProviderType.LocalOpenAICompatible,
		};

		AssistantAgent agent = new AssistantAgent(Trace, host, toolFactory, (s, h) => mockChatClient.Object);
		agent.Initialize(settings);
		return agent;
	}

	private static Mock<IPluginStorage> CreateStorage(IPluginDescription? pluginDescription)
	{
		Mock<IPluginStorage> storage = new Mock<IPluginStorage>();
		if(pluginDescription != null)
			storage.Setup(x => x[PluginId]).Returns(pluginDescription);
		storage.SetupGet(x => x.Count).Returns(0);
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