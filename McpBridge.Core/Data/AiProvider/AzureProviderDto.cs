using System.ComponentModel;
using System.Runtime.Serialization;

namespace McpBridge.Core.Data;

[DataContract]
[TypeConverter(typeof(ExpandableObjectConverter))]
public record AzureProviderDto : NetworkProviderDto;