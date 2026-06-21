using System.ComponentModel;
using System.Runtime.Serialization.Json;
using Plugin.McpBridge.Data;
using Plugin.McpBridge.Tests;
using Plugin.McpBridge.UI.PropertyGrid;

namespace Plugin.McpBridge
{
	/// <summary>Configuration settings for the MCP Bridge plugin.</summary>
	public partial class Settings : INotifyPropertyChanged
	{
		private static class Defaults
		{
			public const String McpServerUrl = "http://localhost:5050";
			public const String DevUIServerUrl = "http://localhost:5051";
			public const String AgUIServerUrl = "http://localhost:5052";
			public const String DtsEmulatorEndpoint = "http://localhost:4001";
			public const String DtsEmulatorDashboard = "http://localhost:4002";
		}

		private static DataContractJsonSerializer ProvidersSerializer = new DataContractJsonSerializer(typeof(AiProviderDto[]));
		private static DataContractJsonSerializer AgentSerializer = new DataContractJsonSerializer(typeof(AiAgentDto[]));

		private String? _aiProvidersJson = null;
		private BindingList<AiProviderDto>? _aiProviders = null;

		private String? _aiAgentsJson = null;
		private BindingList<AiAgentDto>? _aiAgents = null;

		private Guid? _selectedAgentId;
		private String? _lastConversationId;

		private String? _workflowsDirectory = null;

		private Boolean _devUIEnabled = false;
		private String? _devUIServerUrl = null;
		private Boolean _agUIEnabled = false;
		private String? _agUIServerUrl = null;
		private String? _sessionStorageDirectory = null;
		private Boolean _ragProcessEnabled = false;
		private Boolean _mcpServerEnabled = false;
		private String? _mcpServerUrl = null;
		private Boolean _dtsEmulatorProcessEnabled = false;
		private String? _dtsEmulatorEndpoint = null;
		private String? _dtsEmulatorDashboard = null;

		[Browsable(false)]
		public String? AiProvidersJson
		{
			get => this._aiProvidersJson;
			set
			{
				if(String.IsNullOrWhiteSpace(value))
					value = null;
				this.SetField(ref this._aiProvidersJson, value, nameof(this.AiProvidersJson));
			}
		}

		[Category("Agent")]
		[Description("The list of AI providers available for selection. Managed through the AI Providers Manager UI.")]
		[DisplayName("AI Providers Configuration")]
		[TypeConverter(typeof(BindingListConverter<AiProviderDto>))]
		[Editor(typeof(CollectionWithDescriptionEditor), typeof(System.Drawing.Design.UITypeEditor))]
		public BindingList<AiProviderDto> AiProviders
		{
			get
			{
				if(this._aiProviders == null)
				{
					AiProviderDto[]? arrProviders = null;
					if(this.AiProvidersJson != null)
						using(MemoryStream stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(this.AiProvidersJson)))
							arrProviders = (AiProviderDto[]?)ProvidersSerializer.ReadObject(stream);

					List<AiProviderDto> aiProviders = new List<AiProviderDto>(arrProviders ?? Array.Empty<AiProviderDto>());

					this._aiProviders = new BindingList<AiProviderDto>(aiProviders);
					this._aiProviders.ListChanged += this.AiProviders_ListChanged;
				}
				return this._aiProviders;
			}
		}

		[Browsable(false)]
		public String? AiAgentsJson
		{
			get => this._aiAgentsJson;
			set
			{
				if(String.IsNullOrWhiteSpace(value))
					value = null;
				this.SetField(ref this._aiAgentsJson, value, nameof(this.AiAgentsJson));
			}
		}

