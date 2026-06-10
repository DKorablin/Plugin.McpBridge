using System.ComponentModel;
using System.Reflection;
using System.Runtime.Serialization;

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
		public const ProviderCapabilities Capabilities = ProviderCapabilities.Chat | ProviderCapabilities.Embeddings;
	}

	private String? _description = null;
	private AiProviderType _providerType = Defaults.ProviderType;
	private ConnectionSettings _connection = CreateDefaultConnectionSettings(Defaults.ProviderType);
	private ChatSettings _chat = new ChatSettings();
	private EmbeddingSettings _embeddings = new EmbeddingSettings();
	private ProviderCapabilities _capabilities = Defaults.Capabilities;
	private Double? _temperature;
	private Int32? _maxTokens;

	public AiProviderDto()
	{
		AiProviderDto.SubscribeToNestedSettings(this._connection, this.Connection_PropertyChanged);
		AiProviderDto.SubscribeToNestedSettings(this._chat, this.Chat_PropertyChanged);
		AiProviderDto.SubscribeToNestedSettings(this._embeddings, this.Embeddings_PropertyChanged);
	}

	/// <summary>Gets the unique identifier for this instance.</summary>
	[DataMember]
	[ReadOnly(true)]
	public Guid Id { get; init; } = Guid.NewGuid();

	[DataMember]
	public String? Description
	{
		get => this._description;
		set
		{
			if(String.IsNullOrWhiteSpace(value))
				value = null;
			this.SetField(ref this._description, value, nameof(this.Description));
		}
	}

	/// <summary>Selects the provider profile used to initialize the AI client.</summary>
	[DataMember]
	[Category("AI Provider")]
	[DefaultValue(Defaults.ProviderType)]
	[Description("Selects the provider profile (OpenAI, Azure OpenAI, Local/OpenAI-compatible, Qwen-compatible, Grok-compatible, Gemini-compatible, custom compatible).")]
	public AiProviderType ProviderType
	{
		get => _providerType;
		set
		{
			if(!this.SetField(ref _providerType, value, nameof(this.ProviderType)))
				return;

			ConnectionSettings targetConnection = CreateDefaultConnectionSettings(value);
			if(this._connection?.GetType() != targetConnection.GetType())
				this.Connection = targetConnection;

			ProviderCapabilities normalized = NormalizeCapabilities(value, this._capabilities);
			if(this._capabilities != normalized)
			{
				this._capabilities = normalized;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Capabilities)));
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Chat)));
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Embeddings)));
			}
		}
	}

	[DataMember]
	[Category("Capabilities")]
	[Description("Enables the AI client capabilities supported by this provider profile. Chat selection lists only providers with Chat enabled, while embedding selection lists only providers with Embeddings enabled.")]
	[DefaultValue(Defaults.Capabilities)]
	public ProviderCapabilities Capabilities
	{
		get => this._capabilities;
		set
		{
			ProviderCapabilities normalized = NormalizeCapabilities(this.ProviderType, value);
			if(!this.SetField(ref this._capabilities, normalized, nameof(this.Capabilities)))
				return;

			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Chat)));
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Embeddings)));
		}
	}

	[DataMember]
	[Category("Connection")]
	[Description("Connection settings specific to the selected provider type.")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public ConnectionSettings Connection
	{
		get => this._connection;
		set
		{
			ConnectionSettings normalized = value ?? CreateDefaultConnectionSettings(this.ProviderType);
			if(ReferenceEquals(this._connection, normalized))
				return;

			AiProviderDto.SubscribeToNestedSettings(this._connection, this.Connection_PropertyChanged, false);
			this._connection = normalized;
			AiProviderDto.SubscribeToNestedSettings(this._connection, this.Connection_PropertyChanged);
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Connection)));
		}
	}

	[DataMember]
	[Category("Chat")]
	[Description("Chat model settings for this provider profile.")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public ChatSettings Chat
	{
		get => this._chat;
		set
		{
			ChatSettings normalized = value ?? new ChatSettings();
			if(ReferenceEquals(this._chat, normalized))
				return;

			AiProviderDto.SubscribeToNestedSettings(this._chat, this.Chat_PropertyChanged, false);
			this._chat = normalized;
			AiProviderDto.SubscribeToNestedSettings(this._chat, this.Chat_PropertyChanged);
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Chat)));
		}
	}

	[DataMember]
	[Category("Embeddings")]
	[Description("Embedding model settings for this provider profile.")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public EmbeddingSettings Embeddings
	{
		get => this._embeddings;
		set
		{
			EmbeddingSettings normalized = value ?? new EmbeddingSettings();
			if(ReferenceEquals(this._embeddings, normalized))
				return;

			AiProviderDto.SubscribeToNestedSettings(this._embeddings, this.Embeddings_PropertyChanged, false);
			this._embeddings = normalized;
			AiProviderDto.SubscribeToNestedSettings(this._embeddings, this.Embeddings_PropertyChanged);
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Embeddings)));
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

	public override String ToString()
	{
		if(this.Description != null)
			return this.Description;
		if(this.Chat?.ModelId != null)
			return $"{this.ProviderType} ({this.Chat.ModelId})";

		return this.ProviderType.ToString();
	}

	/// <summary>Returns a new instance of the appropriate derived type for <paramref name="source"/>.ProviderType, with base properties copied.</summary>
	internal static AiProviderDto Morph(AiProviderDto source)
	{
		Type targetType = source.ProviderType switch
		{
			AiProviderType.CoPilot => typeof(CoPilotProviderDto),
			AiProviderType.Azure => typeof(AzureProviderDto),
			AiProviderType.Gemini => typeof(NetworkProviderDto),
			AiProviderType.Grok => typeof(NetworkProviderDto),
			AiProviderType.Local => typeof(NetworkProviderDto),
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

	public Boolean SupportsCapability(ProviderCapabilities capability)
		=> (this.Capabilities & capability) == capability;

	private static ConnectionSettings CreateDefaultConnectionSettings(AiProviderType providerType)
		=> providerType == AiProviderType.CoPilot
			? new CoPilotConnectionSettings()
			: new NetworkConnectionSettings();

	private static ProviderCapabilities NormalizeCapabilities(AiProviderType providerType, ProviderCapabilities value)
	{
		ProviderCapabilities allowed = GetAllowedCapabilities(providerType);
		ProviderCapabilities normalized = value & allowed;
		return normalized == ProviderCapabilities.None
			? GetDefaultCapabilities(providerType)
			: normalized;
	}

	private static ProviderCapabilities GetDefaultCapabilities(AiProviderType providerType)
		=> providerType is AiProviderType.CoPilot or AiProviderType.Stub
			? ProviderCapabilities.Chat
			: ProviderCapabilities.Chat | ProviderCapabilities.Embeddings;

	private static ProviderCapabilities GetAllowedCapabilities(AiProviderType providerType)
		=> providerType is AiProviderType.CoPilot or AiProviderType.Stub
			? ProviderCapabilities.Chat
			: ProviderCapabilities.Chat | ProviderCapabilities.Embeddings;

	private void Connection_PropertyChanged(Object? sender, PropertyChangedEventArgs e)
		=> this.ForwardNestedPropertyChanged(nameof(this.Connection), e.PropertyName);

	private void Chat_PropertyChanged(Object? sender, PropertyChangedEventArgs e)
		=> this.ForwardNestedPropertyChanged(nameof(this.Chat), e.PropertyName);

	private void Embeddings_PropertyChanged(Object? sender, PropertyChangedEventArgs e)
		=> this.ForwardNestedPropertyChanged(nameof(this.Embeddings), e.PropertyName);

	private void ForwardNestedPropertyChanged(String parentPropertyName, String? childPropertyName)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(parentPropertyName));
		if(!String.IsNullOrWhiteSpace(childPropertyName))
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"{parentPropertyName}.{childPropertyName}"));
	}

	private static void SubscribeToNestedSettings(INotifyPropertyChanged? target, PropertyChangedEventHandler handler, Boolean subscribe = true)
	{
		if(target == null)
			return;

		if(subscribe)
			target.PropertyChanged += handler;
		else
			target.PropertyChanged -= handler;
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