namespace Plugin.McpBridge.Data;

[Flags]
public enum ProviderCapabilities
{
	None = 0,
	Chat = 1,
	Embeddings = 2,
}