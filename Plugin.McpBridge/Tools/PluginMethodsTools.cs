using System.ComponentModel;
using System.Reflection;
using System.Text;
using Plugin.McpBridge.Helpers;
using SAL.Flatbed;

namespace Plugin.McpBridge.Tools
{
	internal sealed class PluginMethodsTools
	{
		private readonly IHost _host;

		public PluginMethodsTools(IHost host)
		{
			this._host = host ?? throw new ArgumentNullException(nameof(host));
		}

		[Tool(Settings.Tools.MethodsList)]
		[Description("List all callable methods for a plugin")]
		public Task<String> MethodsList([Description("Plugin identifier")] String pluginId)
		{
			var pluginDescription = this._host.Plugins[pluginId];
			if(pluginDescription == null)
				return Task.FromResult($"Plugin with ID '{pluginId}' was not found.");

			IEnumerable<IPluginMemberInfo> callableMembers = PluginMethodsTools.GetCallableMembers(pluginDescription);
			if(!callableMembers.Any())
				return Task.FromResult($"Plugin '{pluginDescription.ID}' does not expose any callable methods.");

			StringBuilder builder = new StringBuilder();
			builder.Append("Callable methods for plugin '");
			builder.Append(pluginDescription.ID);
			builder.Append("' (");
			builder.Append(pluginDescription.Name);
			builder.AppendLine("):");
			foreach(IPluginMemberInfo member in PluginMethodsTools.GetCallableMembers(pluginDescription))
			{
				if(member.MemberType == MemberTypes.Method)
				{
					builder.Append("- ");
					builder.Append(member.Name);

					IPluginMethodInfo method = (IPluginMethodInfo)member;
					Boolean firstArg = true;
					builder.Append(" with parameters: ");
					foreach(IPluginParameterInfo argument in method.GetParameters())
					{
						if(!firstArg)
							builder.Append(", ");
						if(argument.IsOut)
							builder.Append("out ");
						builder.Append($"{argument.Name}: {argument.AssemblyQualifiedName}");
						String[] defaultValues = argument.GetDefaultValues();
						if(defaultValues?.Length > 0)
							builder.Append($" [{String.Join("|", defaultValues)}]");

						firstArg = false;
					}

					builder.AppendLine();
				}
			}

			return Task.FromResult(builder.ToString());
		}

		[Tool(Settings.Tools.MethodsInvoke, confirmationRequired: true)]
		[Description("Invoke a plugin method with arguments provided as JSON; requires user confirmation")]
		public Task<Object?> MethodsInvoke(
			[Description("Plugin identifier")] String pluginId,
			[Description("Method name")] String methodName,
			[Description("Arguments as JSON")] String argumentsJson)
		{
			var pluginDescription = this._host.Plugins[pluginId]
				?? throw new ArgumentException($"Plugin '{pluginId}' was not found.");

			var member = PluginMethodsTools.GetCallableMembers(pluginDescription).FirstOrDefault(m => m.Name == methodName)
				?? throw new ArgumentException($"Method '{methodName}' was not found in plugin '{pluginId}'.");

			if(member.MemberType == MemberTypes.Method)
			{
				var method = (IPluginMethodInfo)member;
				var arguments = Utils.ConvertArgumentsValue(method, argumentsJson);
				var result = method.Invoke(arguments);

				return Task.FromResult<Object?>(result);
			}

			var exc = new ArgumentException($"Unsupported member type '{member.MemberType}' for method invocation. Only methods are supported.");
			exc.Data.Add(nameof(pluginId), pluginId);
			exc.Data.Add(nameof(methodName), methodName);
			exc.Data.Add(nameof(argumentsJson), argumentsJson);
			throw exc;
		}

		public static Boolean HasCallableMembers(IPluginDescription pluginDescription)
			=> GetCallableMembers(pluginDescription).Any();

		public static IEnumerable<IPluginMemberInfo> GetCallableMembers(IPluginDescription pluginDescription)
		{
			if(pluginDescription.Type != null && pluginDescription.Type.Members != null)
				foreach(IPluginMemberInfo pluginMember in pluginDescription.Type.Members)
					if(pluginMember != null && pluginMember.MemberType == System.Reflection.MemberTypes.Method)
						yield return pluginMember;
		}
	}
}