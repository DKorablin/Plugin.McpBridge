using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;

namespace McpBridge.Core.Logging;

/// <summary>Single-line console format with the same layout as the simple console logger, plus the correlation ID of the current request.</summary>
/// <remarks>Reads <see cref="CorrelationContext.Current"/> at write time because the hosts use separate logger pipelines that do not share scopes.</remarks>
internal sealed class CorrelationConsoleFormatter : ConsoleFormatter
{
	public const String FormatterName = "correlation";

	public CorrelationConsoleFormatter()
		: base(FormatterName) { }

	public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter)
	{
		String? message = logEntry.Formatter?.Invoke(logEntry.State, logEntry.Exception);
		if(message == null && logEntry.Exception == null)
			return;

		textWriter.Write(DateTime.Now.ToString("HH:mm:ss "));
		textWriter.Write(GetLevelText(logEntry.LogLevel));
		textWriter.Write(": ");
		textWriter.Write(logEntry.Category);
		textWriter.Write('[');
		textWriter.Write(logEntry.EventId.Id);
		textWriter.Write("] ");

		String? correlationId = CorrelationContext.Current;
		if(correlationId != null)
		{
			textWriter.Write('[');
			textWriter.Write(correlationId);
			textWriter.Write("] ");
		}

		if(message != null)
			textWriter.Write(message.Replace(Environment.NewLine, " "));
		if(logEntry.Exception != null)
		{
			textWriter.Write(' ');
			textWriter.Write(logEntry.Exception.ToString().Replace(Environment.NewLine, " "));
		}
		textWriter.Write(Environment.NewLine);
	}

	private static String GetLevelText(LogLevel logLevel)
		=> logLevel switch
		{
			LogLevel.Trace => "trce",
			LogLevel.Debug => "dbug",
			LogLevel.Information => "info",
			LogLevel.Warning => "warn",
			LogLevel.Error => "fail",
			LogLevel.Critical => "crit",
			_ => "none",
		};
}
