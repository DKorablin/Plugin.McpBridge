using System.Reflection;
using System.Text;
using System.Text.Json;
using SAL.Flatbed;

namespace Plugin.McpBridge.Helpers
{
	internal sealed class PluginMethodsHelper
	{
		private readonly SAL.Flatbed.IHost _host;

		public PluginMethodsHelper(SAL.Flatbed.IHost host)
		{
			this._host = host ?? throw new ArgumentNullException(nameof(host));
		}

		public String ListPluginMethods(String pluginId)
		{
			var pluginDescription = this._host.Plugins[pluginId];
			if(pluginDescription == null)
				return $"Plugin with ID '{pluginId}' was not found.";

			IEnumerable<IPluginMemberInfo> callableMembers = PluginMethodsHelper.GetCallableMembers(pluginDescription);
			if(!callableMembers.Any())
				return $"Plugin '{pluginDescription.ID}' does not expose any callable methods.";

			StringBuilder builder = new StringBuilder();
			builder.Append("Callable methods for plugin '");
			builder.Append(pluginDescription.ID);
			builder.Append("' (");
			builder.Append(pluginDescription.Name);
			builder.AppendLine("):");
			foreach(IPluginMemberInfo member in PluginMethodsHelper.GetCallableMembers(pluginDescription))
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

			return builder.ToString();
		}

		public String InvokePluginMethod(String pluginId, String methodName, String argumentsJson)
		{
			var pluginDescription = this._host.Plugins[pluginId]
				?? throw new ArgumentException($"Plugin '{pluginId}' was not found.");

			var member = PluginMethodsHelper.GetCallableMembers(pluginDescription).FirstOrDefault(m => m.Name == methodName)
				?? throw new ArgumentException($"Method '{methodName}' was not found in plugin '{pluginId}'.");

			if(member.MemberType == MemberTypes.Method)
			{
				var method = (IPluginMethodInfo)member;
				var arguments = Utils.ConvertArgumentsValue(method, argumentsJson);
				var result = method.Invoke(arguments);

				return JsonSerializer.Serialize(result);
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