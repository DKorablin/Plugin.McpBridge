using System.ComponentModel;
using System.Drawing.Design;
using System.Runtime.Serialization;
using Plugin.McpBridge.RAG;
using Plugin.McpBridge.UI.PropertyGrid;

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
	private String? _embeddingModelId;
	private Int32? _embeddingModelDimention;
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

	/// <summary>The AI model identifier used for text embeddings.</summary>
	[DataMember]
	[Category("RAG")]
	[Editor(typeof(EmbeddingModelEditor), typeof(UITypeEditor))]
	[Description("The AI model identifier used for text embeddings (e.g. text-embedding-3-large).")]
	public String? EmbeddingModelId
	{
		get => this._embeddingModelId;
		set
		{
			if(String.IsNullOrWhiteSpace(value))
				value = null;

			this.SetField(ref this._embeddingModelId, value, nameof(this.EmbeddingModelId));
		}
	}

	[DataMember]
	[Category("RAG")]
	[Description("The dimension of the embeddings produced by the specified embedding model. If not set explicitly, it will be inferred based on the EmbeddingModelId if possible.")]
	public Int32? EmbeddingModelDimention
	{
		get
		{
			if(this._embeddingModelDimention == null && this.EmbeddingModelId != null)
				return EmbeddingModel.GetDimention(this.EmbeddingModelId);
			return this._embeddingModelDimention;
		}
		set
		{
			if(value<=0)
				value = null;

			this.SetField(ref this._embeddingModelDimention, value, nameof(this.EmbeddingModelDimention));
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