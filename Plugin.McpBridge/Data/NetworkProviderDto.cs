using System.ComponentModel;
using System.Runtime.Serialization;

namespace Plugin.McpBridge.Data;

[DataContract]
[TypeConverter(typeof(ExpandableObjectConverter))]
public record NetworkProviderDto : AiProviderDto
{
	private static class Defaults
	{
		public static readonly TimeSpan ConnectionTimeout = TimeSpan.FromSeconds(100);
	}
	private String? _apiKey;
	private TimeSpan? _connectionTimeout;

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

	[Category("Network")]
	[DefaultValue(typeof(TimeSpan), "00:01:40")]
	[Description("The timeout duration for network connections to the AI provider.")]
	public TimeSpan ConnectionTimeout
	{
		get => this._connectionTimeout ?? Defaults.ConnectionTimeout;
		set
		{
			TimeSpan? nullValue = value <= TimeSpan.Zero ? null : value;
			this.SetField(ref this._connectionTimeout, nullValue, nameof(this.ConnectionTimeout));
		}
	}

	public override String ToString()
		=> base.ToString();
}