using System.ComponentModel;
using System.Runtime.Serialization;

namespace Plugin.McpBridge.Data;

[DataContract]
[TypeConverter(typeof(ExpandableObjectConverter))]
public record NetworkProviderDto : AiProviderDto;