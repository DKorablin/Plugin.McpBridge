using System.Text.Json;
using System.Text.Json.Nodes;
using McpBridge.Core;
using Microsoft.Extensions.AI;
using SAL.Flatbed;

namespace Plugin.McpBridge.Tools;

/// <summary>Wraps an <see cref="IPluginMethodInfo"/> as an <see cref="AIFunction"/>, building its JSON schema and handling argument deserialization directly.</summary>
internal class PluginMethodAIFunction : AIFunction
{
	private readonly IPluginMethodInfo _method;
	private readonly JsonElement _schema;

	public override String Name { get; }
	public override String Description { get; }
	public override JsonElement JsonSchema => this._schema;

	public PluginMethodAIFunction(String name, String description, IPluginMethodInfo method, IReadOnlyDictionary<String, String>? paramDescriptions = null)
	{
		this.Name = name;
		this.Description = description;
		this._method = method ?? throw new ArgumentNullException(nameof(method));
		this._schema = BuildSchema(method, paramDescriptions);
	}

	protected override ValueTask<Object?> InvokeCoreAsync(AIFunctionArguments arguments, CancellationToken cancellationToken)
	{
		IPluginParameterInfo[] parameters = this._method.GetParameters().ToArray();
		Object?[] args = new Object?[parameters.Length];
		for(Int32 i = 0; i < parameters.Length; i++)
		{
			Type? paramType = Type.GetType(parameters[i].AssemblyQualifiedName);
			if(paramType == null)
				continue;

			args[i] = arguments.TryGetValue(parameters[i].Name, out Object? value)
				? value is JsonElement je ? Utils.ConvertValue(je.GetRawText(), paramType) : Convert.ChangeType(value, paramType)
				: paramType.IsValueType ? Activator.CreateInstance(paramType) : null;
		}

		return ValueTask.FromResult(this._method.Invoke(args));
	}

	private static JsonElement BuildSchema(IPluginMethodInfo method, IReadOnlyDictionary<String, String>? paramDescriptions)
	{
		JsonObject properties = new JsonObject();
		List<String> required = new List<String>();

		foreach(IPluginParameterInfo param in method.GetParameters())
		{
			Type? paramType = Type.GetType(param.AssemblyQualifiedName);
			if(paramType == null)
				continue;

			String? paramDescription = null;
			paramDescriptions?.TryGetValue(param.Name, out paramDescription);
			JsonElement typeSchema = AIJsonUtilities.CreateJsonSchema(paramType, paramDescription, false, null, null, null);
			properties[param.Name] = JsonNode.Parse(typeSchema.GetRawText());

			String[] defaults = param.GetDefaultValues();
			if(defaults == null || defaults.Length == 0)
				required.Add(param.Name);
		}

		JsonObject schema = new JsonObject { ["type"] = "object", ["properties"] = properties };
		if(required.Count > 0)
			schema["required"] = new JsonArray(required.Select(r => (JsonNode?)JsonValue.Create(r)).ToArray());

		return JsonDocument.Parse(schema.ToJsonString()).RootElement;
	}
}
