namespace Plugin.McpBridge.RAGHost;

internal sealed class RagSyncOptions
{
	public TimeSpan DebounceInterval { get; set; } = TimeSpan.FromSeconds(1);
}
