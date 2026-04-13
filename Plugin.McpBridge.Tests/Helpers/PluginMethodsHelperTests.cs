using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using FluentAssertions;
using Moq;
using Plugin.McpBridge.Helpers;
using SAL.Flatbed;
using Xunit;

namespace Plugin.McpBridge.Tests.Helpers
{
	public class PluginMethodsHelperTests
	{
		[Fact]
		public void Ctor_HostIsNull_ThrowsArgumentNullException()
		{
			Action act = () => _ = new PluginMethodsHelper(null!);

			act.Should().Throw<ArgumentNullException>();
		}

		[Fact]
		public void ListPluginMethods_PluginNotFound_ReturnsNotFoundMessage()
		{
			PluginMethodsHelper sut = CreateSut(null);

			String result = sut.ListPluginMethods("missing-plugin");

			result.Should().Be("Plugin with ID 'missing-plugin' was not found.");
		}

		[Fact]
		public void ListPluginMethods_PluginHasNoCallableMembers_ReturnsNoMethodsMessage()
		{
			Mock<IPluginDescription> pluginDescription = new Mock<IPluginDescription>();
			pluginDescription.SetupGet(x => x.ID).Returns("plugin-id");
			pluginDescription.SetupGet(x => x.Name).Returns("Plugin Name");
			pluginDescription.SetupGet(x => x.Type).Returns(CreatePluginType(Array.Empty<IPluginMemberInfo>()));

			PluginMethodsHelper sut = CreateSut(pluginDescription.Object, "plugin-id");

			String result = sut.ListPluginMethods("plugin-id");

			result.Should().Be("Plugin 'plugin-id' does not expose any callable methods.");
		}

		[Fact]
		public void ListPluginMethods_WithMethodAndParameters_ReturnsFormattedOutput()
		{
			Mock<IPluginParameterInfo> firstParameter = CreateParameter("count", "System.Int32", false);
			Mock<IPluginParameterInfo> secondParameter = CreateParameter("value", "System.String", true);

			Mock<IPluginMethodInfo> method = CreateMethod("Execute", new IPluginParameterInfo[]
			{
				firstParameter.Object,
				secondParameter.Object,
			});

			Mock<IPluginDescription> pluginDescription = new Mock<IPluginDescription>();
			pluginDescription.SetupGet(x => x.ID).Returns("plugin-id");
			pluginDescription.SetupGet(x => x.Name).Returns("Plugin Name");
			pluginDescription.SetupGet(x => x.Type).Returns(CreatePluginType(new IPluginMemberInfo[]
			{
				method.Object,
			}));

			PluginMethodsHelper sut = CreateSut(pluginDescription.Object, "plugin-id");

			String result = sut.ListPluginMethods("plugin-id");

			result.Should().Contain("Callable methods for plugin 'plugin-id' (Plugin Name):");
			result.Should().Contain("- Execute with parameters: count: System.Int32, out value: System.String");
		}

		[Fact]
		public void HasCallableMembers_WithMethods_ReturnsTrue()
		{
			Mock<IPluginMethodInfo> method = CreateMethod("Run", Array.Empty<IPluginParameterInfo>());
			Mock<IPluginDescription> pluginDescription = new Mock<IPluginDescription>();
			pluginDescription.SetupGet(x => x.Type).Returns(CreatePluginType(new IPluginMemberInfo[]
			{
				method.Object,
			}));

			Boolean result = PluginMethodsHelper.HasCallableMembers(pluginDescription.Object);

			result.Should().BeTrue();
		}

		[Fact]
		public void GetCallableMembers_WithNullTypeOrMembers_ReturnsEmpty()
		{
			Mock<IPluginDescription> pluginWithoutType = new Mock<IPluginDescription>();
			pluginWithoutType.SetupGet(x => x.Type).Returns((IPluginTypeInfo)null!);

			Mock<IPluginTypeInfo> pluginTypeWithoutMembers = new Mock<IPluginTypeInfo>();
			pluginTypeWithoutMembers.SetupGet(x => x.Members).Returns((IEnumerable<IPluginMemberInfo>)null!);
			Mock<IPluginDescription> pluginWithoutMembers = new Mock<IPluginDescription>();
			pluginWithoutMembers.SetupGet(x => x.Type).Returns(pluginTypeWithoutMembers.Object);

			PluginMethodsHelper.GetCallableMembers(pluginWithoutType.Object).Should().BeEmpty();
			PluginMethodsHelper.GetCallableMembers(pluginWithoutMembers.Object).Should().BeEmpty();
		}

		[Fact]
		public void GetCallableMembers_FiltersOutNonMethodsAndNullMembers()
		{
			Mock<IPluginMethodInfo> method = CreateMethod("Run", Array.Empty<IPluginParameterInfo>());

			Mock<IPluginMemberInfo> property = new Mock<IPluginMemberInfo>();
			property.SetupGet(x => x.MemberType).Returns(MemberTypes.Property);

			Mock<IPluginDescription> pluginDescription = new Mock<IPluginDescription>();
			pluginDescription.SetupGet(x => x.Type).Returns(CreatePluginType(new IPluginMemberInfo[]
			{
				null!,
				property.Object,
				method.Object,
			}));

			IPluginMemberInfo[] members = PluginMethodsHelper.GetCallableMembers(pluginDescription.Object).ToArray();

			members.Should().ContainSingle();
			members[0].Should().BeSameAs(method.Object);
		}

