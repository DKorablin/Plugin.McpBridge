using System.ComponentModel;
using System.Drawing.Design;
using System.Runtime.Serialization.Json;
using Plugin.McpBridge.Data;
using Plugin.McpBridge.Tests;
using Plugin.McpBridge.UI.PropertyGrid;

namespace Plugin.McpBridge
{
	/// <summary>Configuration settings for the MCP Bridge plugin.</summary>
	public abstract class SettingsBase : INotifyPropertyChanged
	{
		private static class Defaults
		{
			public const String AssistantSystemPrompt = @"You are a SAL automation assistant.
Use available MCP tools when useful.
Return clear user-facing responses, or a command payload only when automation is required.
Before using relative dates (today, yesterday, last hour), obtain the current system time from the SystemInformation tool.";
			public const String RagToolName = "SearchKnowledgeBase";
			public const String RagToolDescription = "Search the knowledge base for relevant information to assist in answering the user's question. Use this tool when the user is asking for specific information, or if you want to provide a more comprehensive answer. The input to this tool is a string representing the search query, and the output is a list of relevant search results.";
			public const String RagCitationsPrompt = "Always cite sources at the end of your response using the format: **Source:** [SourceName](SourceLink)";
			public const String McpServerUrl = "http://localhost:5050";
			public const String DevUIServerUrl = "http://localhost:5051";
			public const String AgUIServerUrl = "http://localhost:5052";
		}

		private static DataContractJsonSerializer Serializer = new DataContractJsonSerializer(typeof(AiProviderDto[]));

		private String? _aiProvidersJson = null;
		private BindingList<AiProviderDto>? _aiProviders = null;
		private Guid? _selectedProviderId;
		private String? _assistantSystemPrompt = Defaults.AssistantSystemPrompt;
		private String? _skillsDirectory = null;
		private String? _workflowsDirectory = null;

		private String? _ragToolName = null;
		private String? _ragToolDescription = null;
		private String? _ragDirectory = null;
		private String? _ragCitationsPrompt = null;

		private String[]? _toolsPermission = null;
		private String[]? _pluginsPermission = null;

		private Boolean _devUIEnabled = false;
		private String? _devUIServerUrl = null;
		private Boolean _agUIEnabled = false;
		private String? _agUIServerUrl = null;
		private String? _agUISessionStorageDirectory = null;
		private Boolean _mcpServerEnabled = false;
		private String? _mcpServerUrl = null;

		[Browsable(false)]
		public String? AiProvidersJson
		{
			get => this._aiProvidersJson;
			set
			{
				if(String.IsNullOrEmpty(value))
					value = null;
				this.SetField(ref this._aiProvidersJson, value, nameof(this.AiProvidersJson));
			}
		}

		[Category("AI Provider")]
		[Description("The list of AI providers available for selection. Managed through the AI Providers Manager UI.")]
		[DisplayName("AI Providers")]
		[TypeConverter(typeof(BindingListConverter<AiProviderDto>))]
		public BindingList<AiProviderDto> AiProviders
		{
			get
			{
				if(this._aiProviders == null)
				{
					AiProviderDto[]? arrProviders = null;
					if(this.AiProvidersJson != null)
						using(MemoryStream stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(this.AiProvidersJson)))
							arrProviders = (AiProviderDto[]?)Serializer.ReadObject(stream);

					List<AiProviderDto> aiProviders = new List<AiProviderDto>(arrProviders ?? Array.Empty<AiProviderDto>());

					this._aiProviders = new BindingList<AiProviderDto>(aiProviders);
					this._aiProviders.ListChanged += this.AiProviders_ListChanged;
				}
				return this._aiProviders;
			}
		}

		[Category("AI Provider")]
		[DisplayName("Selected Provider")]
		[Description("The active AI provider profile to use.")]
		[TypeConverter(typeof(AiProviderIdConverter))]
		[DefaultValue(null)]
		public Guid? SelectedProviderId
		{
			get => _selectedProviderId;
			set => this.SetField(ref this._selectedProviderId, value, nameof(this.SelectedProviderId));
		}

