namespace Plugin.McpBridge.Events
{
	/// <summary>Arguments for the AiResponseReceived event.</summary>
	internal sealed class AgentResponseEventArgs : EventArgs
	{
		public String Response { get; }

		public Boolean IsFinal { get; }

		public AgentResponseEventArgs(String response, Boolean isFinal)
		{
			this.Response = response;
			this.IsFinal = isFinal;
		}
	}

	/// <summary>Arguments for the ConfirmationRequired event.</summary>
	internal sealed class AgentConfirmationEventArgs : EventArgs
	{
		private readonly TaskCompletionSource<Boolean> _tcs = new TaskCompletionSource<Boolean>();

		public String ActionDescription { get; }
		public Task<Boolean> ConfirmationTask => this._tcs.Task;

		public AgentConfirmationEventArgs(String actionDescription)
			=> this.ActionDescription = actionDescription ?? throw new ArgumentNullException(nameof(actionDescription));

		public void Confirm(Boolean allowed)
			=> this._tcs.TrySetResult(allowed);

		public void Cancel()
			=> this._tcs.TrySetResult(false);
	}
}