		[Fact]
		public void InvokePluginMethodPlaceholder_PluginNotFound_ThrowsArgumentException()
		{
			PluginMethodsHelper sut = CreateSut(null);

			Action act = () => _ = sut.InvokePluginMethodPlaceholder("missing-plugin", "Run", "{}");

			act.Should().Throw<ArgumentException>().WithMessage("Plugin 'missing-plugin' was not found.");
		}

		[Fact]
		public void InvokePluginMethodPlaceholder_MethodNotFound_ThrowsArgumentException()
		{
			Mock<IPluginDescription> pluginDescription = new Mock<IPluginDescription>();
			pluginDescription.SetupGet(x => x.ID).Returns("plugin-id");
			pluginDescription.SetupGet(x => x.Type).Returns(CreatePluginType(Array.Empty<IPluginMemberInfo>()));

			PluginMethodsHelper sut = CreateSut(pluginDescription.Object, "plugin-id");

			Action act = () => _ = sut.InvokePluginMethodPlaceholder("plugin-id", "MissingMethod", "{}");

			act.Should().Throw<ArgumentException>().WithMessage("Method 'MissingMethod' was not found in plugin 'plugin-id'.");
		}

		[Fact]
		public void InvokePluginMethodPlaceholder_ValidMethod_ConvertsArgumentsAndSerializesResult()
		{
			Mock<IPluginParameterInfo> firstParameter = CreateParameter("levels", "System.String[]", false);
			Mock<IPluginParameterInfo> secondParameter = CreateParameter("from", "System.DateTime", false);
			Mock<IPluginParameterInfo> thirdParameter = CreateParameter("to", "System.DateTime", false);
			Mock<IPluginParameterInfo> forthParameter = CreateParameter("empty", "System.String", false);

			Object[] invokedArguments = null;
			Mock<IPluginMethodInfo> method = CreateMethod("Execute", new IPluginParameterInfo[]
			{
				firstParameter.Object,
				secondParameter.Object,
				thirdParameter.Object,
				forthParameter.Object,
			});
			method
				.Setup(x => x.Invoke(It.IsAny<Object[]>()))
				.Callback<Object[]>(args => invokedArguments = args)
				.Returns(new { ok = true, message = "done" });

			Mock<IPluginDescription> pluginDescription = new Mock<IPluginDescription>();
			pluginDescription.SetupGet(x => x.ID).Returns("plugin-id");
			pluginDescription.SetupGet(x => x.Type).Returns(CreatePluginType(new IPluginMemberInfo[]
			{
				method.Object,
			}));

			PluginMethodsHelper sut = CreateSut(pluginDescription.Object, "plugin-id");

			String result = sut.InvokePluginMethodPlaceholder("plugin-id", "Execute", "{\"levels\":[\"Warning\",\"Information\"],\"from\":\"2026-04-05T00:00:00\",\"to\":\"2026-04-05T23:59:59\"}");

			invokedArguments.Should().NotBeNull();
			invokedArguments[0].Should().BeEquivalentTo(new String[] { "Warning", "Information" });
			invokedArguments[1].Should().Be(new DateTime(2026, 04, 05));
			invokedArguments[2].Should().Be(new DateTime(2026, 04, 05, 23, 59, 59));
			invokedArguments[3].Should().BeNull();

			using JsonDocument doc = JsonDocument.Parse(result);
			doc.RootElement.GetProperty("ok").GetBoolean().Should().BeTrue();
			doc.RootElement.GetProperty("message").GetString().Should().Be("done");
		}

		private static PluginMethodsHelper CreateSut(IPluginDescription pluginDescription, String pluginId = "plugin-id")
		{
			Mock<IPluginStorage> plugins = new Mock<IPluginStorage>();
			plugins.Setup(x => x[pluginId]).Returns(pluginDescription);

			Mock<IHost> host = new Mock<IHost>();
			host.SetupGet(x => x.Plugins).Returns(plugins.Object);

			return new PluginMethodsHelper(host.Object);
		}

		private static Mock<IPluginMethodInfo> CreateMethod(String name, IEnumerable<IPluginParameterInfo> parameters)
		{
			Mock<IPluginMethodInfo> method = new Mock<IPluginMethodInfo>();
			method.SetupGet(x => x.Name).Returns(name);
			method.SetupGet(x => x.TypeName).Returns("System.Object");
			method.SetupGet(x => x.MemberType).Returns(MemberTypes.Method);
			method.Setup(x => x.GetParameters()).Returns(parameters);

			return method;
		}

		private static Mock<IPluginParameterInfo> CreateParameter(String name, String typeName, Boolean isOut)
		{
			Mock<IPluginParameterInfo> parameter = new Mock<IPluginParameterInfo>();
			parameter.SetupGet(x => x.Name).Returns(name);
			parameter.SetupGet(x => x.TypeName).Returns(typeName);
			parameter.SetupGet(x => x.MemberType).Returns(MemberTypes.Custom);
			parameter.SetupGet(x => x.IsOut).Returns(isOut);

			return parameter;
		}

		private static IPluginTypeInfo CreatePluginType(IEnumerable<IPluginMemberInfo> members)
		{
			Mock<IPluginTypeInfo> pluginType = new Mock<IPluginTypeInfo>();
			pluginType.SetupGet(x => x.Members).Returns(members);

			return pluginType.Object;
		}
	}
}
