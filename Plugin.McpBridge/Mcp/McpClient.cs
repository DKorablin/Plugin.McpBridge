using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.AI;

namespace Plugin.McpBridge.Mcp;

public sealed class McpClient : AIFunction
{
	private readonly String _name;
	private readonly String _description;
	private readonly JsonElement _schema;
	private readonly McpSession _session;
	private readonly Dictionary<String, Object?> _additionalProperties;

	public override String Name => this._name;
	public override String Description => this._description;
	public override JsonElement JsonSchema => this._schema;
	public override IReadOnlyDictionary<String, Object?> AdditionalProperties => this._additionalProperties;

	private McpClient(String name, String description, JsonElement schema, McpSession session, Dictionary<String, Object?> additionalProperties)
	{
		this._name = name;
		this._description = description;
		this._schema = schema;
		this._session = session;
		this._additionalProperties = additionalProperties;
	}

	protected override async ValueTask<Object?> InvokeCoreAsync(AIFunctionArguments arguments, CancellationToken cancellationToken)
	{
		Int32 id = this._session.NextId();
		JsonObject requestBody = new JsonObject
		{
			["jsonrpc"] = "2.0",
			["id"] = id,
			["method"] = "tools/call",
			["params"] = new JsonObject
			{
				["name"] = this._name,
				["arguments"] = BuildArgsNode(arguments),
			},
		};

		JsonObject? result = await this._session.SendAsync(id, requestBody, cancellationToken);
		return result?["content"]?[0]?["text"]?.GetValue<String>() ?? String.Empty;
	}

	/// <summary>Connects to the MCP server at <paramref name="baseUrl"/> via SSE and fetches all available tools.</summary>
	public static async Task<AITool[]> FetchAllAsync(String applicationName, HttpClient http, CancellationToken cancellationToken = default)
	{
		McpSession session = new McpSession(http);
		await session.ConnectAsync(cancellationToken);

		JsonObject initRequest = new JsonObject
		{
			["jsonrpc"] = "2.0",
			["id"] = session.NextId(),
			["method"] = "initialize",
			["params"] = new JsonObject
			{
				["protocolVersion"] = "2024-11-05",
				["clientInfo"] = new JsonObject { ["name"] = applicationName, ["version"] = "1.0" },
				["capabilities"] = new JsonObject(),
			},
		};
		await session.SendAsync(initRequest["id"]!.GetValue<Int32>(), initRequest, cancellationToken);

		JsonObject initializedNotification = new JsonObject { ["jsonrpc"] = "2.0", ["method"] = "notifications/initialized" };
		await session.NotifyAsync(initializedNotification, cancellationToken);

		Int32 listId = session.NextId();
		JsonObject listRequest = new JsonObject { ["jsonrpc"] = "2.0", ["id"] = listId, ["method"] = "tools/list" };
		JsonObject? listResult = await session.SendAsync(listId, listRequest, cancellationToken);

		List<AITool> tools = new List<AITool>();
		if(listResult?["tools"] is JsonArray arr)
			foreach(JsonNode? item in arr)
			{
				String name = item?["name"]?.GetValue<String>() ?? String.Empty;
				String description = item?["description"]?.GetValue<String>() ?? String.Empty;
				JsonElement schema = JsonSerializer.Deserialize<JsonElement>(item?["inputSchema"]?.ToJsonString() ?? "{}");

				Dictionary<String, Object?> props = new Dictionary<String, Object?>();
				if(item?["annotations"] is JsonObject annotations)
					foreach(KeyValuePair<String, JsonNode?> kv in annotations)
						props[kv.Key] = kv.Value?.GetValue<Object>();

				AIFunction aIFunction = new McpClient(name, description, schema, session, props);

				//(2025-03-26 MCP) destructiveHint - Tool may perform destructive updates (default: true)
				if(aIFunction.AdditionalProperties.TryGetValue("destructiveHint", out Object? v)
					&& v is JsonElement e
					&& e.GetBoolean())
					aIFunction = new ApprovalRequiredAIFunction(aIFunction);

				tools.Add(aIFunction);
			}

		return tools.ToArray();
	}

	private static JsonObject BuildArgsNode(AIFunctionArguments arguments)
	{
		JsonObject obj = new JsonObject();
		foreach(KeyValuePair<String, Object?> kv in arguments)
			obj[kv.Key] = kv.Value == null ? null : JsonNode.Parse(JsonSerializer.Serialize(kv.Value));
		return obj;
	}
}
