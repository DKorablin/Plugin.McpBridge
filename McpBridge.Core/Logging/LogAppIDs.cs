namespace McpBridge.Shared.Logging;

/// <summary>Base EventId for each component's logs.</summary>
/// <remarks>Each component owns a block of 1000 IDs. Never change an existing value; add new components at the end.</remarks>
internal static class LogAppIDs
{
	public const Int32 Core = 1000;
	public const Int32 Plugin = 2000;
	public const Int32 Rag = 3000;
	public const Int32 AgUI = 4000;
	public const Int32 DevUI = 5000;
	public const Int32 Dts = 6000;
}