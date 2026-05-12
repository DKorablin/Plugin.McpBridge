using System.ComponentModel;
using System.Drawing.Design;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Json;
using System.Security.Policy;
using Google.Protobuf.WellKnownTypes;
using Plugin.McpBridge.Data;
using Plugin.McpBridge.UI.PropertyGrid;
using SAL.Flatbed;

namespace Plugin.McpBridge
{
	public enum AiProviderType
	{
		OpenAI,
		AzureOpenAI,
		OpenAICompatible,
		LocalOpenAICompatible,
		QwenCompatible,
		GrokCompatible,
		GeminiCompatible,
		CustomCompatible,
#if DEBUG
		/// <summary>Returns scripted responses locally. No credentials or network required. Intended for UI testing.</summary>
		Stub,
#endif
	}

	/// <summary>Configuration settings for the MCP Bridge plugin.</summary>
	public class Settings : INotifyPropertyChanged
	{
		private static class Defaults
		{
			public const String AssistantSystemPrompt = @"You are a SAL automation assistant.
Use available MCP tools when useful.
Return clear user-facing responses, or a command payload only when automation is required.
Before using relative dates (today, yesterday, last hour), obtain the current system time from the SystemInformation tool.";
			public static readonly TimeSpan ConnectionTimeout = TimeSpan.FromSeconds(100);
			public const String McpServerUrl = "http://localhost:5051";
			public const String DevUiServerUrl = "http://localhost:5050";
		}

		private static DataContractJsonSerializer Serializer = new DataContractJsonSerializer(typeof(AiProviderDto[]));

		private String? _aiProvidersJson = null;
		private BindingList<AiProviderDto>? _aiProviders = null;
		private Guid? _selectedProviderId;
		private String? _assistantSystemPrompt = Defaults.AssistantSystemPrompt;
		private Int32? _maxTokens;
		private TimeSpan _connectionTimeout = Defaults.ConnectionTimeout;
		private String[]? _toolsPermission = null;
		private String[]? _pluginsPermission = null;
		private Boolean _devUIEnabled = false;
		private String? _devUiServerUrl = null;
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
		[Editor(typeof(WithDescriptionCollectionEditor), typeof(UITypeEditor))]
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
		[Category("Prompt Settings")]
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

		/// <summary>The maximum number of tokens to generate in a single response.</summary>
		[Category("Prompt Settings")]
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
		[Description("When enabled, starts an embedded web server exposing the DevUI interface for local agent diagnostics. Navigate to http://localhost:<DevUIPort>/devui.")]
		[DisplayName("DevUI Enabled")]
		public Boolean DevUIEnabled
		{
			get => this._devUIEnabled;
			set => this.SetField(ref this._devUIEnabled, value, nameof(this.DevUIEnabled));
		}

		[Category("DevUI")]
		[DefaultValue(Defaults.DevUiServerUrl)]
		[Description("The local port used by the embedded DevUI web server.")]
		[DisplayName("DevUI Port")]
		public String DevUIServerUrl
		{
			get
			{
				if(this._devUiServerUrl == null)
					this._devUiServerUrl = Defaults.DevUiServerUrl;
				return this._devUiServerUrl;
			}
			set
			{
				if(String.IsNullOrWhiteSpace(value) || !Uri.IsWellFormedUriString(value, UriKind.Absolute))
					value = null!;

				this.SetField(ref this._devUiServerUrl, value, nameof(this.DevUIServerUrl));
			}
		}

		[Category("Network")]
		[Description("When enabled, starts an MCP server that tools can connect to for execution. Requires at least one tool with 'MCP Bridge' selected as its execution mode.")]
		public Boolean McpServerEnabled
		{
			get => this._mcpServerEnabled;
			set => this.SetField(ref this._mcpServerEnabled, value, nameof(this.McpServerEnabled));
		}

		[Category("Network")]
		[DefaultValue(Defaults.McpServerUrl)]
		[Description("The URL of the MCP server to connect to for tool calls. If empty, the MCP server will not be used and tools will not be available.")]
		public String McpServerUrl
		{
			get
			{
				if(this._mcpServerUrl == null)
					this._mcpServerUrl = Defaults.McpServerUrl;
				return this._mcpServerUrl;

				Int32 FindFreePort()
				{
					using TcpListener probe = new TcpListener(IPAddress.Loopback, 0);
					probe.Start();
					Int32 port = ((IPEndPoint)probe.LocalEndpoint).Port;
					probe.Stop();
					return port;
				}
			}
			set
			{
				if(String.IsNullOrWhiteSpace(value) || !Uri.IsWellFormedUriString(value, UriKind.Absolute))
					value = null!;

				this.SetField(ref this._mcpServerUrl, value, nameof(this.McpServerUrl));
			}
		}

		[Category("Network")]
		[DefaultValue(typeof(TimeSpan), "00:01:40")]
		[Description("The timeout duration for network connections to the AI provider.")]
		public TimeSpan ConnectionTimeout
		{
			get => this._connectionTimeout;
			set
			{
				if(value <= TimeSpan.Zero)
					value = Defaults.ConnectionTimeout;
				this.SetField(ref this._connectionTimeout, value, nameof(this.ConnectionTimeout));
			}
		}

		internal IHost Host { get; }

		public Settings() : this(null!) { }

		internal Settings(IHost host)
			=> this.Host = host ?? throw new ArgumentNullException(nameof(host));

		internal AiProviderDto? GetSelectedProvider()
			=> this.AiProviders.FirstOrDefault(x => x.Id == this.SelectedProviderId) ?? this.AiProviders.FirstOrDefault();

		private Boolean _listChangedPending = false;

		private void AiProviders_ListChanged(Object? sender, ListChangedEventArgs e)
		{
			if(this._listChangedPending)
				return;
			this._listChangedPending = true;

			SynchronizationContext.Current?.Post(_ =>
			{
				this._listChangedPending = false;

				if(this._aiProviders == null || this._aiProviders.Count == 0)
					this.AiProvidersJson = null;
				else
				{
					using(MemoryStream stream = new MemoryStream())
					{
						Serializer.WriteObject(stream, this._aiProviders.ToArray());
						stream.Seek(0, SeekOrigin.Begin);
						this.AiProvidersJson = System.Text.Encoding.UTF8.GetString(stream.ToArray());
					}
				}

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