		/// <summary>The system prompt that defines the assistant's behavior and persona.</summary>
		[Category("Instruments")]
		[DefaultValue(Defaults.AssistantSystemPrompt)]
		[Description("The system prompt that defines the assistant's behavior and persona.")]
		public String? AssistantSystemPrompt
		{
			get => this._assistantSystemPrompt;
			set
			{
				if(String.IsNullOrWhiteSpace(value))
					value = Defaults.AssistantSystemPrompt;

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

		[Category("Instruments")]
		[Description("An optional directory path where workfows definitions are stored.")]
		public String? WorkflowsDirectory
		{
			get => this._workflowsDirectory;
			set
			{
				if(String.IsNullOrWhiteSpace(value) || !Directory.Exists(value))
					value = null;
				this.SetField(ref this._workflowsDirectory, value, nameof(this.WorkflowsDirectory));
			}
		}

		[Category("RAG")]
		[Description("The name of the RAG knowledge base search tool, which provides relevant information to assist in answering user questions. This should be a concise name that identifies the tool's purpose and can be used to invoke it in agent responses.")]
		[DefaultValue(Defaults.RagToolName)]
		public String? RagToolName
		{
			get => this._ragToolName ?? Defaults.RagToolName;
			set
			{
				if(String.IsNullOrWhiteSpace(value))
					value = null;

				this.SetField(ref this._ragToolName, value, nameof(this.RagToolName));
			}
		}

		[Category("RAG")]
		[DefaultValue(Defaults.RagToolDescription)]
		[Description("The description for the RAG knowledge base search tool, which provides relevant information to assist in answering user questions. This should explain when and how the tool should be used, as well as the expected input and output formats.")]
		public String? RagToolDescription
		{
			get => this._ragToolDescription ?? Defaults.RagToolDescription;
			set
			{
				if(String.IsNullOrWhiteSpace(value))
					value = null;
				this.SetField(ref this._ragToolDescription, value, nameof(this.RagToolDescription));
			}
		}

		[Category("RAG")]
		[DefaultValue(Defaults.RagCitationsPrompt)]
		[Description("A prompt to instruct the assistant to include citations when using RAG tools. This should explain the required format for citing sources, and can be used to ensure that the assistant provides proper attribution for information retrieved from the knowledge base.")]
		public String? RagCitationsPrompt
		{
			get => this._ragCitationsPrompt ?? Defaults.RagCitationsPrompt;
			set
			{
				if(String.IsNullOrWhiteSpace(value))
					value = null;
				this.SetField(ref this._ragCitationsPrompt, value, nameof(this.RagCitationsPrompt));
			}
		}

		[Category("RAG")]
		[Description("An optional directory path where RAG knowledge base files are stored. If not set, RAG tools will not be available.")]
		public String? RagDirectory
		{
			get => this._ragDirectory;
			set
			{
				if(String.IsNullOrWhiteSpace(value) || !Directory.Exists(value))
					value = null;
				this.SetField(ref this._ragDirectory, value, nameof(this.RagDirectory));
			}
		}

		[Category("Security")]
		[DefaultValue(null)]
		[Editor(typeof(ToolsPermissionEditor), typeof(UITypeEditor))]
		[Description("Controls which tools the assistant may use. Leave empty to allow all tools; otherwise only the listed method names are enabled.")]
		public String[]? ToolsPermission
		{
			get => this._toolsPermission;
			set
			{
				if(value?.Length == 0)
					value = null;

				this.SetField(ref this._toolsPermission, value, nameof(this.ToolsPermission));
			}
		}

		[Category("Security")]
		[DefaultValue(null)]
		[Editor(typeof(PluginsPermissionEditor), typeof(UITypeEditor))]
		[TypeConverter(typeof(PluginsPermissionConverter))]
		[Description("Controls which plugins the assistant may use. Leave empty to allow all plugins")]
		public String[]? PluginsPermission
		{
			get => this._pluginsPermission;
			set
			{
				if(value?.Length == 0)
					value = null;

				this.SetField(ref this._pluginsPermission, value, nameof(this.PluginsPermission));
			}
		}

		[Category("DevUI")]
		[DefaultValue(false)]
		[Description("When enabled, starts an embedded web server exposing the DevUI interface for local agent diagnostics.")]
		[DisplayName("DevUI Enabled")]
		public Boolean DevUIEnabled
		{
			get => this._devUIEnabled && ProcessHost.GetExePath(ProcessHost.ExeType.DevUI, false) != null;
			set
			{
				if(value)
					_ = ProcessHost.GetExePath(ProcessHost.ExeType.DevUI, true);

				this.SetField(ref this._devUIEnabled, value, nameof(this.DevUIEnabled));
			}
		}

		[Category("DevUI")]
		[DefaultValue(Defaults.DevUIServerUrl)]
		[Description("The URL of the embedded web server for the DevUI interface. Must be a valid absolute URL. If empty, defaults to " + Defaults.DevUIServerUrl)]
		[DisplayName("DevUI Server Url")]
		public String DevUIServerUrl
		{
			get
			{
				if(this._devUIServerUrl == null)
					this._devUIServerUrl = Defaults.DevUIServerUrl;
				return this._devUIServerUrl;
			}
			set
			{
				if(String.IsNullOrWhiteSpace(value) || !Uri.IsWellFormedUriString(value, UriKind.Absolute))
					value = null!;

				this.SetField(ref this._devUIServerUrl, value, nameof(this.DevUIServerUrl));
			}
		}

		[Category("AG-UI")]
		[DefaultValue(false)]
		[Description("When enabled, starts an embedded web server exposing the AG-UI interface for agent interaction and testing.")]
		[DisplayName("AG-UI Enabled")]
		public Boolean AgUIEnabled
		{
			get => this._agUIEnabled && ProcessHost.GetExePath(ProcessHost.ExeType.AgUI, false) != null;
			set
			{
				if(value)
					_ = ProcessHost.GetExePath(ProcessHost.ExeType.AgUI, true);

				this.SetField(ref this._agUIEnabled, value, nameof(this.AgUIEnabled));
			}
		}

		[Category("AG-UI")]
		[DefaultValue(Defaults.AgUIServerUrl)]
		[Description("The URL of the embedded web server for the AG-UI interface. Must be a valid absolute URL. If empty, defaults to " + Defaults.AgUIServerUrl)]
		[DisplayName("AG-UI Server Url")]
		public String AgUIServerUrl
		{
			get
			{
				if(this._agUIServerUrl == null)
					this._agUIServerUrl = Defaults.AgUIServerUrl;
				return this._agUIServerUrl;
			}
			set
			{
				if(String.IsNullOrWhiteSpace(value) || !Uri.IsWellFormedUriString(value, UriKind.Absolute))
					value = null!;

				this.SetField(ref this._agUIServerUrl, value, nameof(this.AgUIServerUrl));
			}
		}

		[Category("AG-UI")]
		[Description("An optional directory path where the AG-UI can read/write session data. If not set, AG-UI sessions will not be stored.")]
		public String? AgUISessionStorageDirectory
		{
			get => this._agUISessionStorageDirectory;
			set
			{
				if(String.IsNullOrWhiteSpace(value))
					value = null;
				this.SetField(ref this._agUISessionStorageDirectory, value, nameof(this.AgUISessionStorageDirectory));
			}
		}

		[Category("Network")]
		[Description("When enabled, starts an MCP server that tools can connect to for execution. Requires at least one tool with 'MCP Bridge' selected as its execution mode.")]
		public Boolean McpServerEnabled
		{
			get => this._mcpServerEnabled || this._devUIEnabled || this._agUIEnabled;
			set => this.SetField(ref this._mcpServerEnabled, value, nameof(this.McpServerEnabled));
		}

		[Category("Network")]
		[DefaultValue(Defaults.McpServerUrl)]
		[Description("The URL of the MCP server that tools will connect to for execution. Must be a valid absolute URL. If empty, defaults to " + Defaults.McpServerUrl)]
		public String McpServerUrl
		{
			get
			{
				if(this._mcpServerUrl == null)
					this._mcpServerUrl = Defaults.McpServerUrl;
				return this._mcpServerUrl;
			}
			set
			{
				if(String.IsNullOrWhiteSpace(value) || !Uri.IsWellFormedUriString(value, UriKind.Absolute))
					value = null!;

				this.SetField(ref this._mcpServerUrl, value, nameof(this.McpServerUrl));
			}
		}

		internal AiProviderDto? GetSelectedProvider()
			=> this.AiProviders.FirstOrDefault(x => x.Id == this.SelectedProviderId) ?? this.AiProviders.FirstOrDefault();

		public abstract String BuildSystemInstructions();

		private Boolean _listChangedPending = false;

		private void AiProviders_ListChanged(Object? sender, ListChangedEventArgs e)
		{
			if(this._listChangedPending)
				return;
			this._listChangedPending = true;

			SynchronizationContext.Current?.Post(_ =>
			{
				if(this._aiProviders == null || this._aiProviders.Count == 0)
					this.AiProvidersJson = null;
				else
				{
					Boolean morphed = false;
					for(Int32 i = 0; i < this._aiProviders.Count; i++)
					{
						AiProviderDto morphedItem = AiProviderDto.Morph(this._aiProviders[i]);
						if(!ReferenceEquals(morphedItem, this._aiProviders[i]))
						{
							this._aiProviders[i] = morphedItem;
							morphed = true;
						}
					}
					if(morphed)
						this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.AiProviders)));
					using(MemoryStream stream = new MemoryStream())
					{
						Serializer.WriteObject(stream, this._aiProviders.ToArray());
						stream.Seek(0, SeekOrigin.Begin);
						this.AiProvidersJson = System.Text.Encoding.UTF8.GetString(stream.ToArray());
					}
				}

				this._listChangedPending = false;

				if(this.SelectedProviderId != null && this._aiProviders?.Any(p => p.Id == this.SelectedProviderId) != true)
					this.SelectedProviderId = null;
			}, null);
		}

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
}