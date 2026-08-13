using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Text.Json;
using McpBridge.Core;
using McpBridge.Core.Tools;
using SAL.Flatbed;

namespace Plugin.McpBridge.Tools;

internal class PluginMethodsTools : ToolsDiscoveryBase
{
	private readonly IHost _host;

	public PluginMethodsTools(IHost host)
	{
		this._host = host ?? throw new ArgumentNullException(nameof(host));
	}

	[Tool]
	[Description("List all callable methods for a plugin")]
	public Task<String> MethodsList([Description("Plugin identifier")] String pluginId)
	{
		var pluginDescription = this._host.Plugins[pluginId];
		if(pluginDescription == null)
			return Task.FromResult($"Plugin with ID '{pluginId}' was not found.");

		IPluginMemberInfo[] callableMembers = PluginMethodsToolsExtractor.GetCallableMembers(pluginDescription).ToArray();
		if(callableMembers.Length == 0)
			return Task.FromResult($"Plugin '{pluginDescription.ID}' does not expose any callable methods.");

		StringBuilder builder = new StringBuilder();
		builder.Append("Callable methods for plugin '");
		builder.Append(pluginDescription.ID);
		builder.Append("' (");
		builder.Append(pluginDescription.Name);
		builder.AppendLine("):");
		foreach(IPluginMemberInfo member in callableMembers)
		{
			if(member.MemberType == MemberTypes.Method)
			{
				builder.Append("- ");
				builder.Append(member.Name);
				//var methodDescription = this._xmlReader.FindDocumentation(pluginDescription, member);

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

	[Tool(confirmationRequired: true)]
	[Description("Invoke a plugin method with arguments provided as JSON; requires user confirmation")]
	public Task<Object?> MethodsInvoke(
		[Description("Plugin identifier")] String pluginId,
		[Description("Method name")] String methodName,
		[Description("Arguments as JSON")] String argumentsJson,
		CancellationToken cancellationToken = default)
	{
		var pluginDescription = this._host.Plugins[pluginId]
			?? throw new ArgumentException($"Plugin '{pluginId}' was not found.");

		var member = PluginMethodsToolsExtractor.GetCallableMembers(pluginDescription).FirstOrDefault(m => m.Name == methodName)
			?? throw new ArgumentException($"Method '{methodName}' was not found in plugin '{pluginId}'.");

		if(member.MemberType == MemberTypes.Method)
		{
			var method = (IPluginMethodInfo)member;
			var arguments = ConvertArgumentsValue(method, argumentsJson);
			var result = method.Invoke(arguments);

			return Task.FromResult<Object?>(result);
		}

		var exc = new ArgumentException($"Unsupported member type '{member.MemberType}' for method invocation. Only methods are supported.");
		exc.Data.Add(nameof(pluginId), pluginId);
		exc.Data.Add(nameof(methodName), methodName);
		exc.Data.Add(nameof(argumentsJson), argumentsJson);
		throw exc;
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

				if(root.TryGetProperty(argument.Name, out JsonElement element))
				{
					Type targetType = Type.GetType(argument.AssemblyQualifiedName, true)
						?? throw new InvalidOperationException($"Could not resolve type '{argument.TypeName}' for argument '{argument.Name}'.");

					result[loop] = Utils.ConvertValue(element.GetRawText(), targetType);
				} else
					result[loop] = null; // Or handle missing arguments as needed
			}

			return result;
		}
	}
}