namespace McpBridge.Core.Logging;

/// <summary>Sends the current correlation ID with each outgoing request so the receiving process logs under the same ID.</summary>
internal sealed class CorrelationIdHandler : DelegatingHandler
{
	public CorrelationIdHandler(HttpMessageHandler innerHandler)
		: base(innerHandler) { }

	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		String? correlationId = CorrelationContext.Current;
		if(correlationId != null)
			request.Headers.TryAddWithoutValidation(CorrelationContext.HeaderName, correlationId);

		return base.SendAsync(request, cancellationToken);
	}
}