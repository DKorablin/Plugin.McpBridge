using System.Reflection;
using System.Text;
using SAL.Flatbed;
using System.Text.Json;

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
						builder.Append($"{argument.Name}: {argument.TypeName}");
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
				var arguments = ConvertArgumentsValue(method, argumentsJson);
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

		private static Object?[] ConvertArgumentsValue(IPluginMethodInfo method, String argumentsJson)
		{
			using(JsonDocument doc = JsonDocument.Parse(argumentsJson))
			{
				JsonElement root = doc.RootElement;

				var arguments = method.GetParameters().ToArray();
				var result = new Object?[arguments.Length];

				for(var loop = 0; loop < arguments.Length; loop++)
				{
					var argument = arguments[loop];

					// 1. Find the property in the JSON
					if(root.TryGetProperty(argument.Name, out JsonElement element))
					{
						// 2. Resolve the string type name to a System.Type
						Type targetType = ResolveType(argument.TypeName);

						// 3. Convert the specific JsonElement to the target type
						result[loop] = JsonSerializer.Deserialize(element.GetRawText(), targetType);
					} else
						result[loop] = null; // Or handle missing arguments as needed
				}

				return result;

				Type ResolveType(String typeName)
				{
					// Type.GetType requires assembly-qualified names for non-primitive types
					// (e.g., "System.DateTime" or "System.String[]")
					var t = Type.GetType(typeName)
						?? throw new ArgumentException($"Could not resolve type: {typeName}");

					return t;
				}
			}
		}
	}
}