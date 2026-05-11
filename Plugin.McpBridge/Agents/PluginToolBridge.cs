using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.AI;
using SAL.Flatbed;

namespace Plugin.McpBridge.Agents;

/// <summary>Lightweight HTTP bridge that exposes plugin tools to the out-of-process DevUI executable.</summary>
internal sealed class PluginToolBridge : IDisposable
{
	private readonly ITraceSource _trace;
	private readonly HttpListener _listener;
	private readonly Dictionary<String, AIFunction> _tools;
	private readonly CancellationTokenSource _cts = new CancellationTokenSource();

	public String BaseUrl { get; }

	public PluginToolBridge(ITraceSource trace, IReadOnlyList<AITool> tools)
	{
		this._trace = trace ?? throw new ArgumentNullException(nameof(trace));
		Int32 port = FindFreePort();
		this.BaseUrl = $"http://localhost:{port}";
		this._listener = new HttpListener();
		this._listener.Prefixes.Add(this.BaseUrl + "/");
		this._trace.TraceEvent(System.Diagnostics.TraceEventType.Start, 0, $"DevUI bridge started at http://localhost:{port}");

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
			}
			catch(HttpListenerException) when(ct.IsCancellationRequested) { break; }
			catch(ObjectDisposedException) { break; }
		}
	}

	private async Task HandleAsync(HttpListenerContext ctx, CancellationToken ct)
	{
		try
		{
			String path = ctx.Request.Url?.AbsolutePath ?? "/";
			String method = ctx.Request.HttpMethod;

			if(method == "GET" && path == "/bridge/tools")
				await this.WriteToolListAsync(ctx.Response, ct);
			else if(method == "POST" && path.StartsWith("/bridge/tools/", StringComparison.OrdinalIgnoreCase))
				await this.InvokeToolAsync(ctx, path["/bridge/tools/".Length..], ct);
			else
			{
				ctx.Response.StatusCode = (Int32)HttpStatusCode.NotFound;
				ctx.Response.Close();
			}
		}
		catch
		{
			try { ctx.Response.StatusCode = (Int32)HttpStatusCode.InternalServerError; ctx.Response.Close(); } catch { }
		}
	}

	private async Task WriteToolListAsync(HttpListenerResponse response, CancellationToken ct)
	{
		var list = this._tools.Values.Select(t => new
		{
			name = t.Name,
			description = t.Description ?? String.Empty,
			schema = t.JsonSchema,
		});

		Byte[] bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(list));
		response.ContentType = "application/json; charset=utf-8";
		response.ContentLength64 = bytes.Length;
		await response.OutputStream.WriteAsync(bytes, ct);
		response.Close();
	}

	private async Task InvokeToolAsync(HttpListenerContext ctx, String toolName, CancellationToken ct)
	{
		if(!this._tools.TryGetValue(toolName, out AIFunction? tool))
		{
			ctx.Response.StatusCode = (Int32)HttpStatusCode.NotFound;
			ctx.Response.Close();
			return;
		}

		using StreamReader reader = new StreamReader(ctx.Request.InputStream, Encoding.UTF8);
		String body = await reader.ReadToEndAsync(ct);

		Dictionary<String, JsonElement>? argsRaw = String.IsNullOrWhiteSpace(body)
			? null
			: JsonSerializer.Deserialize<Dictionary<String, JsonElement>>(body);

		AIFunctionArguments arguments = argsRaw == null
			? new AIFunctionArguments()
			: new AIFunctionArguments(argsRaw.ToDictionary(kvp => kvp.Key, kvp => (Object)kvp.Value.Clone()));

		Object? result = await tool.InvokeAsync(arguments, ct);
		String resultStr = result?.ToString() ?? String.Empty;

		Byte[] bytes = Encoding.UTF8.GetBytes(resultStr);
		ctx.Response.ContentType = "text/plain; charset=utf-8";
		ctx.Response.ContentLength64 = bytes.Length;
		await ctx.Response.OutputStream.WriteAsync(bytes, ct);
		ctx.Response.Close();
	}

	public void Dispose()
	{
		this._cts.Cancel();
		this._listener.Stop();
		this._listener.Close();
		this._cts.Dispose();
	}

	private static Int32 FindFreePort()
	{
		using TcpListener probe = new TcpListener(IPAddress.Loopback, 0);
		probe.Start();
		Int32 port = ((IPEndPoint)probe.LocalEndpoint).Port;
		probe.Stop();
		return port;
	}
}
