using System.ComponentModel;
using System.Drawing.Design;
using Plugin.McpBridge.UI.PropertyGrid;

namespace Plugin.McpBridge.Data;

[TypeConverter(typeof(ExpandableObjectConverter))]
public record AiAgentDto : INotifyPropertyChanged
{
	internal static class Defaults
	{
		public const String AssistantSystemPrompt = @"You are a SAL automation assistant.
Use available MCP tools when useful.
Return clear user-facing responses, or a command payload only when automation is required.
Before using relative dates (today, yesterday, last hour), obtain the current system time from the SystemInformation tool.";
	}

	private String[]? _toolsPermission = null;
	private String[]? _pluginsPermission = null;

	private Guid? _selectedProviderId;
	private Guid? _embeddingProviderId;

	private String? _assistantSystemPrompt = Defaults.AssistantSystemPrompt;

	private String? _skillsDirectory = null;

	private String? _ragToolName = null;
	private String? _ragToolDescription = null;
	private String? _ragDirectory = null;
	private String? _ragCitationsPrompt = null;

	/// <summary>Gets the unique identifier for this instance.</summary>
	[ReadOnly(true)]
	public Guid Id { get; init; } = Guid.NewGuid();

	[Category("Provider")]
	[DisplayName("Chat Provider")]
	[Description("The active AI provider profile to use for chat completions.")]
	[TypeConverter(typeof(AiProviderIdConverter))]
	[DefaultValue(null)]
	public Guid? SelectedProviderId
	{
		get => _selectedProviderId;
		set => this.SetField(ref this._selectedProviderId, value, nameof(this.SelectedProviderId));
	}

	[Category("Provider")]
	[DisplayName("Embedding Provider")]
	[Description("Optional AI provider profile used for embeddings and RAG indexing. If empty, the selected chat provider will also be used for embeddings.")]
	[TypeConverter(typeof(EmbeddingProviderIdConverter))]
	[DefaultValue(null)]
	public Guid? EmbeddingProviderId
	{
		get => this._embeddingProviderId;
		set => this.SetField(ref this._embeddingProviderId, value, nameof(this.EmbeddingProviderId));
	}

	/// <summary>The system prompt that defines the assistant's behavior and persona.</summary>
	[Category("Instruments")]
	[DefaultValue(Defaults.AssistantSystemPrompt)]
	[Description("The system prompt that defines the assistant's behavior and persona.")]
	[Editor(typeof(System.ComponentModel.Design.MultilineStringEditor), typeof(System.Drawing.Design.UITypeEditor))]
	public String? AssistantSystemPrompt
	{
		get => this._assistantSystemPrompt ?? Defaults.AssistantSystemPrompt;
		set
		{
			if(String.IsNullOrWhiteSpace(value))
				value = null;

			this.SetField(ref this._assistantSystemPrompt, value, nameof(this.AssistantSystemPrompt));
		}
	}

	[Category("Instruments")]
	[Description("An optional directory path where the assistant can read/write files when using skills. If not set, skills will not be available.")]
	[Editor(typeof(System.Windows.Forms.Design.FolderNameEditor), typeof(System.Drawing.Design.UITypeEditor))]
	public String? SkillsDirectory
	{
		get => this._skillsDirectory;
		set
		{
			if(String.IsNullOrWhiteSpace(value) || !Directory.Exists(value))
				value = null;
			this.SetField(ref this._skillsDirectory, value, nameof(this.SkillsDirectory));
		}
	}

	[Category("RAG")]
	[DisplayName("RAG Tool Name")]
	[Description("The name of the RAG knowledge base search tool, which provides relevant information to assist in answering user questions. This should be a concise name that identifies the tool's purpose and can be used to invoke it in agent responses.")]
	public String? RagToolName
	{
		get => this._ragToolName;
		set
		{
			if(String.IsNullOrWhiteSpace(value))
				value = null;

			this.SetField(ref this._ragToolName, value, nameof(this.RagToolName));
		}
	}

