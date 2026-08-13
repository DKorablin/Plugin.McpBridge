using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.AI;
using Plugin.McpBridge.Agents;
using McpBridge.Core;
using McpBridge.Core.Remoting;
using Plugin.McpBridge.Data;
using Plugin.McpBridge.Events;
using Plugin.McpBridge.Hosting;
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

		private readonly Dictionary<ProcessType, ProcessHost> _processHosts = new Dictionary<ProcessType, ProcessHost>();
		private McpServer? _mcpServer;

		private IMenuItem? _menuChat;

		internal ITraceSource Trace { get; }

		internal IMcpTrace McpTrace { get; }

		Object IPluginSettings.Settings => this.Settings;

		public Settings Settings
		{
			get
			{
				if(this._settings == null)
				{
					PropertyGridMetadata.Register();
					AiAgentDto.Defaults.AssistantSystemPrompt = @"You are a SAL automation assistant.
Use available MCP tools when useful.
Return clear user-facing responses, or a command payload only when automation is required.
Before using relative dates (today, yesterday, last hour), obtain the current system time from the SystemInformation tool.";

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

			foreach(var process in this._processHosts.Values)
				Task.Run(() => process.StopAsync());
			this._processHosts.Clear();

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
			this.McpTrace = new TraceSourceMcpTrace(this.Trace);
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

		internal ToolsFactory CreateToolsFactory()
		{
			List<ToolsDiscoveryBase> tools = new List<ToolsDiscoveryBase>()
			{
				new PluginSettingsTools(this.Host),
				//new PluginMethodsTools(this.Host),
				new PluginMethodsToolsExtractor(this.Host, this.Settings.SelectedAgent),
				new ShellTools(),
			};

			if(this.Host is IHostWindows hostWindows)
				tools.Add(new WindowsTools(hostWindows));

			return new ToolsFactory(this.Settings, this.Settings.SelectedAgent, tools.ToArray());
		}

		internal async Task<AssistantAgent> InitializeAgent(AiProviderDto provider, String? conversationId = null)
		{
			try
			{
				ToolsFactory toolsFactory = CreateToolsFactory();
				AgentFactory agentFactory = new AgentFactory();
				String? sessionStoreDir = this.Settings.GetSessionStorageDirectory();
				FileSystemAgentSessionStore? sessionStore = sessionStoreDir == null
					? null
					: new FileSystemAgentSessionStore(sessionStoreDir);

				var result = new AssistantAgent(this.McpTrace, toolsFactory, agentFactory);
				String instructions = this.BuildSystemInstructions();
				await result.Initialize(this.Settings, instructions, sessionStore, conversationId);
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
				ToolsFactory toolsFactory = CreateToolsFactory();
				AgentFactory agentFactory = new AgentFactory();
				FileSystemAgentSessionStore? sessionStore = this.CreateSessionStore();

				var result = new AssistantWorkflowAgent(this.McpTrace, toolsFactory, agentFactory);
				await result.InitializeWorkflow(this.Settings, workflow, sessionStore, conversationId, token);
				return result;
			} catch(Exception exc)
			{
				this.Trace.TraceData(System.Diagnostics.TraceEventType.Error, 10, exc);
				throw;
			}
		}

		private FileSystemAgentSessionStore? CreateSessionStore()
			=> this.Settings.EnableSessionStorage && this.Settings.SessionStorageDirectory != null
				? new FileSystemAgentSessionStore(this.Settings.GetSessionStorageDirectory())
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

		Boolean IPlugin.OnDisconnection(DisconnectMode mode)
		{
			if(this._menuChat != null)
				this.HostWindows.MainMenu.Items.Remove(this._menuChat);

			foreach(var process in this._processHosts.Values)
				process.Dispose();
			this._processHosts.Clear();

			this._mcpServer?.Dispose();
			return true;
		}

		private void Plugins_PluginsLoaded(Object? sender, EventArgs e)
		{
			ToolsFactory toolsFactory = CreateToolsFactory();

			if(this.Settings.McpServerEnabled)
			{
				IEnumerable<AITool> bridgeTools = toolsFactory.CreateTools(this.McpTrace);
				this._mcpServer = new McpServer(this.Trace, this.Settings.McpServerUrl, bridgeTools);
				this._mcpServer.Start();
			}

			String instructions = this.BuildSystemInstructions();
			String assemblyName = Plugin.GetAssemblyName();
			foreach(ProcessType exeType in this.Settings.EnabledProcesses)
			{
				String traceName = $"{assemblyName}.{exeType}";
				IMcpTrace processTrace = new TraceSourceMcpTrace(this.Host.Plugins.CreateTraceSource(traceName));
				var process = this._processHosts[exeType] = new ProcessHost(exeType, processTrace, this.OnProcessFailed);
				Task.Run(() => process.StartAsync(this.Settings, instructions));
			}
		}

		private void OnProcessFailed(ProcessType exeType, Int32 exitCode)
		{
			switch(exeType)
			{
			case ProcessType.DTS:
			case ProcessType.RAG:
				this.Trace.TraceEvent(TraceEventType.Warning, 0, $"Process {exeType} failed with exit code {exitCode}. Disabling...");
				this.Settings.DisableProcess(exeType);
				break;
			default:
				this.Trace.TraceEvent(TraceEventType.Warning, 0, $"Process {exeType} failed with exit code {exitCode}.");
				break;
			}
		}

		private IWindow? CreateWindow(String typeName, Boolean searchForOpened, Object? args = null)
			=> Plugin.DocumentTypes.TryGetValue(typeName, out DockState state)
				? this.HostWindows.Windows.CreateWindow(this, typeName, searchForOpened, state, args)
				: null;

		private String BuildSystemInstructions()
		{
			String pluginInventory = ListPluginInventory(this.Settings.SelectedAgent, this.Host);
			return BuildSystemInstructions(this.Settings.SelectedAgent.AssistantSystemPrompt, pluginInventory);

			String BuildSystemInstructions(String? systemPrompt, String pluginInventory)
			{
				if(pluginInventory.Length > 0)
				{
					StringBuilder sb = new StringBuilder(systemPrompt);
					sb.AppendLine();
					sb.AppendLine();
					sb.AppendLine("Loaded SAL plugins:");
					sb.AppendLine(pluginInventory);
					return sb.ToString().Trim();
				} else
					return systemPrompt;
			}

			String ListPluginInventory(AiAgentDto agent, IHost host)
			{
				var allowedPlugins = agent.PluginsPermission;
				if(allowedPlugins?.Length == 0)
					return String.Empty;

				List<String> pluginsText = new List<String>();
				Boolean allAllowed = allowedPlugins == null;
				var allowedSet = allAllowed ? null : new HashSet<String>(allowedPlugins!);
				foreach(IPluginDescription pluginDescription in host.Plugins)
					if(allAllowed || allowedSet!.Contains(pluginDescription.ID))
					{
						String hasSettings = PluginSettingsTools.HasPluginSettings(pluginDescription) ? "yes" : "no";
						String pluginText = @$"- {pluginDescription.ID} | {pluginDescription.Name} | {pluginDescription.Version} | Settings: {hasSettings}";
						pluginsText.Add(pluginText);
					}

				return String.Join(Environment.NewLine, pluginsText.ToArray());
			}
		}

		internal static String GetAssemblyName()
			=> typeof(Plugin).Assembly.GetName().Name ?? String.Empty;
	}
}