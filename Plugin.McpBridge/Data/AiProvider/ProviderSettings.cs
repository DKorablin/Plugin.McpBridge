using System.ComponentModel;
using System.Drawing.Design;
using System.Runtime.Serialization;
using Microsoft.Extensions.AI;
using Plugin.McpBridge.RAG;
using Plugin.McpBridge.UI.PropertyGrid;

namespace Plugin.McpBridge.Data;

[DataContract]
[KnownType(typeof(NetworkConnectionSettings))]
[KnownType(typeof(CoPilotConnectionSettings))]
[TypeConverter(typeof(ExpandableObjectConverter))]
public record ConnectionSettings : INotifyPropertyChanged
{
	private static class Defaults
	{
		public static readonly TimeSpan Timeout = TimeSpan.FromSeconds(100);
	}

	private TimeSpan? _timeout;

	[DataMember]
	[Category("Connection")]
	[DefaultValue(typeof(TimeSpan), "00:01:40")]
	[Description("The timeout duration for network or process connections.")]
	public TimeSpan Timeout
	{
		get => this._timeout ?? Defaults.Timeout;
		set
		{
			TimeSpan? normalized = value <= TimeSpan.Zero ? null : value;
			this.SetField(ref this._timeout, normalized, nameof(this.Timeout));
		}
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

[DataContract]
[TypeConverter(typeof(ExpandableObjectConverter))]
public sealed record NetworkConnectionSettings : ConnectionSettings
{
	private String? _endpointUrl;
	private String? _apiKey;

	[DataMember]
	[Category("Connection")]
	[Description("Optional endpoint URL for OpenAI-compatible providers. Required for Azure and most local/self-hosted providers.")]
	[DefaultValue(null)]
	public String? EndpointUrl
	{
		get => this._endpointUrl;
		set
		{
			if(String.IsNullOrWhiteSpace(value))
				value = null;
			else if(!Uri.IsWellFormedUriString(value, UriKind.Absolute))
				throw new ArgumentException("EndpointUrl must be an absolute URL.", nameof(this.EndpointUrl));

			this.SetField(ref this._endpointUrl, value, nameof(this.EndpointUrl));
		}
	}

	[DataMember]
	[Category("Authentication")]
	[Description("The API key used to authenticate with the provider. Leave empty for local providers that do not require a key.")]
	[DefaultValue(null)]
	public String? ApiKey
	{
		get => this._apiKey;
		set
		{
			String? normalized = String.IsNullOrWhiteSpace(value) ? null : value;
			this.SetField(ref this._apiKey, normalized, nameof(this.ApiKey));
		}
	}
}

[DataContract]
[TypeConverter(typeof(ExpandableObjectConverter))]
public sealed record CoPilotConnectionSettings : ConnectionSettings
{
	private String? _coPilotPath;
	private String? _gitHubToken;

	[DataMember]
	[Category("Configuration")]
	[Description("The absolute path to the GitHub Copilot CLI executable. Leave empty to use environment discovery.")]
	[DefaultValue(null)]
	public String? CoPilotPath
	{
		get => this._coPilotPath;
		set
		{
			String? normalized = String.IsNullOrWhiteSpace(value) ? null : value;
			this.SetField(ref this._coPilotPath, normalized, nameof(this.CoPilotPath));
		}
	}

	[DataMember]
	[Category("Authentication")]
	[DisplayName("GitHub Token")]
	[Description("The GitHub token used to authenticate with the GitHub Copilot CLI. Leave empty to use environment or GitHub CLI auth.")]
	[DefaultValue(null)]
	public String? GitHubToken
	{
		get => this._gitHubToken;
		set
		{
			String? normalized = String.IsNullOrWhiteSpace(value) ? null : value;
			this.SetField(ref this._gitHubToken, normalized, nameof(this.GitHubToken));
		}
	}
}

[DataContract]
[TypeConverter(typeof(ExpandableObjectConverter))]
public sealed record ChatSettings : INotifyPropertyChanged
{
	private String? _modelId;
	private ReasoningOutput? _reasoningOutput = null;
	private ReasoningEffort? _reasoningEffort = null;

	[DataMember]
	[Category("Chat")]
	[DisplayName("Model")]
	[Description("The chat model identifier or deployment name used for chat completions.")]
	[DefaultValue(null)]
	public String? ModelId
	{
		get => this._modelId;
		set
		{
			String? normalized = String.IsNullOrWhiteSpace(value) ? null : value;
			this.SetField(ref this._modelId, normalized, nameof(this.ModelId));
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

	#region INotifyPropertyChanged
	public event PropertyChangedEventHandler? PropertyChanged;
	private void SetField<T>(ref T field, T value, String propertyName)
	{
		if(field is Array a && value is Array b
			? a.Cast<Object>().SequenceEqual(b.Cast<Object>())
			: EqualityComparer<T>.Default.Equals(field, value))
			return;

		field = value;
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
	#endregion INotifyPropertyChanged
}

[DataContract]
[TypeConverter(typeof(ExpandableObjectConverter))]
public sealed record EmbeddingSettings : INotifyPropertyChanged
{
	private static class Defaults
	{
		public const UInt16 TopResults = 3;
	}
	private String? _modelId;
	private Int32? _dimension;
	private UInt16? _topResults;

	[DataMember]
	[Category("Embeddings")]
	[DisplayName("Model")]
	[Editor(typeof(EmbeddingModelEditor), typeof(UITypeEditor))]
	[Description("The embedding model identifier or deployment name.")]
	public String? ModelId
	{
		get => this._modelId;
		set
		{
			String? normalized = String.IsNullOrWhiteSpace(value) ? null : value;
			this.SetField(ref this._modelId, normalized, nameof(this.ModelId));
		}
	}

	[DataMember]
	[Category("Embeddings")]
	[Description("The embedding vector dimension. If not set, known model defaults are inferred when available.")]
	public Int32? Dimension
	{
		get
		{
			if(this._dimension == null && this.ModelId != null)
				return EmbeddingModel.GetDimention(this.ModelId);
			return this._dimension;
		}
		set
		{
			Int32? normalized = value <= 0 ? null : value;
			this.SetField(ref this._dimension, normalized, nameof(this.Dimension));
		}
	}

	[DataMember]
	[Category("Embeddings")]
	[Description("The number of top similar results to return for embedding similarity queries. If not set, default value is used.")]
	[DefaultValue(Defaults.TopResults)]
	public UInt16 TopResults
	{
		get => this._topResults ?? Defaults.TopResults;
		set
		{
			UInt16? normalized = value <= 0 ? null : value;
			this.SetField(ref this._topResults, normalized, nameof(this.TopResults));
		}
	}

	#region INotifyPropertyChanged
	public event PropertyChangedEventHandler? PropertyChanged;
	private void SetField<T>(ref T field, T value, String propertyName)
	{
		if(field is Array a && value is Array b
			? a.Cast<Object>().SequenceEqual(b.Cast<Object>())
			: EqualityComparer<T>.Default.Equals(field, value))
			return;

		field = value;
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
	#endregion INotifyPropertyChanged
}