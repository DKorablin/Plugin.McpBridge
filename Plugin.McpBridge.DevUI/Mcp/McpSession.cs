using System.Collections.Concurrent;
using System.Text;
using System.Text.Json.Nodes;

namespace Plugin.McpBridge.DevUI.Mcp;

/// <summary>Owns the SSE connection and correlates JSON-RPC responses by id.</summary>
internal sealed class McpSession : IDisposable
{
	private readonly HttpClient _http;
	private readonly CancellationTokenSource _cts = new CancellationTokenSource();
	private readonly ConcurrentDictionary<Int32, TaskCompletionSource<JsonObject?>> _pending = new();
	private String _messageUrl = String.Empty;
	private Int32 _nextId = 0;

	public McpSession(HttpClient http) => this._http = http;

	public Int32 NextId() => Interlocked.Increment(ref this._nextId);

	public async Task ConnectAsync(CancellationToken ct)
	{
		HttpResponseMessage sse = await this._http.GetAsync("sse", HttpCompletionOption.ResponseHeadersRead, ct);
		sse.EnsureSuccessStatusCode();

		StreamReader reader = new StreamReader(await sse.Content.ReadAsStreamAsync(ct), Encoding.UTF8);
		this._messageUrl = await ReadEndpointEventAsync(reader, ct);

		_ = Task.Run(() => this.SseReaderLoopAsync(reader, sse, this._cts.Token), this._cts.Token);
	}

	public async Task<JsonObject?> SendAsync(Int32 id, JsonObject body, CancellationToken ct)
	{
		TaskCompletionSource<JsonObject?> tcs = new TaskCompletionSource<JsonObject?>(TaskCreationOptions.RunContinuationsAsynchronously);
		this._pending[id] = tcs;

		using CancellationTokenRegistration reg = ct.Register(() =>
		{
			this._pending.TryRemove(id, out _);
			tcs.TrySetCanceled();
		});

		using StringContent content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json");
		using HttpResponseMessage response = await this._http.PostAsync(this._messageUrl, content, ct);
		response.EnsureSuccessStatusCode();

		return await tcs.Task;
	}

	public async Task NotifyAsync(JsonObject body, CancellationToken ct)
	{
		using StringContent content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json");
		using HttpResponseMessage response = await this._http.PostAsync(this._messageUrl, content, ct);
		response.EnsureSuccessStatusCode();
	}

	private async Task SseReaderLoopAsync(StreamReader reader, HttpResponseMessage sse, CancellationToken ct)
	{
		try
		{
			String? eventName = null;
			while(!ct.IsCancellationRequested)
			{
				String? line = await reader.ReadLineAsync(ct);
				if(line == null) break;
				if(line.StartsWith("event:", StringComparison.Ordinal))
					eventName = line["event:".Length..].Trim();
				else if(line.StartsWith("data:", StringComparison.Ordinal) && eventName == "message")
				{
					String data = line["data:".Length..].Trim();
					if(JsonNode.Parse(data) is JsonObject msg && msg["id"] != null)
					{
						Int32 id = msg["id"]!.GetValue<Int32>();
						if(this._pending.TryRemove(id, out TaskCompletionSource<JsonObject?>? tcs))
						{
							if(msg["error"] is JsonObject err)
								tcs.TrySetException(new InvalidOperationException(err["message"]?.GetValue<String>() ?? "Unknown MCP error"));
							else
								tcs.TrySetResult(msg["result"] as JsonObject);
						}
					}
					eventName = null;
				}
			}
		} catch(OperationCanceledException) { } finally
		{
			foreach(TaskCompletionSource<JsonObject?> tcs in this._pending.Values)
				tcs.TrySetCanceled();
			this._pending.Clear();
			reader.Dispose();
			sse.Dispose();
		}
	}

	private static async Task<String> ReadEndpointEventAsync(StreamReader reader, CancellationToken ct)
	{
		String? eventName = null;
		while(true)
		{
			String? line = await reader.ReadLineAsync(ct);
			if(line == null) break;
			if(line.StartsWith("event:", StringComparison.Ordinal))
				eventName = line["event:".Length..].Trim();
			else if(line.StartsWith("data:", StringComparison.Ordinal) && eventName == "endpoint")
				return line["data:".Length..].Trim();
		}
		throw new InvalidOperationException("MCP server did not send an endpoint event.");
	}

	public void Dispose()
	{
		this._cts.Cancel();
		this._cts.Dispose();
	}
}