		[Category("Agent")]
		[DisplayName("AI Agents Configuration")]
		[TypeConverter(typeof(BindingListConverter<AiAgentDto>))]
		[Editor(typeof(CollectionWithDescriptionEditor), typeof(System.Drawing.Design.UITypeEditor))]
		public BindingList<AiAgentDto> AiAgents
		{
			get
			{
				if(this._aiAgents == null)
				{
					AiAgentDto[]? arrAgents = null;
					if(this.AiAgentsJson != null)
						using(MemoryStream stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(this.AiAgentsJson)))
							arrAgents = (AiAgentDto[]?)AgentSerializer.ReadObject(stream);

					List<AiAgentDto> aiAgents = arrAgents?.Length > 0
						? new List<AiAgentDto>(arrAgents)
						: new List<AiAgentDto> { new AiAgentDto() { AssistantSystemPrompt = AiAgentDto.Defaults.AssistantSystemPrompt, }, };

					this._aiAgents = new BindingList<AiAgentDto>(aiAgents);
					this._aiAgents.ListChanged += this.AiAgents_ListChanged;
				}
				return this._aiAgents;
			}
		}

		[Category("Agent")]
		[DisplayName("Selected Agent")]
		[TypeConverter(typeof(AiAgentIdConverter))]
		[DefaultValue(null)]
		public Guid? SelectedAgentId
		{
			get => _selectedAgentId;
			set => this.SetField(ref this._selectedAgentId, value, nameof(this.SelectedAgentId));
		}

		[Browsable(false)]
		public AiAgentDto SelectedAgent
			=> this._selectedAgentId.HasValue
				? this.AiAgents.FirstOrDefault(a => a.Id == this._selectedAgentId) ?? this.AiAgents[0]
				: this.AiAgents[0];

		[Browsable(false)]
		public String? LastConversationId
		{
			get => this._lastConversationId ?? (this._lastConversationId = Guid.NewGuid().ToString());
			set => this.SetField(ref this._lastConversationId, value, nameof(this.LastConversationId));
		}

		[Category("Agent")]
		[Description("An optional directory path where workfows definitions are stored.")]
		[Editor(typeof(System.Windows.Forms.Design.FolderNameEditor), typeof(System.Drawing.Design.UITypeEditor))]
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

		[Category("Agent")]
		[DisplayName("Session Storage Directory")]
		[Description("An optional directory path where the agents can read/write session data. If not set, sessions will not be stored.")]
		[Editor(typeof(System.Windows.Forms.Design.FolderNameEditor), typeof(System.Drawing.Design.UITypeEditor))]
		public String? SessionStorageDirectory
		{
			get => this._sessionStorageDirectory;
			set
			{
				if(String.IsNullOrWhiteSpace(value))
					value = null;

				this.SetField(ref this._sessionStorageDirectory, value, nameof(this.SessionStorageDirectory));
			}
		}

		[Category("Network")]
		[DisplayName("DevUI Enabled")]
		[Description("When enabled, starts an embedded web server exposing the DevUI interface for local agent diagnostics.")]
		[DefaultValue(false)]
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

		[Category("Network")]
		[DisplayName("DevUI Server Url")]
		[Description("The URL of the embedded web server for the DevUI interface. Must be a valid absolute URL. If empty, defaults to " + Defaults.DevUIServerUrl)]
		[DefaultValue(Defaults.DevUIServerUrl)]
		public String DevUIServerUrl
		{
			get => this._devUIServerUrl ?? Defaults.DevUIServerUrl;
			set
			{
				if(String.IsNullOrWhiteSpace(value) || !Uri.IsWellFormedUriString(value, UriKind.Absolute))
					value = null!;

				this.SetField(ref this._devUIServerUrl, value, nameof(this.DevUIServerUrl));
			}
		}

		[Category("Network")]
		[DisplayName("AG-UI Enabled")]
		[Description("When enabled, starts an embedded web server exposing the AG-UI interface for agent interaction and testing.")]
		[DefaultValue(false)]
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

		[Category("Network")]
		[DisplayName("AG-UI Server Url")]
		[Description("The URL of the embedded web server for the AG-UI interface. Must be a valid absolute URL. If empty, defaults to " + Defaults.AgUIServerUrl)]
		[DefaultValue(Defaults.AgUIServerUrl)]
		public String AgUIServerUrl
		{
			get	=> this._agUIServerUrl ?? Defaults.AgUIServerUrl;
			set
			{
				if(String.IsNullOrWhiteSpace(value)
					|| !Uri.IsWellFormedUriString(value, UriKind.Absolute))
					value = null!;

				this.SetField(ref this._agUIServerUrl, value, nameof(this.AgUIServerUrl));
			}
		}

		[Category("RAG")]
		[DisplayName("RAG Sidecar Enabled")]
		[Description("When enabled, starts the Plugin.McpBridge.RAG process to keep SQLite vector indexes synchronized with configured RAG directories.")]
		[DefaultValue(false)]
		public Boolean RagProcessEnabled
		{
			get => this._ragProcessEnabled && ProcessHost.GetExePath(ProcessHost.ExeType.RAG, false) != null;
			set
			{
				if(value)
					_ = ProcessHost.GetExePath(ProcessHost.ExeType.RAG, true);

				this.SetField(ref this._ragProcessEnabled, value, nameof(this.RagProcessEnabled));
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
			get	=> this._mcpServerUrl ?? Defaults.McpServerUrl;
			set
			{
				if(String.IsNullOrWhiteSpace(value) || !Uri.IsWellFormedUriString(value, UriKind.Absolute))
					value = null!;

				this.SetField(ref this._mcpServerUrl, value, nameof(this.McpServerUrl));
			}
		}

		[Category("DTS")]
		[DisplayName("DTS Emulator Enabled")]
		[Description("When enabled, starts the Plugin.McpBridge.DTS process to provide durable task scheduling support via Docker container.")]
		[DefaultValue(false)]
		public Boolean DtsEmulatorProcessEnabled
		{
			get => this._dtsEmulatorProcessEnabled && ProcessHost.GetExePath(ProcessHost.ExeType.DTS, false) != null;
			set
			{
				if(value)
					_ = ProcessHost.GetExePath(ProcessHost.ExeType.DTS, true);

				this.SetField(ref this._dtsEmulatorProcessEnabled, value, nameof(this.DtsEmulatorProcessEnabled));
			}
		}

		[Category("DTS")]
		[DisplayName("DTS Emulator Endpoint")]
		[Description("The endpoint URL of the DTS Emulator used for durable task scheduling. Must be a valid absolute URL. If empty, defaults to " + Defaults.DtsEmulatorEndpoint)]
		[DefaultValue(Defaults.DtsEmulatorEndpoint)]
		public String DtsEmulatorEndpoint
		{
			get => this._dtsEmulatorEndpoint ?? Defaults.DtsEmulatorEndpoint;
			set
			{
				if(String.IsNullOrWhiteSpace(value) || !Uri.IsWellFormedUriString(value, UriKind.Absolute))
					value = null!;

				this.SetField(ref this._dtsEmulatorEndpoint, value, nameof(this.DtsEmulatorEndpoint));
			}
		}

		[Category("DTS")]
		[DisplayName("DTS Emulator Dashboard")]
		[Description("The optional URL to the DTS Emulator dashboard UI for monitoring and diagnostics. If empty, defaults to " + Defaults.DtsEmulatorDashboard)]
		[DefaultValue(Defaults.DtsEmulatorDashboard)]
		public String DtsEmulatorDashboard
		{
			get => this._dtsEmulatorDashboard ?? Defaults.DtsEmulatorDashboard;
			set
			{
				if(String.IsNullOrWhiteSpace(value) || !Uri.IsWellFormedUriString(value, UriKind.Absolute))
					value = null!;

				this.SetField(ref this._dtsEmulatorDashboard, value, nameof(this.DtsEmulatorDashboard));
			}
		}

		internal IEnumerable<ProcessHost.ExeType> EnabledProcesses
		{
			get
			{
				if(this.DevUIEnabled)
					yield return ProcessHost.ExeType.DevUI;
				if(this.AgUIEnabled)
					yield return ProcessHost.ExeType.AgUI;
				if(this.RagProcessEnabled)
					yield return ProcessHost.ExeType.RAG;
				if(this.DtsEmulatorProcessEnabled)
					yield return ProcessHost.ExeType.DTS;
			}
		}
	}
}