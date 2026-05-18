using System.ComponentModel;
using System.Runtime.Serialization;

namespace Plugin.McpBridge.Data;

[DataContract]
[TypeConverter(typeof(ExpandableObjectConverter))]
public record NetworkProviderDto : AiProviderDto
{
	private String? _apiKey;

	/// <summary>The API key used to authenticate with the AI provider.</summary>
	[DataMember]
	[Category("AI Provider")]
	[Description("The API key used to authenticate with the AI provider.")]
	[DefaultValue(null)]
	public String? ApiKey
	{
		get => _apiKey;
		set
		{
			if(String.IsNullOrWhiteSpace(value))
				value = null;

			this.SetField(ref _apiKey, value, nameof(this.ApiKey));
		}
	}

	public override String ToString()
		=> base.ToString();
}