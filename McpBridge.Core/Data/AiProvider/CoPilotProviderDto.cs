using System.ComponentModel;
using System.Runtime.Serialization;

namespace McpBridge.Core.Data;

/// <summary>Provider configuration for the GitHub Copilot CLI.</summary>
[DataContract]
[TypeConverter(typeof(ExpandableObjectConverter))]
public sealed record CoPilotProviderDto : AiProviderDto;