using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.AI;
using Moq;
using Plugin.McpBridge.Helpers;
using SAL.Flatbed;

namespace Plugin.McpBridge.Tests;

internal static class TestHelpers
{
	public const String PluginId = "test-plugin";
	public static readonly TraceSource Trace = new TraceSource("test");

	public static (IHost Host, PluginSettingsHelper SettingsHelper, PluginMethodsHelper MethodsHelper) CreateDependencies(IPluginDescription? pluginDescription = null)
	{
		Mock<IPluginStorage> mockStorage = CreateStorage(pluginDescription);
		Mock<IHost> mockHost = new Mock<IHost>();
		mockHost.SetupGet(x => x.Plugins).Returns(mockStorage.Object);

		IHost host = mockHost.Object;
		return (
			host,
			new PluginSettingsHelper(host),
			new PluginMethodsHelper(host));
	}

	/// <summary>Creates a bare agent without calling Initialize — _maxToolResultLength stays 0.</summary>
	public static AssistantAgent CreateSut(IPluginDescription? pluginDescription = null)
	{
		(IHost host, PluginSettingsHelper settingsHelper, PluginMethodsHelper methodsHelper) = CreateDependencies(pluginDescription);
		return new AssistantAgent(Trace, host, settingsHelper, methodsHelper);
	}

	/// <summary>Creates an agent with Initialize called so _maxToolResultLength is set.</summary>
	public static AssistantAgent CreateInitializedSut(
		IPluginDescription? pluginDescription = null,
		TimeProvider? timeProvider = null)
	{
		(IHost host, PluginSettingsHelper settingsHelper, PluginMethodsHelper methodsHelper) = CreateDependencies(pluginDescription);

		Mock<IChatClient> mockChatClient = new Mock<IChatClient>();

		Settings settings = new Settings
		{
			ProviderType = AiProviderType.LocalOpenAICompatible,
		};

		AssistantAgent agent = new AssistantAgent(Trace, host, settingsHelper, methodsHelper, (s, h) => mockChatClient.Object, timeProvider);
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