using System.ComponentModel;
using System.Runtime.Serialization;

namespace Plugin.McpBridge.Data;

[DataContract]
[TypeConverter(typeof(ExpandableObjectConverter))]
public record AiProviderDto : INotifyPropertyChanged
{
	private static class Defaults
	{
		public const AiProviderType ProviderType = AiProviderType.OpenAI;
	}

	private AiProviderType _providerType = Defaults.ProviderType;
	private String? _modelId = null;
	private String? _apiKey;
	private String? _deploymentName;
	private String? _modelEndpointUrl = null;

	/// <summary>Gets the unique identifier for this instance.</summary>
	[DataMember]
	[Browsable(false)]
	public Guid Id { get; init; } = Guid.NewGuid();

	/// <summary>Selects the provider profile used to initialize the AI client.</summary>
	[DataMember]
	[Category("AI Provider")]
	[DefaultValue(Defaults.ProviderType)]
	[Description("Selects the provider profile (OpenAI, Azure OpenAI, Local/OpenAI-compatible, Qwen-compatible, Grok-compatible, Gemini-compatible, custom compatible).")]
	public AiProviderType ProviderType
	{
		get => _providerType;
		set => this.SetField(ref _providerType, value, nameof(this.ProviderType));
	}

	/// <summary>The AI model identifier used for chat completions.</summary>
	[DataMember]
	[Category("AI Provider")]
	[Description("The AI model identifier or Azure OpenAI deployment name used for chat completions (e.g. gpt-4o-mini).")]
	public String? ModelId
	{
		get => _modelId;
		set => this.SetField(ref _modelId, value, nameof(this.ModelId));
	}

	/// <summary>The API key used to authenticate with the AI provider.</summary>
	[DataMember]
	[Category("AI Provider")]
	[Description("The API key used to authenticate with the AI provider.")]
	public String? ApiKey
	{
		get => _apiKey;
		set => this.SetField(ref _apiKey, value, nameof(this.ApiKey));
	}

	/// <summary>The Azure OpenAI deployment name found in Azure OpenAI Studio under Deployments or organization identifier supported by some OpenAI-compatible providers.</summary>
	[DataMember]
	[Category("AI Provider")]
	[DisplayName("Deplymanet Name / Organization ID")]
	[Description("The deployment name from Azure OpenAI Studio (Deployments section). Required when using the Azure OpenAI provider.\r\nOrganization identifier supported by some OpenAI-compatible providers.")]
	public String? DeploymentName
	{
		get => _deploymentName;
		set => this.SetField(ref _deploymentName, value, nameof(this.DeploymentName));
	}

	/// <summary>Optional custom OpenAI-compatible chat completions endpoint URL.</summary>
	[DataMember]
	[Category("AI Provider")]
	[Description("Optional custom OpenAI-compatible chat completions endpoint URL. Required for Azure OpenAI and most non-OpenAI providers.")]
	public String? ModelEndpointUrl
	{
		get => this._modelEndpointUrl;
		set
		{
			if(String.IsNullOrWhiteSpace(value))
				value = null;
			else if(!Uri.IsWellFormedUriString(value, UriKind.Absolute))
				throw new ArgumentException("ModelEndpointUrl must be an absolute URL.", nameof(this.ModelEndpointUrl));

			this.SetField(ref this._modelEndpointUrl, value, nameof(this.ModelEndpointUrl));
		}
	}

	public override String ToString()
		=> this.ModelId == null
			? this.ProviderType.ToString()
			: $"{this.ProviderType} ({this.ModelId})";

	#region INotifyPropertyChanged
	public event PropertyChangedEventHandler? PropertyChanged;
	private Boolean SetField<T>(ref T field, T value, String propertyName)
	{
		if(EqualityComparer<T>.Default.Equals(field, value))
			return false;

		field = value;
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		return true;
	}
	#endregion INotifyPropertyChanged
}