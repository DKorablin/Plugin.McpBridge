using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.AI;
using SAL.Flatbed;

namespace Plugin.McpBridge.Mcp;

/// <summary>MCP-compliant HTTP/SSE server (JSON-RPC 2.0) that exposes plugin tools to the out-of-process DevUI executable.</summary>
internal sealed class McpServer : IDisposable
{
	private readonly ITraceSource _trace;
	private readonly String _mcpServerUrl;
	private readonly HttpListener _listener;
	private readonly Dictionary<String, AIFunction> _tools;
	private readonly CancellationTokenSource _cts = new CancellationTokenSource();
	private readonly ConcurrentDictionary<String, HttpListenerResponse> _sessions = new();

	private enum RpcErrorCodes
	{
		ParseError = -32700,
		InvalidRequest = -32600,
		MethodNotFound = -32601,
		InvalidParams = -32602,
		InternalError = -32603,
	}

	public McpServer(ITraceSource trace, String mcpServerUrl, IEnumerable<AITool> tools)
	{
		this._trace = trace ?? throw new ArgumentNullException(nameof(trace));
		this._mcpServerUrl = mcpServerUrl ?? throw new ArgumentNullException(nameof(mcpServerUrl));

		this._listener = new HttpListener();
		this._listener.Prefixes.Add(this._mcpServerUrl + "/");
		this._trace.TraceEvent(System.Diagnostics.TraceEventType.Start, 0, $"MCP server started at {this._mcpServerUrl}");

		this._tools = tools.OfType<AIFunction>()
			.ToDictionary(t => t.Name, t => t, StringComparer.OrdinalIgnoreCase);
	}

	public void Start()
	{
		this._listener.Start();
		_ = Task.Run(() => this.AcceptLoopAsync(this._cts.Token));
	}

	private async Task AcceptLoopAsync(CancellationToken ct)
	{
		while(!ct.IsCancellationRequested)
		{
			try
			{
				HttpListenerContext ctx = await this._listener.GetContextAsync();
				_ = Task.Run(() => this.HandleAsync(ctx, ct), ct);
			} catch(HttpListenerException) when(ct.IsCancellationRequested)
			{
				break;
			} catch(ObjectDisposedException)
			{
				break;
			}
		}
	}

	private async Task HandleAsync(HttpListenerContext ctx, CancellationToken ct)
	{
		try
		{
			String path = ctx.Request.Url?.AbsolutePath ?? "/";
			String method = ctx.Request.HttpMethod;

			if(method == "GET" && path == "/sse")
				await this.HandleSseAsync(ctx, ct);
			else if(method == "POST" && path == "/message")
				await this.HandleMessageAsync(ctx, ct);
			else
			{
				ctx.Response.StatusCode = (Int32)HttpStatusCode.NotFound;
				ctx.Response.Close();
			}
		} catch(Exception ex)
		{
			this._trace.TraceEvent(System.Diagnostics.TraceEventType.Error, 0, ex.Message);

			ctx.Response.StatusCode = (Int32)HttpStatusCode.InternalServerError;
			ctx.Response.Close();
		}
	}

	/// <summary>Opens an SSE stream for the client and sends the MCP <c>endpoint</c> event.</summary>
	private async Task HandleSseAsync(HttpListenerContext ctx, CancellationToken ct)
	{
		String sessionId = Guid.NewGuid().ToString("N");

		ctx.Response.ContentType = "text/event-stream";
		ctx.Response.Headers["Cache-Control"] = "no-cache";
		ctx.Response.Headers["X-Accel-Buffering"] = "no";
		ctx.Response.SendChunked = true;

		this._sessions[sessionId] = ctx.Response;
		try
		{
			await WriteSseEventAsync(ctx.Response, "endpoint", $"{this._mcpServerUrl}/message?sessionId={sessionId}", ct);
			await Task.Delay(Timeout.Infinite, ct);
		} catch(OperationCanceledException) { }
		finally
		{
			this._sessions.TryRemove(sessionId, out _);
			ctx.Response.Close();
		}
	}

