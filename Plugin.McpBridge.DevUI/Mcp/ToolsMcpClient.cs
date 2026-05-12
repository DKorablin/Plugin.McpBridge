using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.AI;

namespace Plugin.McpBridge.DevUI.Mcp;

internal sealed class ToolsMcpClient : AIFunction
{
	private readonly String _name;
	private readonly String _description;
	private readonly JsonElement _schema;
	private readonly McpSession _session;

	public override String Name => this._name;
	public override String Description => this._description;
	public override JsonElement JsonSchema => this._schema;

	private ToolsMcpClient(String name, String description, JsonElement schema, McpSession session)
	{
		this._name = name;
		this._description = description;
		this._schema = schema;
		this._session = session;
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
	internal static async Task<IReadOnlyList<AIFunction>> FetchAllAsync(String baseUrl, HttpClient http, CancellationToken cancellationToken = default)
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
				["clientInfo"] = new JsonObject { ["name"] = "Plugin.McpBridge.DevUI", ["version"] = "1.0" },
				["capabilities"] = new JsonObject(),
			},
		};
		await session.SendAsync(initRequest["id"]!.GetValue<Int32>(), initRequest, cancellationToken);

		JsonObject initializedNotification = new JsonObject { ["jsonrpc"] = "2.0", ["method"] = "notifications/initialized" };
		await session.NotifyAsync(initializedNotification, cancellationToken);

		Int32 listId = session.NextId();
		JsonObject listRequest = new JsonObject { ["jsonrpc"] = "2.0", ["id"] = listId, ["method"] = "tools/list" };
		JsonObject? listResult = await session.SendAsync(listId, listRequest, cancellationToken);

		List<ToolsMcpClient> tools = new List<ToolsMcpClient>();
		if(listResult?["tools"] is JsonArray arr)
			foreach(JsonNode? item in arr)
			{
				String name = item?["name"]?.GetValue<String>() ?? String.Empty;
				String description = item?["description"]?.GetValue<String>() ?? String.Empty;
				JsonElement schema = JsonSerializer.Deserialize<JsonElement>(item?["inputSchema"]?.ToJsonString() ?? "{}");
				tools.Add(new ToolsMcpClient(name, description, schema, session));
			}

		return tools;
	}

	private static JsonObject BuildArgsNode(AIFunctionArguments arguments)
	{
		JsonObject obj = new JsonObject();
		foreach(KeyValuePair<String, Object?> kv in arguments)
			obj[kv.Key] = kv.Value == null ? null : JsonNode.Parse(JsonSerializer.Serialize(kv.Value));
		return obj;
	}
}