	[Category("RAG")]
	[DisplayName("RAG Tool Description")]
	[Description("The description for the RAG knowledge base search tool, which provides relevant information to assist in answering user questions. This should explain when and how the tool should be used, as well as the expected input and output formats.")]
	public String? RagToolDescription
	{
		get => this._ragToolDescription;
		set
		{
			if(String.IsNullOrWhiteSpace(value))
				value = null;

			this.SetField(ref this._ragToolDescription, value, nameof(this.RagToolDescription));
		}
	}

	[Category("RAG")]
	[DisplayName("RAG Citations Prompt")]
	[Description("A prompt to instruct the assistant to include citations when using RAG tools. This should explain the required format for citing sources, and can be used to ensure that the assistant provides proper attribution for information retrieved from the knowledge base.")]
	public String? RagCitationsPrompt
	{
		get => this._ragCitationsPrompt;
		set
		{
			if(String.IsNullOrWhiteSpace(value))
				value = null;
			this.SetField(ref this._ragCitationsPrompt, value, nameof(this.RagCitationsPrompt));
		}
	}

	[Category("RAG")]
	[DisplayName("RAG Knowledge Base Directory")]
	[Description("An optional directory path where RAG knowledge base files are stored (Supported extensions: *.txt, *.md).")]
	[Editor(typeof(System.Windows.Forms.Design.FolderNameEditor), typeof(System.Drawing.Design.UITypeEditor))]
	public String? RagDirectory
	{
		get => this._ragDirectory;
		set
		{
			if(String.IsNullOrWhiteSpace(value))
				value = null;

			this.SetField(ref this._ragDirectory, value, nameof(this.RagDirectory));
		}
	}

	[Category("Security")]
	[DefaultValue(null)]
	[Editor(typeof(ToolsPermissionEditor), typeof(UITypeEditor))]
	[TypeConverter(typeof(ToolsPermissionConverter))]
	[Description("Controls which tools the assistant may use.")]
	public String[]? ToolsPermission
	{
		get => this._toolsPermission;
		set => this.SetField(ref this._toolsPermission, value, nameof(this.ToolsPermission));
	}

	[Category("Security")]
	[DefaultValue(null)]
	[Editor(typeof(PluginsPermissionEditor), typeof(UITypeEditor))]
	[TypeConverter(typeof(PluginsPermissionConverter))]
	[Description("Controls which plugins the assistant may use.")]
	public String[]? PluginsPermission
	{
		get => this._pluginsPermission;
		set => this.SetField(ref this._pluginsPermission, value, nameof(this.PluginsPermission));
	}

	public AiProviderDto GetSelectedProvider(IEnumerable<AiProviderDto> providers)
	{
		if(providers == null)
			throw new InvalidOperationException("No AI providers available.");

		return GetProvider(providers, this.SelectedProviderId, true);
	}

	public AiProviderDto GetEmbeddingProvider(IEnumerable<AiProviderDto> providers)
	{
		if(providers == null)
			throw new InvalidOperationException("No AI providers available.");

		Guid? providerId = this.EmbeddingProviderId ?? this.SelectedProviderId;
		return GetProvider(providers, providerId, true);
	}

	private static AiProviderDto GetProvider(IEnumerable<AiProviderDto> providers, Guid? providerId, Boolean useFirstIfNull)
	{
		List<AiProviderDto> providerList = providers.ToList();
		if(providerList.Count == 0)
			throw new InvalidOperationException("No AI providers available.");

		if(providerId != null)
		{
			AiProviderDto? provider = providerList.FirstOrDefault(x => x.Id == providerId.Value);
			if(provider != null)
				return provider;
		}

		if(useFirstIfNull)
			return providerList[0];

		throw new InvalidOperationException("No AI providers available.");
	}

	#region INotifyPropertyChanged
	public event PropertyChangedEventHandler? PropertyChanged;
	private Boolean SetField<T>(ref T field, T value, String propertyName)
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