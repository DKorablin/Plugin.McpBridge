using System.ComponentModel;
using Microsoft.Extensions.AI;
using Plugin.McpBridge.Agents;
using Plugin.McpBridge.Data;
using Plugin.McpBridge.Events;
using Plugin.McpBridge.Mcp;
using Plugin.McpBridge.Tests;
using Plugin.McpBridge.Tools;
using Plugin.McpBridge.Workflows;
using SAL.Flatbed;
using SAL.Windows;

namespace Plugin.McpBridge
{
	public class Plugin : IPlugin, IPluginSettings<Settings>
	{
		private Settings? _settings;
		internal static Plugin? StaticInstance { get; private set; }
		private AssistantAgent? _agent;

		private ProcessHost? _devUIHost;
		private ProcessHost? _agUIHost;
		private ProcessHost? _ragHost;
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
					this._settings = new Settings();
					this.Host.Plugins.Settings(this).LoadAssemblyParameters(this._settings);
					this._settings.PropertyChanged += this._settings_PropertyChanged;
				}
				return this._settings;
			}
		}

		private void _settings_PropertyChanged(Object? sender, PropertyChangedEventArgs e)
		{
			switch(e.PropertyName)
			{
			case nameof(this.Settings.LastConversationId):
				return;
			}

			this._agent = null;

			this._devUIHost?.StopAsync().GetAwaiter().GetResult();
			this._devUIHost = null;
			this._agUIHost?.StopAsync().GetAwaiter().GetResult();
			this._agUIHost = null;
			this._ragHost?.StopAsync().GetAwaiter().GetResult();
			this._ragHost = null;
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
			Plugin.StaticInstance = this;
			this.Host = host ?? throw new ArgumentNullException(nameof(host));
			this.Trace = trace ?? throw new ArgumentNullException(nameof(trace));
		}

		public IWindow? GetPluginControl(String typeName, Object args)
			=> this.CreateWindow(typeName, false, args);

		public IEnumerable<String> InvokeMessage(String message)
		{
			var responses = new List<String>();
			EventHandler<AgentResponseEventArgs> responseHandler = (Object? sender, AgentResponseEventArgs e) =>
			{
				if(e.Message != null)
					responses.Add(e.Message.Text);
			};

			var provider = this.Settings.SelectedAgent.GetSelectedProvider(this.Settings.AiProviders);

			var agent = Task.Run(() => this.GetAgent(provider)).GetAwaiter().GetResult();
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

		private async Task<AssistantAgent> GetAgent(AiProviderDto provider)
		{
			if(this._agent == null)
				this._agent = await this.InitializeAgent(provider);
			return this._agent;
		}

		internal async Task<AssistantAgent> InitializeAgent(AiProviderDto provider, String? conversationId = null)
		{
			try
			{
				ToolsFactory toolsFactory = new ToolsFactory(this.Host, this.Settings, this.Settings.SelectedAgent);
				AgentFactory agentFactory = new AgentFactory();
				FileSystemAgentSessionStore? sessionStore = this.CreateSessionStore();

				var result = new AssistantAgent(this.Trace, this.Host, toolsFactory, agentFactory);
				await result.Initialize(this.Settings, provider, sessionStore, conversationId);
				return result;
			}catch(Exception exc)
			{
				this.Trace.TraceData(System.Diagnostics.TraceEventType.Error, 10, exc);
				throw;
			}
		}

		internal async Task<AssistantAgent> InitializeWorkflowAgent(WorkflowFactoryItem workflow, String? conversationId = null, CancellationToken token = default)
		{
			try
			{
				ToolsFactory toolsFactory = new ToolsFactory(this.Host, this.Settings, this.Settings.SelectedAgent);
				AgentFactory agentFactory = new AgentFactory();
				FileSystemAgentSessionStore? sessionStore = this.CreateSessionStore();

				var result = new AssistantAgent(this.Trace, this.Host, toolsFactory, agentFactory);
				await result.InitializeWorkflow(this.Settings, workflow, sessionStore, conversationId, token);
				return result;
			} catch(Exception exc)
			{
				this.Trace.TraceData(System.Diagnostics.TraceEventType.Error, 10, exc);
				throw;
			}
		}

		private FileSystemAgentSessionStore? CreateSessionStore()
			=> this.Settings.SessionStorageDirectory != null
				? new FileSystemAgentSessionStore(this.Settings.SessionStorageDirectory)
				: null;

		Boolean IPlugin.OnConnection(ConnectMode mode)
		{
			this.Host.Plugins.PluginsLoaded += this.Plugins_PluginsLoaded;

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
			ToolsFactory toolsFactory = new ToolsFactory(this.Host, this.Settings, this.Settings.SelectedAgent);

			if(this.Settings.McpServerEnabled)
			{
				IEnumerable<AITool> bridgeTools = toolsFactory.CreateTools(this.Trace);
				this._mcpServer = new McpServer(this.Trace, this.Settings.McpServerUrl, bridgeTools);
				this._mcpServer.Start();
			}

			if(this.Settings.DevUIEnabled)
			{
				this._devUIHost = new ProcessHost(this.Host, ProcessHost.ExeType.DevUI);
				Task.Run(() => this._devUIHost.StartAsync(this.Settings));
			}
			if(this.Settings.AgUIEnabled)
			{
				this._agUIHost = new ProcessHost(this.Host, ProcessHost.ExeType.AgUI);
				Task.Run(() => this._agUIHost.StartAsync(this.Settings));
			}
			if(this.Settings.RagProcessEnabled)
			{
				this._ragHost = new ProcessHost(this.Host, ProcessHost.ExeType.RAG);
				Task.Run(() => this._ragHost.StartAsync(this.Settings));
			}
		}

		Boolean IPlugin.OnDisconnection(DisconnectMode mode)
		{
			if(this._menuChat != null)
				this.HostWindows.MainMenu.Items.Remove(this._menuChat);
			this._devUIHost?.Dispose();
			this._agUIHost?.Dispose();
			this._ragHost?.Dispose();
			this._mcpServer?.Dispose();
			return true;
		}

		private IWindow? CreateWindow(String typeName, Boolean searchForOpened, Object? args = null)
			=> Plugin.DocumentTypes.TryGetValue(typeName, out DockState state)
				? this.HostWindows.Windows.CreateWindow(this, typeName, searchForOpened, state, args)
				: null;
	}
}