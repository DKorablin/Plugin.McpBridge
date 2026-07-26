using System.ComponentModel;
using System.Runtime.Serialization;
using Plugin.McpBridge.UI.PropertyGrid.Converters;

namespace Plugin.McpBridge.Data;

[DataContract]
[TypeConverter(typeof(ExpandableObjectConverter))]
public record NetworkProviderDto : AiProviderDto
{
	private Boolean _evaluationCacheEnabled = false;
	private String? _evaluationCacheDirectory = null;

	[Category("Diagnostics")]
	[DisplayName("Evaluation Cache Enabled")]
	[Description("When enabled, wraps the chat client with persistent disk caching (Microsoft.Extensions.AI.Evaluation.Reporting) so LLM responses survive app restarts for reproducible debugging.")]
	[DefaultValue(false)]
	[DataMember]
	public Boolean EvaluationCacheEnabled
	{
		get => this._evaluationCacheEnabled;
		set => this.SetField(ref this._evaluationCacheEnabled, value, nameof(this.EvaluationCacheEnabled));
	}

	[Category("Diagnostics")]
	[DisplayName("Evaluation Cache Directory")]
	[Description("Optional directory for persistent evaluation cache data. Defaults to %LOCALAPPDATA%\\Plugin.McpBridge\\.EvaluationCache\\{AgentRole} when empty.")]
	[TypeConverter(typeof(EvaluationCacheDirConverter))]
	[DefaultValue(null)]
	[DataMember]
	public String? EvaluationCacheDirectory
	{
		get => this._evaluationCacheDirectory;
		set
		{
			if(String.IsNullOrWhiteSpace(value))
				value = null;
			else if(value.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
				throw new ArgumentException("Evaluation cache directory path contains invalid characters.", nameof(value));

			this.SetField(ref this._evaluationCacheDirectory, value, nameof(this.EvaluationCacheDirectory));
		}
	}
}