namespace Plugin.McpBridge.Data;

internal class AiProviderChangedEventArgs : EventArgs
{
	public enum StateType
	{
		Added,
		Removed,
	}

	public StateType Type { get; }

	public AiProviderDto Provider { get; }

	public AiProviderChangedEventArgs(StateType state, AiProviderDto provider)
	{
		this.Type = state;
		this.Provider = provider ?? throw new ArgumentNullException(nameof(provider));
	}
}