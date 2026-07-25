using Microsoft.Extensions.AI;

namespace Plugin.McpBridge.Events
{
	/// <summary>Arguments for the AiResponseReceived event.</summary>
	public class AgentResponseEventArgs : EventArgs
	{
		public Boolean IsFinal { get; }

		public ChatMessage? Message { get; }

		public AgentResponseEventArgs(ChatMessage? message, Boolean isFinal)
		{
			this.Message = message;
			this.IsFinal = isFinal;
		}
	}

	/// <summary>Arguments for the ConfirmationRequired event.</summary>
	public class AgentConfirmationEventArgs : EventArgs
	{
		private readonly TaskCompletionSource<Boolean> _tcs = new TaskCompletionSource<Boolean>();

		public String ActionDescription { get; }
		public Task<Boolean> ConfirmationTask => this._tcs.Task;

		public AgentConfirmationEventArgs(String actionDescription)
			=> this.ActionDescription = actionDescription ?? throw new ArgumentNullException(nameof(actionDescription));

		public AgentConfirmationEventArgs(FunctionCallContent request)
		{
			String method = request.Name;
			String args = String.Join(", ", request.Arguments?.Select(kv => $"{kv.Key}={kv.Value}") ?? []);
			this.ActionDescription = $"{method} {args}";
		}

		public void Confirm(Boolean allowed)
			=> this._tcs.TrySetResult(allowed);

		public void Cancel()
			=> this._tcs.TrySetResult(false);
	}
}