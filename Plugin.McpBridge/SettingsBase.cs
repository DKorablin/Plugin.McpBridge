using System.Collections;
using System.ComponentModel;
using System.Drawing.Design;
using System.Runtime.Serialization.Json;
using Plugin.McpBridge.Data;
using Plugin.McpBridge.Tests;
using Plugin.McpBridge.UI.PropertyGrid;

namespace Plugin.McpBridge
{
	/// <summary>Configuration settings for the MCP Bridge plugin.</summary>
	public class SettingsBase : INotifyPropertyChanged
	{
		private static class Defaults
		{
			public const String McpServerUrl = "http://localhost:5050";
			public const String DevUIServerUrl = "http://localhost:5051";
			public const String AgUIServerUrl = "http://localhost:5052";
		}

		private static DataContractJsonSerializer ProvidersSerializer = new DataContractJsonSerializer(typeof(AiProviderDto[]));
		private static DataContractJsonSerializer AgentSerializer = new DataContractJsonSerializer(typeof(AiAgentDto));

		private String? _aiProvidersJson = null;
		private BindingList<AiProviderDto>? _aiProviders = null;

		private String? _aiAgentJson = null;
		private AiAgentDto? _aiAgent = null;

		private String? _workflowsDirectory = null;

		private Boolean _devUIEnabled = false;
		private String? _devUIServerUrl = null;
		private Boolean _agUIEnabled = false;
		private String? _agUIServerUrl = null;
		private String? _agUISessionStorageDirectory = null;
		private Boolean _mcpServerEnabled = false;
		private String? _mcpServerUrl = null;

		private String[]? _toolsPermission = null;
		private String[]? _pluginsPermission = null;

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
		public String? AiAgentJson
		{
			get => this._aiAgentJson;
			set
			{
				if(String.IsNullOrWhiteSpace(value))
					value = null;
				this.SetField(ref this._aiAgentJson, value, nameof(this.AiAgentJson));
			}
		}

		[Category("Agent")]
		[DisplayName("AI Agent Configuration")]
		public AiAgentDto AiAgent
		{
			get
			{
				if(this._aiAgent == null)
				{
					if(this.AiAgentJson != null)
						using(MemoryStream stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(this.AiAgentJson)))
							this._aiAgent = (AiAgentDto?)AgentSerializer.ReadObject(stream) ?? new AiAgentDto();
					else
						this._aiAgent = new AiAgentDto() { AssistantSystemPrompt = AiAgentDto.Defaults.AssistantSystemPrompt, };

					this._aiAgent.PropertyChanged += (s, e) =>
					{
						using(MemoryStream stream = new MemoryStream())
						{
							AgentSerializer.WriteObject(stream, this._aiAgent);
							stream.Seek(0, SeekOrigin.Begin);
							this.AiAgentJson = System.Text.Encoding.UTF8.GetString(stream.ToArray());
						}
					};
				}
				return this._aiAgent;
			}
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
			get => this._devUIServerUrl ?? Defaults.DevUIServerUrl;
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
				return this._agUIServerUrl ?? Defaults.AgUIServerUrl;
			}
			set
			{
				if(String.IsNullOrWhiteSpace(value)
					|| !Uri.IsWellFormedUriString(value, UriKind.Absolute))
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
			get	=> this._mcpServerUrl ?? Defaults.McpServerUrl;
			set
			{
				if(String.IsNullOrWhiteSpace(value) || !Uri.IsWellFormedUriString(value, UriKind.Absolute))
					value = null!;

				this.SetField(ref this._mcpServerUrl, value, nameof(this.McpServerUrl));
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
						ProvidersSerializer.WriteObject(stream, this._aiProviders.ToArray());
						stream.Seek(0, SeekOrigin.Begin);
						this.AiProvidersJson = System.Text.Encoding.UTF8.GetString(stream.ToArray());
					}
				}

				this._listChangedPending = false;

				if(this.AiAgent.SelectedProviderId != null && this._aiProviders?.Any(p => p.Id == this.AiAgent.SelectedProviderId) != true)
					this.AiAgent.SelectedProviderId = null;
			}, null);
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
}