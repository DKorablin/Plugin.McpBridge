using System.Runtime.Serialization;

namespace Plugin.McpBridge.Agents;

/// <summary>Serializable snapshot of the settings needed to start the DevUI process.</summary>
[DataContract]
public sealed class DevUIConfig
{
	[DataMember] public Int32 Port { get; set; } = 5050;
	[DataMember] public String? SystemPrompt { get; set; }
	[DataMember] public Int32? MaxTokens { get; set; }
	[DataMember] public String[]? ToolsPermission { get; set; }
	[DataMember] public String[]? PluginsPermission { get; set; }
	[DataMember] public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(100);
	[DataMember] public required DevUIProviderConfig Provider { get; set; }
	[DataMember] public List<DevUIPluginInfo> Plugins { get; set; } = new();
	/// <summary>Base URL of the in-process tool bridge server (e.g. http://localhost:12345). Null when the bridge is not running.</summary>
	[DataMember] public String? BridgeUrl { get; set; }
}

[DataContract]
public sealed class DevUIProviderConfig
{
	[DataMember] public String ProviderType { get; set; } = "OpenAI";
	[DataMember] public String? ModelId { get; set; }
	[DataMember] public String? ApiKey { get; set; }
	[DataMember] public String? DeploymentName { get; set; }
	[DataMember] public String? ModelEndpointUrl { get; set; }
	[DataMember] public Double? Temperature { get; set; }
	[DataMember] public String? ReasoningOutput { get; set; }
	[DataMember] public String? ReasoningEffort { get; set; }
}

[DataContract]
public sealed class DevUIPluginInfo
{
	[DataMember] public required String Id { get; set; }
	[DataMember] public required String Name { get; set; }
	[DataMember] public String? Version { get; set; }
	[DataMember] public Boolean HasSettings { get; set; }
	[DataMember] public Boolean HasMembers { get; set; }
}
