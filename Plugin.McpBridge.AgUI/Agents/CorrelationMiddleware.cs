using System.Collections.Concurrent;
using System.Text.Json;
using McpBridge.Core.Logging;

namespace Plugin.McpBridge.AgUI.Agents;

/// <summary>Starts a correlation scope for each AG-UI run and continues it when the client answers a tool approval.</summary>
/// <remarks>
/// A tool approval ends the current POST and the answer arrives in a new one, but both belong to the same user request.
/// The scope has to start here, not in <see cref="ApprovalMiddleware"/>: changes to an AsyncLocal made inside an async iterator are lost at each yield.
/// </remarks>
internal static class CorrelationMiddleware
{
	private static readonly ConcurrentDictionary<String, String> _pendingApprovals = new ConcurrentDictionary<String, String>();

	/// <summary>Remembers the current correlation ID until the client answers the approval.</summary>
	public static void RegisterPendingApproval(String approvalId)
	{
		String? correlationId = CorrelationContext.Current;
		if(correlationId != null)
			_pendingApprovals[approvalId] = correlationId;
	}

	/// <summary>Installs the middleware for POST requests to the given AG-UI path.</summary>
	public static void UseCorrelationId(this WebApplication app, String path)
		=> app.Use(async (context, next) =>
		{
			if(!HttpMethods.IsPost(context.Request.Method) || !context.Request.Path.Equals(path, StringComparison.OrdinalIgnoreCase))
			{
				await next(context);
				return;
			}

			String? pendingId = await ResolvePendingApprovalAsync(context.Request);
			using IDisposable? correlation = CorrelationContext.BeginRequest(true, pendingId);
			await next(context);
		});

	/// <summary>Finds the correlation ID of the approval answered by this request, leaving the body readable for the AG-UI endpoint.</summary>
	private static async Task<String?> ResolvePendingApprovalAsync(HttpRequest request)
	{
		request.EnableBuffering();
		try
		{
			using JsonDocument document = await JsonDocument.ParseAsync(request.Body, cancellationToken: request.HttpContext.RequestAborted);
			if(document.RootElement.ValueKind != JsonValueKind.Object
				|| !document.RootElement.TryGetProperty("messages", out JsonElement messages)
				|| messages.ValueKind != JsonValueKind.Array)
				return null;

			String? correlationId = null;
			foreach(JsonElement message in messages.EnumerateArray())
				if(message.ValueKind == JsonValueKind.Object
					&& message.TryGetProperty("role", out JsonElement role) && role.ValueEquals("tool")
					&& message.TryGetProperty("toolCallId", out JsonElement toolCallId) && toolCallId.ValueKind == JsonValueKind.String
					&& _pendingApprovals.TryRemove(toolCallId.GetString()!, out String? pendingId))
					correlationId = pendingId;

			return correlationId;
		} catch(JsonException)
		{// The AG-UI endpoint reports malformed requests
			return null;
		} finally
		{
			request.Body.Position = 0;
		}
	}
}
