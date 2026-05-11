using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.AI;

namespace Plugin.McpBridge.DevUI;

/// <summary>An <see cref="AIFunction"/> proxy that forwards invocations to the plugin-side <see cref="PluginToolBridge"/> over HTTP.</summary>
internal sealed class BridgeProxyTool : AIFunction
{
	private readonly String _name;
	private readonly String _description;
	private readonly JsonElement _schema;
	private readonly String _invokeUrl;
	private readonly HttpClient _http;

	public override String Name => this._name;
	public override String Description => this._description;
	public override JsonElement JsonSchema => this._schema;

	internal BridgeProxyTool(String name, String description, JsonElement schema, String baseUrl, HttpClient http)
	{
		this._name = name;
		this._description = description;
		this._schema = schema;
		this._invokeUrl = $"{baseUrl}/bridge/tools/{name}";
		this._http = http;
	}

	protected override async ValueTask<Object?> InvokeCoreAsync(AIFunctionArguments arguments, CancellationToken cancellationToken)
	{
		Dictionary<String, Object?> argsDict = arguments.ToDictionary(kv => kv.Key, kv => kv.Value);
		String body = JsonSerializer.Serialize(argsDict);
		using StringContent content = new StringContent(body, Encoding.UTF8, "application/json");
		using HttpResponseMessage response = await this._http.PostAsync(this._invokeUrl, content, cancellationToken);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadAsStringAsync(cancellationToken);
	}

	/// <summary>Fetches all bridge tools from the running <see cref="PluginToolBridge"/> at <paramref name="baseUrl"/>.</summary>
	internal static async Task<IReadOnlyList<BridgeProxyTool>> FetchAllAsync(String baseUrl, HttpClient http, CancellationToken cancellationToken = default)
	{
		String json = await http.GetStringAsync($"{baseUrl}/bridge/tools", cancellationToken);
		using JsonDocument doc = JsonDocument.Parse(json);

		List<BridgeProxyTool> result = new List<BridgeProxyTool>();
		foreach(JsonElement item in doc.RootElement.EnumerateArray())
		{
			String name = item.GetProperty("name").GetString() ?? String.Empty;
			String description = item.GetProperty("description").GetString() ?? String.Empty;
			JsonElement schema = item.GetProperty("schema").Clone();
			result.Add(new BridgeProxyTool(name, description, schema, baseUrl, http));
		}

		return result;
	}
}
