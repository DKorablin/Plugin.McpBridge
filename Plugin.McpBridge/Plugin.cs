using System.ComponentModel;
using System.Diagnostics;
using Plugin.McpBridge.Events;
using Plugin.McpBridge.Tools;
using SAL.Flatbed;
using SAL.Windows;

namespace Plugin.McpBridge
{
	public class Plugin : IPlugin, IPluginSettings<Settings>
	{
		private Settings? _settings;
		private TraceSource? _trace;

		private AssistantAgent? _agent;

		private IMenuItem? _menuChat;

		internal TraceSource Trace => this._trace ?? (this._trace = Plugin.CreateTraceSource<Plugin>());

		Object IPluginSettings.Settings => this.Settings;

		public Settings Settings
		{
			get
			{
				if(this._settings == null)
				{
					this._settings = new Settings();
					this.Host.Plugins.Settings(this).LoadAssemblyParameters(this._settings);
					this._settings.PropertyChanged += _settings_PropertyChanged;
				}
				return this._settings;
			}
		}

		private void _settings_PropertyChanged(Object? sender, PropertyChangedEventArgs e)
		{
			this._agent = null;
		}

		internal IHost Host { get; }

		private IHostWindows HostWindows => this.Host as IHostWindows ?? throw new InvalidOperationException("Host does not support windows.");

		private static Dictionary<String, DockState> DocumentTypes
		{
			get => new Dictionary<String, DockState>()
			{
				{ typeof(PanelChat).ToString(), DockState.DockRightAutoHide },
			};
		}

		public Plugin(IHost host)
		{
			this.Host = host ?? throw new ArgumentNullException(nameof(host));
		}

		public IWindow? GetPluginControl(String typeName, Object args)
			=> this.CreateWindow(typeName, false, args);

		public IEnumerable<String> InvokeMessage(String message)
		{
			var responses = new List<String>();
			EventHandler<AgentResponseEventArgs> responseHandler =
				(Object? sender, AgentResponseEventArgs e) => responses.Add(e.Response);

			var agent = this.GetAgent();
			agent.AiResponseReceived += responseHandler;

			try
			{
				Task.Run(() => agent.InvokeMessageAsync(message, cancellationToken: CancellationToken.None))
					.GetAwaiter().GetResult();
			} finally
			{
				agent.AiResponseReceived -= responseHandler;
			}

			return responses;
		}

		private AssistantAgent GetAgent()
		{
			if(this._agent == null)
				this._agent = this.InitializeAgent();
			return this._agent;
		}

		internal AssistantAgent InitializeAgent()
		{
			PluginSettingsTools settingsTools = new PluginSettingsTools(this.Host);
			PluginMethodsTools methodsTools = new PluginMethodsTools(this.Host);
			ShellTools shellTools = new ShellTools();
			ToolsFactory toolsFactory = new ToolsFactory(this.Trace, settingsTools, methodsTools, shellTools);
			var result = new AssistantAgent(this.Trace, this.Host, toolsFactory);
			result.Initialize(this.Settings);

			return result;
		}

		Boolean IPlugin.OnConnection(ConnectMode mode)
		{
			var hostWindows = this.Host as IHostWindows;
			if(hostWindows != null)
			{
				IMenuItem menuTools = hostWindows.MainMenu.FindMenuItem("Tools");

				this._menuChat = menuTools.Create("OpenAI Chat");
				this._menuChat.Name = "Tools.McpBridge";
				this._menuChat.Click += (sender, e) => this.CreateWindow(typeof(PanelChat).ToString(), false);

				menuTools.Items.Add(this._menuChat);
			}

			return true;
		}

		Boolean IPlugin.OnDisconnection(DisconnectMode mode)
		{
			if(this._menuChat != null)
				this.HostWindows.MainMenu.Items.Remove(this._menuChat);
			return true;
		}

		private IWindow? CreateWindow(String typeName, Boolean searchForOpened, Object? args = null)
			=> Plugin.DocumentTypes.TryGetValue(typeName, out DockState state)
				? this.HostWindows.Windows.CreateWindow(this, typeName, searchForOpened, state, args)
				: null;

		private static TraceSource CreateTraceSource<T>(String? name = null) where T : IPlugin
		{
			TraceSource result = new TraceSource(typeof(T).Assembly.GetName().Name + name);
			result.Switch.Level = SourceLevels.All;
			result.Listeners.Remove("Default");
			result.Listeners.AddRange(System.Diagnostics.Trace.Listeners);
			return result;
		}
	}
}