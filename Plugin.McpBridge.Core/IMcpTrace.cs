using System.Diagnostics;

namespace Plugin.McpBridge.Core;

/// <summary>Core-owned trace sink abstraction, decoupling Core from SAL.Flatbed's <c>ITraceSource</c>.</summary>
public interface IMcpTrace
{
	void TraceEvent(TraceEventType eventType, Int32 id, String message);

	void TraceData(TraceEventType eventType, Int32 id, Object data);
}