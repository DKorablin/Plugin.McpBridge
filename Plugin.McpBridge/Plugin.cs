using System.ComponentModel;
using Microsoft.Extensions.AI;
using Plugin.McpBridge.Agents;
using Plugin.McpBridge.Data;
using Plugin.McpBridge.Events;
using Plugin.McpBridge.Mcp;
using Plugin.McpBridge.Tests;
using Plugin.McpBridge.Tools;
using SAL.Flatbed;
using SAL.Windows;

namespace Plugin.McpBridge
{
	public class Plugin : IPlugin, IPluginSettings<Settings>
	{
		private Settings? _settings;
		private AssistantAgent? _agent;

		private ProcessHost? _devUIHost;
		private ProcessHost? _agUIHost;
		private McpServer? _mcpServer;

		private IMenuItem? _menuChat;

		internal ITraceSource Trace { get; }

		Object IPluginSettings.Settings => this.Settings;

		public Settings Settings
		{
			get
			{
				if(this._settings == null)
				{
					this._settings = new Settings(this.Host);
					this.Host.Plugins.Settings(this).LoadAssemblyParameters(this._settings);
					this._settings.PropertyChanged += _settings_PropertyChanged;
				}
				return this._settings;
			}
		}

		private void _settings_PropertyChanged(Object? sender, PropertyChangedEventArgs e)
		{
			this._agent = null;

			this._devUIHost?.StopAsync().GetAwaiter().GetResult();
			this._devUIHost = null;
			this._agUIHost?.StopAsync().GetAwaiter().GetResult();
			this._agUIHost = null;
			this._mcpServer?.Dispose();
			this._mcpServer = null;

			this.Plugins_PluginsLoaded(sender, e);
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

		public Plugin(IHost host, ITraceSource trace)
		{
			this.Host = host ?? throw new ArgumentNullException(nameof(host));
			this.Trace = trace ?? throw new ArgumentNullException(nameof(trace));
		}

		public IWindow? GetPluginControl(String typeName, Object args)
			=> this.CreateWindow(typeName, false, args);

		public IEnumerable<String> InvokeMessage(String message)
		{
			var responses = new List<String>();
			EventHandler<AgentResponseEventArgs> responseHandler = (Object? sender, AgentResponseEventArgs e)
				=> responses.Add(e.Response);

			var provider = this.Settings.GetSelectedProvider()
				?? throw new InvalidOperationException("No AI provider configured.");

			var agent = this.GetAgent(provider);
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

		private AssistantAgent GetAgent(AiProviderDto provider)
		{
			if(this._agent == null)
				this._agent = this.InitializeAgent(provider);
			return this._agent;
		}

		internal AssistantAgent InitializeAgent(AiProviderDto provider)
		{
			ToolsFactory toolsFactory = new ToolsFactory(this.Host);

			var result = new AssistantAgent(this.Trace, this.Host, toolsFactory);
			result.Initialize(this.Settings, provider);
			return result;
		}

		Boolean IPlugin.OnConnection(ConnectMode mode)
		{
			this.Host.Plugins.PluginsLoaded += Plugins_PluginsLoaded;

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

		private void Plugins_PluginsLoaded(Object? sender, EventArgs e)
		{
			ToolsFactory toolsFactory = new ToolsFactory(this.Host);

			if(this.Settings.McpServerEnabled || this.Settings.DevUIEnabled || this.Settings.AgUIEnabled)
			{
				IEnumerable<AITool> bridgeTools = toolsFactory.CreateTools(this.Trace, this.Settings.ToolsPermission);
				this._mcpServer = new McpServer(this.Trace, this.Settings.McpServerUrl, bridgeTools);
				this._mcpServer.Start();
			}

			if(this.Settings.DevUIEnabled)
			{
				this._devUIHost = new ProcessHost(this.Host, ProcessHost.ExeType.DevUI);
				Task.Run(() => this._devUIHost.StartAsync(this.Settings, this.Settings.GetSelectedProvider()));
			}
			if(this.Settings.AgUIEnabled)
			{
				this._agUIHost = new ProcessHost(this.Host, ProcessHost.ExeType.AgUI);
				Task.Run(() => this._agUIHost.StartAsync(this.Settings, this.Settings.GetSelectedProvider()));
			}
		}

		Boolean IPlugin.OnDisconnection(DisconnectMode mode)
		{
			if(this._menuChat != null)
				this.HostWindows.MainMenu.Items.Remove(this._menuChat);
			this._devUIHost?.Dispose();
			this._agUIHost?.Dispose();
			this._mcpServer?.Dispose();
			return true;
		}

		private IWindow? CreateWindow(String typeName, Boolean searchForOpened, Object? args = null)
			=> Plugin.DocumentTypes.TryGetValue(typeName, out DockState state)
				? this.HostWindows.Windows.CreateWindow(this, typeName, searchForOpened, state, args)
				: null;
	}
}