	/// <summary>Receives a JSON-RPC 2.0 message from the client and dispatches it.</summary>
	private async Task HandleMessageAsync(HttpListenerContext ctx, CancellationToken ct)
	{
		String sessionId = ctx.Request.QueryString["sessionId"] ?? String.Empty;
		if(!this._sessions.TryGetValue(sessionId, out HttpListenerResponse? sseResponse))
		{
			ctx.Response.StatusCode = (Int32)HttpStatusCode.BadRequest;
			ctx.Response.Close();
			return;
		}

		using StreamReader reader = new StreamReader(ctx.Request.InputStream, Encoding.UTF8);
		String body = await reader.ReadToEndAsync(ct);
		ctx.Response.StatusCode = (Int32)HttpStatusCode.Accepted;
		ctx.Response.Close();

		JsonNode? msg = JsonNode.Parse(body);
		if(msg == null)
		{
			await WriteSseEventAsync(sseResponse, "message", BuildError(null, RpcErrorCodes.ParseError, "Parse error: invalid JSON").ToJsonString(), ct);
			return;
		}

		String? method = msg["method"]?.GetValue<String>();
		JsonNode? id = msg["id"];

		try
		{
			JsonNode? result = method switch
			{
				"initialize" => this.HandleInitialize(),
				"tools/list" => this.HandleToolsList(),
				"tools/call" => await this.HandleToolsCallAsync(msg["params"], ct),
				"notifications/initialized" => null,
				_ => null,
			};

			if(id != null && result != null)
			{
				JsonObject response = new JsonObject
				{
					["jsonrpc"] = "2.0",
					["id"] = id.DeepClone(),
					["result"] = result,
				};
				await WriteSseEventAsync(sseResponse, "message", response.ToJsonString(), ct);
			}
		}catch(Exception exc)
		{
			this._trace.TraceEvent(System.Diagnostics.TraceEventType.Error, 0, exc.Message);
			if(id == null) return;

			await WriteSseEventAsync(sseResponse, "message", BuildError(id, exc).ToJsonString(), ct);
		}
	}

	private JsonObject HandleInitialize()
		=> new JsonObject
		{
			["protocolVersion"] = "2025-03-26",
			["serverInfo"] = new JsonObject { ["name"] = typeof(McpServer).Assembly.GetName().Name, ["version"] = "1.0" },
			["capabilities"] = new JsonObject { ["tools"] = new JsonObject() },
		};

	private JsonObject HandleToolsList()
	{
		JsonArray toolsArray = new JsonArray();
		foreach(AIFunction tool in this._tools.Values)
		{
			//(2025-03-26 MCP) requiresApproval - Tool may perform destructive updates (default: true)
			Boolean requiresApproval = tool.AdditionalProperties.TryGetValue("requiresApproval", out Object? v) && v is true;
			toolsArray.Add(new JsonObject
			{
				["name"] = tool.Name,
				["description"] = tool.Description ?? String.Empty,
				["inputSchema"] = JsonNode.Parse(tool.JsonSchema.GetRawText()),
				["annotations"] = new JsonObject
				{
					["requiresApproval"] = requiresApproval,
				},
			});
		}
		return new JsonObject { ["tools"] = toolsArray };
	}

	private async Task<JsonObject> HandleToolsCallAsync(JsonNode? paramsNode, CancellationToken ct)
	{
		String toolName = paramsNode?["name"]?.GetValue<String>() ?? String.Empty;
		if(!this._tools.TryGetValue(toolName, out AIFunction? tool))
			return BuildError(null, RpcErrorCodes.InvalidParams, $"Unknown tool: {toolName}");

		JsonNode? argsNode = paramsNode?["arguments"];
		AIFunctionArguments arguments = new AIFunctionArguments();
		if(argsNode is JsonObject argsObj)
			foreach(KeyValuePair<String, JsonNode?> kv in argsObj)
				arguments[kv.Key] = kv.Value != null
					? JsonSerializer.Deserialize<JsonElement>(kv.Value.ToJsonString())
					: (Object?)null;

		Object? result = await tool.InvokeAsync(arguments, ct);
		String jsonResult = result == null ? String.Empty : JsonSerializer.Serialize(result);
		return new JsonObject
		{
			["content"] = new JsonArray
			{
				new JsonObject { ["type"] = "text", ["text"] = jsonResult }
			}
		};
	}

	private static JsonObject BuildError(JsonNode? id, RpcErrorCodes code, String message)
		=> new JsonObject
		{
			["jsonrpc"] = "2.0",
			["id"] = id?.DeepClone(),
			["error"] = new JsonObject { ["code"] = (Int32)code, ["message"] = message },
		};
	private static JsonObject BuildError(JsonNode? id, Exception exc)
		=> new JsonObject
		{
			["jsonrpc"] = "2.0",
			["id"] = id?.DeepClone(),
			["error"] = new JsonObject { ["code"] = (Int32)RpcErrorCodes.InternalError, ["message"] = exc.Message, ["data"] = exc.StackTrace, },
		};

	private static async Task WriteSseEventAsync(HttpListenerResponse response, String eventName, String data, CancellationToken ct)
	{
		String payload = $"event: {eventName}\ndata: {data}\n\n";
		Byte[] bytes = Encoding.UTF8.GetBytes(payload);
		await response.OutputStream.WriteAsync(bytes, ct);
		await response.OutputStream.FlushAsync(ct);
	}

	public void Dispose()
	{
		this._cts.Cancel();
		this._listener.Stop();
		this._listener.Close();
		this._cts.Dispose();
	}
}