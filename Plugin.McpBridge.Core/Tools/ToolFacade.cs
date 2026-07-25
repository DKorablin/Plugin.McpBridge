using System.Diagnostics;
using Microsoft.Extensions.AI;
using Plugin.McpBridge.Core;

namespace Plugin.McpBridge.Tools;

/// <summary>Catches tool exceptions and returns the message as a string so the LLM receives a result rather than a broken conversation.</summary>
internal class ToolFacade : DelegatingAIFunction
{
	private readonly IMcpTrace _trace;
	private readonly Dictionary<String, Object?> _additionalProperties;

	public override IReadOnlyDictionary<String, Object?> AdditionalProperties => this._additionalProperties;

	public ToolFacade(IMcpTrace trace, AIFunction function, Boolean confirmationRequired = false)
		: base(function)
	{
		this._additionalProperties = new Dictionary<String, Object?>(function.AdditionalProperties);
		if(confirmationRequired)
			this._additionalProperties["destructiveHint"] = true;//(MCP) destructiveHint - Tool may perform destructive updates (default: true)

		this._trace = trace ?? throw new ArgumentNullException(nameof(trace));
	}

	public ToolFacade(IMcpTrace trace, Delegate method, Boolean confirmationRequired = false)
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
			if(result is Array array && result is not String)
				this._trace.TraceEvent(TraceEventType.Verbose, 0, $"[tool result] {result?.GetType()} Length: {array.Length} Elapsed: {sw}");
			else
				this._trace.TraceEvent(TraceEventType.Verbose, 0, $"[tool result] {result?.GetType()} Elapsed: {sw}");
			return result;
		} catch(Exception exc)
		{
			this._trace.TraceData(TraceEventType.Error, 0, exc);
			throw;
		}
	}
}
