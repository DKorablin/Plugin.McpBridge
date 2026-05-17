using System.Diagnostics;
using Microsoft.Extensions.AI;
using SAL.Flatbed;

namespace Plugin.McpBridge.Tools;

/// <summary>Catches tool exceptions and returns the message as a string so the LLM receives a result rather than a broken conversation.</summary>
internal sealed class ToolFacade : DelegatingAIFunction
{
	private readonly ITraceSource _trace;
	private readonly Dictionary<String, Object?> _additionalProperties;

	public override IReadOnlyDictionary<String, Object?> AdditionalProperties => this._additionalProperties;

	public ToolFacade(ITraceSource trace, AIFunction function, Boolean confirmationRequired = false)
		: base(function)
	{
		this._additionalProperties = new Dictionary<String, Object?>(function.AdditionalProperties);
		if(confirmationRequired)
			this._additionalProperties["destructiveHint"] = true;//(MCP) destructiveHint - Tool may perform destructive updates (default: true)

		this._trace = trace ?? throw new ArgumentNullException(nameof(trace));
	}

	public ToolFacade(ITraceSource trace, Delegate method, Boolean confirmationRequired = false)
		: this(trace, AIFunctionFactory.Create(method), confirmationRequired)
	{
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
		} catch(Exception exc)
		{
			this._trace.TraceData(TraceEventType.Error, 0, exc);
			throw;
		}
	}
}
