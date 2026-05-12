using System.Runtime.Serialization;
using Plugin.McpBridge.Data;

namespace Plugin.McpBridge.Agents;

/// <summary>Serializable snapshot of the settings needed to start the DevUI process.</summary>
public sealed class DevUIConfig
{
	public String? DevUiServerUrl { get; set; }
	public String? Instructions { get; set; }
	public Int32? MaxTokens { get; set; }
	public String[]? ToolsPermission { get; set; }
	public String[]? PluginsPermission { get; set; }
	public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(100);
	public required AiProviderDto Provider { get; set; }
	/// <summary>Base URL of the in-process tool bridge server (e.g. http://localhost:12345). Null when the bridge is not running.</summary>
	public String? McpServerUrl { get; set; }
}