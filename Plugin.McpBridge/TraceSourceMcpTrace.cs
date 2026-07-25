using System.Diagnostics;
using Plugin.McpBridge.Core;
using SAL.Flatbed;

namespace Plugin.McpBridge;

/// <summary>Adapts SAL.Flatbed's <see cref="ITraceSource"/> to the Core-owned <see cref="IMcpTrace"/> abstraction.</summary>
internal sealed class TraceSourceMcpTrace : IMcpTrace
{
	private readonly ITraceSource _trace;

	public TraceSourceMcpTrace(ITraceSource trace)
		=> this._trace = trace ?? throw new ArgumentNullException(nameof(trace));

	public void TraceEvent(TraceEventType eventType, Int32 id, String message)
		=> this._trace.TraceEvent(eventType, id, message);

	public void TraceData(TraceEventType eventType, Int32 id, Object data)
		=> this._trace.TraceData(eventType, id, data);
}