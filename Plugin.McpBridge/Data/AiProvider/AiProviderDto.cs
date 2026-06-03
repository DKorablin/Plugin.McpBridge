using System.ComponentModel;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.Extensions.AI;

namespace Plugin.McpBridge.Data;

[DataContract]
[KnownType(typeof(CoPilotProviderDto))]
[KnownType(typeof(NetworkProviderDto))]
[KnownType(typeof(AzureProviderDto))]
[KnownType(typeof(StubProviderDto))]
[TypeConverter(typeof(ExpandableObjectConverter))]
public record AiProviderDto : INotifyPropertyChanged
{
	private static class Defaults
	{
		public const AiProviderType ProviderType = AiProviderType.OpenAI;
	}

	private AiProviderType _providerType = Defaults.ProviderType;
	private String? _modelId = null;
	private String? _modelEndpointUrl = null;
	private Double? _temperature;
	private Int32? _maxTokens;

	private ReasoningOutput? _reasoningOutput = null;
	private ReasoningEffort? _reasoningEffort = null;

	/// <summary>Gets the unique identifier for this instance.</summary>
	[DataMember]
	[ReadOnly(true)]
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

	/// <summary>The maximum number of tokens to generate in a single response.</summary>
	[Category("Debugging")]
	[Description("The maximum number of tokens to generate in a single response. Leave empty for the model default.")]
	public Int32? MaxTokens
	{
		get => this._maxTokens;
		set
		{
			if(value == null || value <= 0)
				value = null;

			this.SetField(ref this._maxTokens, value, nameof(this.MaxTokens));
		}
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

	/// <summary>Returns a new instance of the appropriate derived type for <paramref name="source"/>.ProviderType, with base properties copied.</summary>
	internal static AiProviderDto Morph(AiProviderDto source)
	{
		Type targetType = source.ProviderType switch
		{
			AiProviderType.CoPilot => typeof(CoPilotProviderDto),
			AiProviderType.Azure => typeof(AzureProviderDto),
			AiProviderType.Gemini => typeof(NetworkProviderDto),
			AiProviderType.Grok => typeof(NetworkProviderDto),
			AiProviderType.OpenAI => typeof(NetworkProviderDto),
			AiProviderType.Stub => typeof(StubProviderDto),
			_ => typeof(AiProviderDto)
		};
		if(source.GetType() == targetType)
			return source;

		AiProviderDto result = (AiProviderDto?)Activator.CreateInstance(targetType)
			?? throw new InvalidOperationException($"Failed to create instance of type {targetType.FullName}.");

		result.ProviderType = source.ProviderType;
		foreach(PropertyInfo prop in source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
		{
			PropertyInfo? target = result.GetType().GetProperty(prop.Name, BindingFlags.Public | BindingFlags.Instance);
			if(target?.CanWrite == true)
				target.SetValue(result, prop.GetValue(source));
		}
		return result;
	}

	#region INotifyPropertyChanged
	public event PropertyChangedEventHandler? PropertyChanged;
	protected Boolean SetField<T>(ref T field, T value, String propertyName)
	{
		if(field is Array a && value is Array b
			? a.Cast<Object>().SequenceEqual(b.Cast<Object>())
			: EqualityComparer<T>.Default.Equals(field, value))
			return false;

		field = value;
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		return true;
	}
	#endregion INotifyPropertyChanged
}