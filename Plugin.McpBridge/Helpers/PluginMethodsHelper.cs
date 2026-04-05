using System.Reflection;
using System.Text;
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

			IEnumerable<IPluginMemberInfo> callableMembers = GetCallableMembers(pluginDescription);
			if(!callableMembers.Any())
				return $"Plugin '{pluginDescription.ID}' does not expose any callable methods.";

			StringBuilder builder = new StringBuilder();
			builder.Append("Callable methods for plugin '");
			builder.Append(pluginDescription.ID);
			builder.Append("' (");
			builder.Append(pluginDescription.Name);
			builder.AppendLine("):");
			foreach(IPluginMemberInfo pluginMember in PluginMethodsHelper.GetCallableMembers(pluginDescription))
			{
				if(pluginMember.MemberType == MemberTypes.Method)
				{
					builder.Append("- ");
					builder.Append(pluginMember.Name);

					IPluginMethodInfo method = (IPluginMethodInfo)pluginMember;
					Boolean firstArg = true;
					builder.Append(" with parameters: ");
					foreach(IPluginParameterInfo argument in method.GetParameters())
					{
						if(!firstArg)
							builder.Append(", ");
						if(argument.IsOut)
							builder.Append("out ");
						builder.Append(argument.Name);
						builder.Append(": ");
						builder.Append(argument.TypeName);
						String[] defaultValues = argument.GetDefaultValues();
						if(defaultValues != null && defaultValues.Length > 0)
						{
							builder.Append(" [");
							builder.Append(String.Join("|", defaultValues));
							builder.Append(']');
						}
						firstArg = false;

					}

					builder.Append(')');
					builder.AppendLine();
				}
			}

			return builder.ToString();
		}

		public String InvokePluginMethodPlaceholder(String pluginId, String methodName, String argumentsJson)
		{
			//TODO: Implement actual method invocation on the specified plugin using reflection, parsing argumentsJson as needed to match the method signature. This is a placeholder to demonstrate the concept.
			//TODO: Consider adding check for recursion or loops if the invoked method can call back into MCP tools, to avoid infinite loops.
			return $"Plugin invocation placeholder. Plugin='{pluginId}', Method='{methodName}', Args='{argumentsJson}'";
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