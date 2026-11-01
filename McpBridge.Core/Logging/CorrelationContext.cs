namespace McpBridge.Core.Logging;

/// <summary>Holds the correlation ID of the user request that is running in the current async flow.</summary>
internal static class CorrelationContext
{
	/// <summary>The HTTP header that carries the correlation ID between processes.</summary>
	public const String HeaderName = "X-Correlation-Id";

	private static readonly AsyncLocal<String?> _current = new AsyncLocal<String?>();

	/// <summary>Gets the correlation ID of the current request, or <see langword="null"/> outside a request or when disabled.</summary>
	public static String? Current => _current.Value;

	/// <summary>Starts a correlation scope for a user request.</summary>
	/// <param name="enabled">The value of <see cref="Settings.CorrelationIdEnabled"/>. When <see langword="false"/> nothing is started.</param>
	/// <param name="existingId">The ID to continue. A new ID is generated when it is missing or not valid.</param>
	/// <returns>A scope that restores the previous ID on dispose, or <see langword="null"/> when disabled.</returns>
	public static IDisposable? BeginRequest(Boolean enabled, String? existingId = null)
	{
		if(!enabled)
			return null;

		Scope scope = new Scope(_current.Value);
		_current.Value = IsValid(existingId) ? existingId : Guid.NewGuid().ToString("N");
		return scope;
	}

	/// <summary>Checks that the value is a GUID. IDs received over HTTP end up in logs, so anything else is rejected.</summary>
	public static Boolean IsValid(String? correlationId)
		=> Guid.TryParse(correlationId, out _);

	private sealed class Scope : IDisposable
	{
		private readonly String? _previous;

		public Scope(String? previous)
			=> this._previous = previous;

		public void Dispose()
			=> _current.Value = this._previous;
	}
}