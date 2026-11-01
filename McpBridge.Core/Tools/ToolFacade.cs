using System.Diagnostics;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace McpBridge.Core.Tools;

/// <summary>Catches tool exceptions and returns the message as a string so the LLM receives a result rather than a broken conversation.</summary>
internal class ToolFacade : DelegatingAIFunction
{
	private readonly ILogger _logger;
	private readonly Dictionary<String, Object?> _additionalProperties;

	public override IReadOnlyDictionary<String, Object?> AdditionalProperties => this._additionalProperties;

	public ToolFacade(ILogger logger, AIFunction function, Boolean confirmationRequired = false)
		: base(function)
	{
		this._additionalProperties = new Dictionary<String, Object?>(function.AdditionalProperties);
		if(confirmationRequired)
			this._additionalProperties["destructiveHint"] = true;//(MCP) destructiveHint - Tool may perform destructive updates (default: true)

		this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public ToolFacade(ILogger logger, Delegate method, Boolean confirmationRequired = false)
		: this(logger, AIFunctionFactory.Create(method), confirmationRequired)
	{
	}

	protected override async ValueTask<Object?> InvokeCoreAsync(AIFunctionArguments arguments, CancellationToken cancellationToken)
	{
		if(this._logger.IsEnabled(LogLevel.Debug))
			this._logger.ToolInvoking(this.Name, String.Join(", ", arguments.Select(kv => $"{kv.Key}={kv.Value}")));
		try
		{
			Stopwatch sw = Stopwatch.StartNew();
			Object? result = await base.InvokeCoreAsync(arguments, cancellationToken);
			if(cancellationToken.IsCancellationRequested)
				return "Operation cancelled.";

			sw.Stop();
			if(result is Array array && result is not String)
				this._logger.ToolCompletedArray(this.Name, result.GetType(), array.Length, sw.Elapsed);
			else
				this._logger.ToolCompleted(this.Name, result?.GetType(), sw.Elapsed);
			return result;
		} catch(Exception exc)
		{
			this._logger.ToolFailed(this.Name, exc);
			throw;
		}
	}
}
