using System.ComponentModel;
using System.Runtime.Serialization;
using Microsoft.Extensions.AI;

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
	private Double? _temperature;

	private ReasoningOutput? _reasoningOutput = null;
	private ReasoningEffort? _reasoningEffort = null;

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
	[DefaultValue(null)]
	public String? ModelId
	{
		get => _modelId;
		set
		{
			if(String.IsNullOrWhiteSpace(value))
				value = null;

			this.SetField(ref _modelId, value, nameof(this.ModelId));
		}
	}

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

	/// <summary>The Azure OpenAI deployment name found in Azure OpenAI Studio under Deployments or organization identifier supported by some OpenAI-compatible providers.</summary>
	[DataMember]
	[Category("AI Provider")]
	[DisplayName("Deplymanet Name / Organization ID")]
	[Description("The deployment name from Azure OpenAI Studio (Deployments section). Required when using the Azure OpenAI provider.\r\nOrganization identifier supported by some OpenAI-compatible providers.")]
	[DefaultValue(null)]
	public String? DeploymentName
	{
		get => _deploymentName;
		set
		{
			if(String.IsNullOrWhiteSpace(value))
				value = null;

			this.SetField(ref _deploymentName, value, nameof(this.DeploymentName));
		}
	}

	/// <summary>Optional custom OpenAI-compatible chat completions endpoint URL.</summary>
	[DataMember]
	[Category("AI Provider")]
	[Description("Optional custom OpenAI-compatible chat completions endpoint URL. Required for Azure OpenAI and most non-OpenAI providers.")]
	[DefaultValue(null)]
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

	/// <summary>The sampling temperature controlling randomness in responses (0.0–2.0).</summary>
	[DataMember]
	[Category("Debugging")]
	[Description("The sampling temperature controlling randomness in responses. Lower values produce more deterministic output (0.0–2.0).")]
	public Double? Temperature
	{
		get => this._temperature;
		set => this.SetField(ref this._temperature, value, nameof(this.Temperature));
	}

	[DataMember]
	[Category("Debugging")]
	[Description("When enabled, the plugin will include the reasoning steps taken by the assistant in the response. This can be useful for debugging and understanding how the assistant arrived at its conclusions.")]
	[DefaultValue(null)]
	public ReasoningOutput? ReasoningOutput
	{
		get => this._reasoningOutput;
		set
		{
			if(value == Microsoft.Extensions.AI.ReasoningOutput.None)
				value = null;
			this.SetField(ref this._reasoningOutput, value, nameof(this.ReasoningOutput));
		}
	}

	[DataMember]
	[Category("Debugging")]
	[Description("Controls the level of effort the assistant should use when reasoning through a problem.")]
	[DefaultValue(null)]
	public ReasoningEffort? ReasoningEffort
	{
		get => this._reasoningEffort;
		set
		{
			if(value == Microsoft.Extensions.AI.ReasoningEffort.None)
				value = null;
			this.SetField(ref this._reasoningEffort, value, nameof(this.ReasoningEffort));
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