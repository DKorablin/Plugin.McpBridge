using System.ComponentModel;
using Plugin.McpBridge.UI.PropertyGrid;

namespace Plugin.McpBridge.Data;

[TypeConverter(typeof(ExpandableObjectConverter))]
public record AiAgentDto : INotifyPropertyChanged
{
	public static class Defaults
	{
		// TODO: Find the better way for default system prompt. This is a temporary solution until we have a better way to manage system prompts.
		public static String AssistantSystemPrompt;
		public static readonly String[] RagSupportedExtensions = new String[] { ".txt", ".md" };
	}

	private String? _description = null;

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
	private String[] _ragSupportedExtensions = (String[])Defaults.RagSupportedExtensions.Clone();

	/// <summary>Gets the unique identifier for this instance.</summary>
	[ReadOnly(true)]
	public Guid Id { get; init; } = Guid.NewGuid();

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

	[Category("Provider")]
	[DisplayName("Chat Provider")]
	[Description("The active AI provider profile to use for chat completions.")]
	[DefaultValue(null)]
	public Guid? SelectedProviderId
	{
		get => _selectedProviderId;
		set => this.SetField(ref this._selectedProviderId, value, nameof(this.SelectedProviderId));
	}

	[Category("Provider")]
	[DisplayName("Embedding Provider")]
	[Description("Optional AI provider profile used for embeddings and RAG indexing. If empty, the selected chat provider will also be used for embeddings.")]
	[DefaultValue(null)]
	public Guid? EmbeddingProviderId
	{
		get => this._embeddingProviderId;
		set => this.SetField(ref this._embeddingProviderId, value, nameof(this.EmbeddingProviderId));
	}

	/// <summary>The system prompt that defines the assistant's behavior and persona.</summary>
	[Category("Instruments")]
	[Description("The system prompt that defines the assistant's behavior and persona.")]
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
	[DisplayName("RAG Supported Extensions")]
	[Description("Optional list of supported RAG file extensions. When empty, defaults to .txt and .md.")]
	[DefaultValue(null)]
	public String[] RagSupportedExtensions
	{
		get => this._ragSupportedExtensions;
		set
		{
			String[] normalized = NormalizeRagSupportedExtensions(value);
			this.SetField(ref this._ragSupportedExtensions, normalized, nameof(this.RagSupportedExtensions));
		}
	}

	[Category("RAG")]
	[DisplayName("RAG Knowledge Base Directory")]
	[Description("An optional directory path where RAG knowledge base files are stored. Supported extensions are configured by RAG Supported Extensions.")]
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
	[TypeConverter(typeof(ToolsPermissionConverter))]
	[Description("Controls which tools the assistant may use.")]
	public String[]? ToolsPermission
	{
		get => this._toolsPermission;
		set => this.SetField(ref this._toolsPermission, value, nameof(this.ToolsPermission));
	}

	[Category("Security")]
	[DefaultValue(null)]
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
		if(providerId != null)
		{
			var provider = providers.FirstOrDefault(x => x.Id == providerId.Value);
			if(provider != null)
				return provider;
		}

		if(useFirstIfNull)
		{
			var provider = providers.FirstOrDefault();
			if(provider != null)
				return provider;
		}

		throw new InvalidOperationException("No AI providers available.");
	}

	internal static String[] NormalizeRagSupportedExtensions(IEnumerable<String>? extensions)
	{
		if(extensions == null)
			return (String[])Defaults.RagSupportedExtensions.Clone();

		HashSet<String> unique = new HashSet<String>(StringComparer.OrdinalIgnoreCase);
		List<String> normalized = new List<String>();
		foreach(String extension in extensions)
		{
			if(String.IsNullOrWhiteSpace(extension))
				throw new ArgumentException("RAG supported extensions cannot contain empty values.", nameof(extensions));

			String normalizedExtension = extension.Trim().ToLowerInvariant();
			ValidateRagSupportedExtension(normalizedExtension);
			if(unique.Add(normalizedExtension))
				normalized.Add(normalizedExtension);
		}

		return normalized.Count == 0
			? (String[])Defaults.RagSupportedExtensions.Clone()
			: normalized.ToArray();
	}

	private static void ValidateRagSupportedExtension(String extension)
	{
		if(extension.Length < 2 || extension[0] != '.')
			throw new ArgumentException($"Invalid RAG supported extension '{extension}'. Extensions must start with '.'.", nameof(extension));
		if(extension[^1] == '.')
			throw new ArgumentException($"Invalid RAG supported extension '{extension}'. Extensions must end with an alphanumeric suffix.", nameof(extension));
		if(extension.Any(Char.IsWhiteSpace))
			throw new ArgumentException($"Invalid RAG supported extension '{extension}'. Extensions cannot contain whitespace.", nameof(extension));
		if(!extension.Skip(1).All(c => Char.IsLetterOrDigit(c) || c == '.' || c == '_' || c == '-'))
			throw new ArgumentException($"Invalid RAG supported extension '{extension}'. Allowed characters are letters, digits, '.', '_' and '-'.", nameof(extension));
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