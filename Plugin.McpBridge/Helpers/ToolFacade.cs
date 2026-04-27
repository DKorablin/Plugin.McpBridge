using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.AI;
using Plugin.McpBridge.Events;

namespace Plugin.McpBridge.Helpers;

/// <summary>Catches tool exceptions and returns the message as a string so the LLM receives a result rather than a broken conversation.</summary>
internal sealed class ToolFacade : DelegatingAIFunction
{
	private readonly TraceSource _trace;

	public event EventHandler<AgentConfirmationEventArgs>? ConfirmationRequired;

	public ToolFacade(TraceSource trace, Delegate method)
		: base(AIFunctionFactory.Create(method))
	{
		this._trace = trace ?? throw new ArgumentNullException(nameof(trace));
	}

	protected override async ValueTask<Object?> InvokeCoreAsync(AIFunctionArguments arguments, CancellationToken cancellationToken)
	{
		String argString = String.Join(", ", arguments.Select(kv => $"{kv.Key}={kv.Value}"));
		this._trace.TraceEvent(TraceEventType.Verbose, 0, $"[tool] {this.Name} {argString}");
		try
		{
			if(this.ConfirmationRequired != null)
			{
				Boolean confirmed = await this.RequestConfirmationAsync($"{this.Name} {argString}");
				if(!confirmed)
					return "Operation declined by user.";
			}

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

	private Task<Boolean> RequestConfirmationAsync(String actionDescription)
	{
		AgentConfirmationEventArgs confirmArgs = new AgentConfirmationEventArgs(actionDescription);
		this.ConfirmationRequired?.Invoke(this, confirmArgs);
		return confirmArgs.ConfirmationTask;
	}

	internal static String GetArgString(AIFunctionArguments arguments, String key)
	{
		Object? value = arguments.FirstOrDefault(kv => kv.Key == key).Value;
		return value is JsonElement je && je.ValueKind == JsonValueKind.String ? je.GetString() ?? String.Empty : value?.ToString() ?? String.Empty;
	}
}