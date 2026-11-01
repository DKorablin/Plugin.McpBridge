using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SAL.Flatbed;

namespace Plugin.McpBridge;

/// <summary>Adapts SAL.Flatbed's <see cref="ITraceSource"/> to <see cref="ILogger"/> so Core components can write into the plugin trace.</summary>
internal sealed class TraceSourceLogger : ILogger
{
	private readonly ITraceSource _trace;

	public TraceSourceLogger(ITraceSource trace)
		=> this._trace = trace ?? throw new ArgumentNullException(nameof(trace));

	public IDisposable? BeginScope<TState>(TState state) where TState : notnull
		=> null;

	/// <remarks><see cref="ITraceSource"/> does not expose its switch level, so filtering is left to the trace listeners.</remarks>
	public Boolean IsEnabled(LogLevel logLevel)
		=> logLevel != LogLevel.None;

	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, String> formatter)
	{
		if(!this.IsEnabled(logLevel))
			return;

		TraceEventType eventType = logLevel switch
		{
			LogLevel.Trace or LogLevel.Debug => TraceEventType.Verbose,
			LogLevel.Information => TraceEventType.Information,
			LogLevel.Warning => TraceEventType.Warning,
			LogLevel.Error => TraceEventType.Error,
			_ => TraceEventType.Critical,
		};

		this._trace.TraceEvent(eventType, eventId.Id, formatter(state, exception));
		if(exception != null)
			this._trace.TraceData(eventType, eventId.Id, exception);
	}
}
