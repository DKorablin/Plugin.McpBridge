using System.ComponentModel;
using System.Runtime.Serialization.Json;
using McpBridge.Core.Remoting;
using McpBridge.Core.Data;
using McpBridge.Core.UI.PropertyGrid.Converters;

namespace McpBridge.Core
{
	/// <summary>Configuration settings for the MCP Bridge plugin.</summary>
	public partial class Settings : INotifyPropertyChanged
	{
		private static class Defaults
		{
			public const String McpServerUrl = "http://localhost:5050";
			public const String DevUIServerUrl = "http://localhost:5051";
			public const String AgUIServerUrl = "http://localhost:5052";
			public const Int32 DtsEmulatorPort = 8080;
			public const Int32 DtsEmulatorDashboardPort = 8082;
		}

		private static DataContractJsonSerializer ProvidersSerializer = new DataContractJsonSerializer(typeof(AiProviderDto[]));
		private static DataContractJsonSerializer AgentSerializer = new DataContractJsonSerializer(typeof(AiAgentDto[]));

		// Legacy namespace migration for deserialization of old settings files. This is a temporary measure.
		private static String MigrateLegacyNamespace(String json)
			=> json.Replace(":#Plugin.McpBridge.", ":#McpBridge.Core.", StringComparison.Ordinal);

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
		private Boolean _enableSessionStorage = false;
		private String? _sessionStorageDirectory = null;
		private Boolean _ragProcessEnabled = false;
		private Boolean _mcpServerEnabled = false;
		private String? _mcpServerUrl = null;
		private Boolean _dtsEmulatorProcessEnabled = false;
		private Int32? _dtsEmulatorPort = null;
		private Int32? _dtsEmulatorDashboardPort = null;

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
		public BindingList<AiProviderDto> AiProviders
		{
			get
			{
				if(this._aiProviders == null)
				{
					AiProviderDto[]? arrProviders = null;
					if(this.AiProvidersJson != null)
						using(MemoryStream stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(MigrateLegacyNamespace(this.AiProvidersJson))))
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
		public BindingList<AiAgentDto> AiAgents
		{
			get
			{
				if(this._aiAgents == null)
				{
					AiAgentDto[]? arrAgents = null;
					if(this.AiAgentsJson != null)
						using(MemoryStream stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(MigrateLegacyNamespace(this.AiAgentsJson))))
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

		[Category("Session Storage")]
		[DisplayName("Enable Session Storage")]
		[Description("When enabled, agents can read/write session data to the specified directory.")]
		[DefaultValue(false)]
		public Boolean EnableSessionStorage
		{
			get => this._enableSessionStorage;
			set => this.SetField(ref this._enableSessionStorage, value, nameof(this.EnableSessionStorage));
		}

		[Category("Session Storage")]
		[DisplayName("Session Storage Directory")]
		[Description("An optional directory path where the agents can read/write session data. Defaults to %LOCALAPPDATA%\\Plugin.McpBridge\\.SessionStore\\{AgentRole} when empty.")]
		[TypeConverter(typeof(SessionStorageDirConverter))]
		[DefaultValue(null)]
		public String? SessionStorageDirectory
		{
			get => this._sessionStorageDirectory;
			set
			{
				if(String.IsNullOrWhiteSpace(value))
					value = null;
				else if(value.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
					throw new ArgumentException("Storage directory path contains invalid characters.", nameof(value));

				this.SetField(ref this._sessionStorageDirectory, value, nameof(this.SessionStorageDirectory));
			}
		}

		[Category("Network")]
		[DisplayName("DevUI Enabled")]
		[Description("When enabled, starts an embedded web server exposing the DevUI interface for local agent diagnostics.")]
		[DefaultValue(false)]
		public Boolean DevUIEnabled
		{
			get => this._devUIEnabled && ProcessHost.GetProcessPath(ProcessType.DevUI, false) != null;
			set
			{
				if(value)
					_ = ProcessHost.GetProcessPath(ProcessType.DevUI, true);

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
			get => this._agUIEnabled && ProcessHost.GetProcessPath(ProcessType.AgUI, false) != null;
			set
			{
				if(value)
					_ = ProcessHost.GetProcessPath(ProcessType.AgUI, true);

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
			get => this._ragProcessEnabled && ProcessHost.GetProcessPath(ProcessType.RAG, false) != null;
			set
			{
				if(value)
					_ = ProcessHost.GetProcessPath(ProcessType.RAG, true);

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
			get => this._dtsEmulatorProcessEnabled && ProcessHost.GetProcessPath(ProcessType.DTS, false) != null;
			set
			{
				if(value)
					_ = ProcessHost.GetProcessPath(ProcessType.DTS, true);

				this.SetField(ref this._dtsEmulatorProcessEnabled, value, nameof(this.DtsEmulatorProcessEnabled));
			}
		}

		[Category("DTS")]
		[DisplayName("DTS Emulator gRPC Port")]
		[Description("Port used for DTS emulator gRPC endpoint. If invalid, defaults to " + nameof(Defaults.DtsEmulatorPort))]
		[DefaultValue(Defaults.DtsEmulatorPort)]
		public Int32 DtsEmulatorPort
		{
			get => this._dtsEmulatorPort ?? Defaults.DtsEmulatorPort;
			set
			{
				if(value < 1 || value > 65535)
					value = Defaults.DtsEmulatorPort;

				this.SetField(ref this._dtsEmulatorPort, value, nameof(this.DtsEmulatorPort));
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.DtsEmulatorEndpoint)));
			}
		}

		[Category("DTS")]
		[DisplayName("DTS Emulator Dashboard Port")]
		[Description("Port used for DTS emulator dashboard endpoint. If invalid, defaults to " + nameof(Defaults.DtsEmulatorDashboardPort))]
		[DefaultValue(Defaults.DtsEmulatorDashboardPort)]
		public Int32 DtsEmulatorDashboardPort
		{
			get => this._dtsEmulatorDashboardPort ?? Defaults.DtsEmulatorDashboardPort;
			set
			{
				if(value < 1 || value > 65535)
					value = Defaults.DtsEmulatorDashboardPort;

				this.SetField(ref this._dtsEmulatorDashboardPort, value, nameof(this.DtsEmulatorDashboardPort));
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.DtsEmulatorDashboard)));
			}
		}

		[Browsable(false)]
		[Category("DTS")]
		[DisplayName("DTS Emulator Endpoint")]
		[Description("Derived gRPC endpoint URL for DTS Emulator.")]
		public String DtsEmulatorEndpoint
		{
			get => $"http://localhost:{this.DtsEmulatorPort}";
			set => _ = value;
		}

		[Browsable(false)]
		[Category("DTS")]
		[DisplayName("DTS Emulator Dashboard")]
		[Description("Derived dashboard URL for DTS Emulator.")]
		public String DtsEmulatorDashboard
		{
			get => $"http://localhost:{this.DtsEmulatorDashboardPort}";
			set => _ = value;
		}

		internal IEnumerable<ProcessType> EnabledProcesses
		{
			get
			{
				if(this.DevUIEnabled)
					yield return ProcessType.DevUI;
				if(this.AgUIEnabled)
					yield return ProcessType.AgUI;
				if(this.RagProcessEnabled)
					yield return ProcessType.RAG;
				if(this.DtsEmulatorProcessEnabled)
					yield return ProcessType.DTS;
			}
		}

		/// <summary>Disables a process type due to failure.</summary>
		internal void DisableProcess(ProcessType exeType)
		{
			switch(exeType)
			{
			case ProcessType.DevUI:
				this.DevUIEnabled = false;
				break;
			case ProcessType.AgUI:
				this.AgUIEnabled = false;
				break;
			case ProcessType.RAG:
				this.RagProcessEnabled = false;
				break;
			case ProcessType.DTS:
				this.DtsEmulatorProcessEnabled = false;
				break;
			default:
				throw new InvalidOperationException($"Unknown process type: {exeType}");
			}
		}

		internal String? GetSessionStorageDirectory()
		{
			if(!this.EnableSessionStorage)
				return null;

			return this._sessionStorageDirectory
				?? Utils.GetAgentStorageDirectory(Utils.SpecialDirectory.SessionStore);
		}
	}
}