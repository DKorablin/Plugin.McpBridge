using System.Diagnostics;
using Microsoft.Extensions.AI;
using SAL.Flatbed;

namespace Plugin.McpBridge.Tools;

/// <summary>Catches tool exceptions and returns the message as a string so the LLM receives a result rather than a broken conversation.</summary>
internal sealed class ToolFacade : DelegatingAIFunction
{
	private readonly ITraceSource _trace;

	public ToolFacade(ITraceSource trace, Delegate method, Boolean confirmationRequired = false)
		: base(CreateFunction(method, confirmationRequired))
	{
		this._trace = trace ?? throw new ArgumentNullException(nameof(trace));
	}

	private static AIFunction CreateFunction(Delegate method, Boolean confirmationRequired)
	{
		var function = AIFunctionFactory.Create(method);
		return confirmationRequired
			? new ApprovalRequiredAIFunction(function)
			: function;
	}

	protected override async ValueTask<Object?> InvokeCoreAsync(AIFunctionArguments arguments, CancellationToken cancellationToken)
	{
		String argString = String.Join(", ", arguments.Select(kv => $"{kv.Key}={kv.Value}"));
		this._trace.TraceEvent(TraceEventType.Verbose, 0, $"[tool] {this.Name} {argString}");
		try
		{
			Stopwatch sw = Stopwatch.StartNew();
			Object? result = await base.InvokeCoreAsync(arguments, cancellationToken);
			if(cancellationToken.IsCancellationRequested)
				return "Operation cancelled.";

			sw.Stop();
			this._trace.TraceEvent(TraceEventType.Verbose, 0, $"[tool result] {result?.GetType()} Elapsed: {sw}");
			return result;
		}catch(Exception exc)
		{
			this._trace.TraceData(TraceEventType.Error, 0, exc);
			throw;
		}
	}
}