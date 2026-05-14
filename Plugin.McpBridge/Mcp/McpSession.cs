using System.Collections.Concurrent;
using System.Text;
using System.Text.Json.Nodes;

namespace Plugin.McpBridge.Mcp;

/// <summary>Owns the SSE connection and correlates JSON-RPC responses by id.</summary>
public sealed class McpSession : IDisposable
{
	private readonly HttpClient _http;
	private readonly CancellationTokenSource _cts = new CancellationTokenSource();
	private readonly ConcurrentDictionary<Int32, TaskCompletionSource<JsonObject?>> _pending = new();
	private String _messageUrl = String.Empty;
	private Int32 _nextId = 0;

	/// <summary>Initializes a new instance of the McpSession class using the specified HTTP client.</summary>
	/// <param name="http">
	/// The HttpClient instance used to send HTTP requests for this session. Cannot be null.
	/// The caller is responsible for managing the lifetime of the HttpClient instance.
	/// </param>
	public McpSession(HttpClient http)
		=> this._http = http ?? throw new ArgumentNullException(nameof(http));

	/// <summary>Generates and returns the next unique identifier in a thread-safe manner.</summary>
	/// <remarks>
	/// This method is safe to call from multiple threads concurrently.
	/// Identifiers are incremented atomically and are unique within the lifetime of the containing instance.
	/// </remarks>
	/// <returns>
	/// The next unique identifier as a 32-bit integer.
	/// Each call returns a value greater than the previous call.
	/// </returns>
	public Int32 NextId()
		=> Interlocked.Increment(ref this._nextId);

	/// <summary>Establishes an asynchronous connection to the server using Server-Sent Events (SSE).</summary>
	/// <remarks>
	/// This method initiates a persistent SSE connection and starts processing incoming events in the background.
	/// If the cancellation token is triggered before the connection is established, the operation is canceled.
	/// Subsequent calls to this method without disconnecting may result in multiple concurrent connections.
	/// </remarks>
	/// <param name="ct">A cancellation token that can be used to cancel the connection attempt.</param>
	/// <returns>A task that represents the asynchronous connect operation.</returns>
	public async Task ConnectAsync(CancellationToken ct)
	{
		HttpResponseMessage sse = await this._http.GetAsync("sse", HttpCompletionOption.ResponseHeadersRead, ct);
		sse.EnsureSuccessStatusCode();

		StreamReader reader = new StreamReader(await sse.Content.ReadAsStreamAsync(ct), Encoding.UTF8);
		this._messageUrl = await ReadEndpointEventAsync(reader, ct);

		_ = Task.Run(() => this.SseReaderLoopAsync(reader, sse, this._cts.Token), this._cts.Token);
	}

	/// <summary>Sends a JSON-RPC message asynchronously and waits for the corresponding response.</summary>
	/// <remarks>
	/// If the operation is canceled via the provided cancellation token, the returned task will be canceled.
	/// The caller is responsible for handling any exceptions resulting from unsuccessful HTTP responses.
	/// </remarks>
	/// <param name="id">
	/// The unique identifier for the JSON-RPC request.
	/// This value is used to correlate the response with the request.
	/// </param>
	/// <param name="body">The JSON object representing the request payload to send. Cannot be null.</param>
	/// <param name="ct">A cancellation token that can be used to cancel the operation.</param>
	/// <returns>
	/// A task that represents the asynchronous operation.
	/// The task result contains the JSON object returned by the server, or null if the response does not contain a result.
	/// </returns>
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

	/// <summary>Sends an asynchronous HTTP POST request with the specified JSON payload to the configured message endpoint.</summary>
	/// <param name="body">The JSON object to include in the body of the HTTP POST request. Cannot be null.</param>
	/// <param name="ct">A cancellation token that can be used to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public async Task NotifyAsync(JsonObject body, CancellationToken ct)
	{
		using StringContent content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json");
		using HttpResponseMessage response = await this._http.PostAsync(this._messageUrl, content, ct);
		response.EnsureSuccessStatusCode();
	}

	/// <summary>
	/// Continuously reads and processes Server-Sent Events (SSE) messages from the specified stream until cancellation is requested or the stream ends.
	/// </summary>
	/// <remarks>
	/// This method processes SSE events in real time, matching incoming messages to pending requests and completing their associated tasks.
	/// All pending operations are cancelled and resources are disposed when the reading loop exits.
	/// </remarks>
	/// <param name="reader">
	/// The StreamReader instance from which SSE lines are read.
	/// Must not be null and must be positioned at the start of the SSE response body.
	/// </param>
	/// <param name="sse">
	/// The HttpResponseMessage representing the active SSE HTTP response.
	/// Used for resource cleanup when reading completes.
	/// </param>
	/// <param name="ct">A CancellationToken that can be used to request cancellation of the reading loop.</param>
	/// <returns>
	/// A task that represents the asynchronous operation of reading and handling SSE messages.
	/// The task completes when reading is finished or cancelled.
	/// </returns>
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
		} catch(OperationCanceledException)
		{
			// Normal shutdown
		} finally
		{
			foreach(TaskCompletionSource<JsonObject?> tcs in this._pending.Values)
				tcs.TrySetCanceled();
			this._pending.Clear();
			reader.Dispose();
			sse.Dispose();
		}
	}

	/// <summary>
	/// Asynchronously reads from the specified stream for an event named "endpoint" and returns its associated data payload.
	/// </summary>
	/// <param name="reader">The StreamReader to read lines from. Must not be null and must be positioned at the start of the event stream.</param>
	/// <param name="ct">A CancellationToken that can be used to cancel the asynchronous read operation.</param>
	/// <returns>A string containing the data payload of the first "endpoint" event found in the stream.</returns>
	/// <exception cref="InvalidOperationException">Thrown if the stream does not contain an "endpoint" event before reaching the end of the stream.</exception>
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

	/// <summary>Releases all resources used by the current instance and cancels any ongoing operations.</summary>
	/// <remarks>
	/// Call this method when you are finished using the instance to ensure that all resources are properly
	/// released and any pending operations are cancelled.
	/// After calling this method, the instance should not be used.</remarks>
	public void Dispose()
	{
		this._cts.Cancel();
		this._cts.Dispose();
	}
}