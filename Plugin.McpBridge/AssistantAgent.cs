using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using Plugin.McpBridge.Helpers;
using System.ClientModel;
using System.ClientModel.Primitives;

namespace Plugin.McpBridge
{
	/// <summary>Manages the MAF AIAgent instance and drives the multi-turn agent loop.</summary>
	internal sealed class AssistantAgent
	{
		private readonly TraceSource _trace;
		private readonly McpBridge _mcpBridge;
		private readonly PluginSettingsHelper _settingsHelper;
		private readonly PluginMethodsHelper _methodsHelper;
		private ChatClientAgent? _agent;
		private AgentSession? _session;

		public event EventHandler<AgentResponseEventArgs>? AiResponseReceived;
		public event EventHandler<AgentConfirmationEventArgs>? ConfirmationRequired;

		public AssistantAgent(TraceSource trace, McpBridge mcpBridge, PluginSettingsHelper settingsHelper, PluginMethodsHelper methodsHelper)
		{
			this._trace = trace ?? throw new ArgumentNullException(nameof(trace));
			this._mcpBridge = mcpBridge ?? throw new ArgumentNullException(nameof(mcpBridge));
			this._settingsHelper = settingsHelper ?? throw new ArgumentNullException(nameof(settingsHelper));
			this._methodsHelper = methodsHelper ?? throw new ArgumentNullException(nameof(methodsHelper));
		}

		public void Initialize(Settings settings)
		{
			this._session = null;

			Boolean requiresApiKey = settings.ProviderType != AiProviderType.LocalOpenAICompatible;
			if(requiresApiKey && String.IsNullOrWhiteSpace(settings.ApiKey))
			{
				this._agent = null;
				return;
			}

			HttpClient httpClient = new HttpClient { Timeout = settings.ConnectionTimeout };
			IChatClient chatClient = this.BuildChatClient(settings, httpClient);

			IChatClient configuredClient = new ChatClientBuilder(chatClient)
				.ConfigureOptions(options =>
				{
					if(settings.MaxTokens.HasValue)
						options.MaxOutputTokens = settings.MaxTokens.Value;
					if(settings.Temperature.HasValue)
						options.Temperature = (Single)settings.Temperature.Value;
				})
				.Build();

			this._agent = (ChatClientAgent)configuredClient.AsAIAgent(
				instructions: settings.AssistantSystemPrompt,
				tools:
				[
					AIFunctionFactory.Create(this.SettingsList,   "SettingsList",   "List all available settings for a plugin"),
					AIFunctionFactory.Create(this.SettingsGet,    "SettingsGet",    "Get the current value of a specific plugin setting"),
					AIFunctionFactory.Create(this.SettingsSet,    "SettingsSet",    "Update a plugin setting value; requires user confirmation"),
					AIFunctionFactory.Create(this.MethodsList,    "MethodsList",    "List all available methods for a plugin"),
					AIFunctionFactory.Create(this.MethodsInvoke,  "MethodsInvoke",  "Invoke a plugin method; requires user confirmation"),
				]);
		}

		public async Task InvokeMessageAsync(String message, CancellationToken cancellationToken = default)
		{
			if(String.IsNullOrWhiteSpace(message))
			{
				this.OnAiResponseReceived(new AgentResponseEventArgs("Message is empty.", true));
				return;
			}

			this._trace.TraceEvent(TraceEventType.Verbose, 0, "< " + message);

			if(this._agent == null)
			{
				this.OnAiResponseReceived(new AgentResponseEventArgs("AI is not configured. Set ApiKey and ModelId in plugin settings.", true));
				return;
			}

			if(this._session == null)
				this._session = await this._agent.CreateSessionAsync(cancellationToken);

			try
			{
				AgentResponse response = await this._agent.RunAsync(this.BuildAiPrompt(message), this._session, null, cancellationToken);
				this.HandleResponse(response);
			}
			catch(HttpRequestException exc)
			{
				this._trace.TraceData(TraceEventType.Error, 0, exc);
				this.OnAiResponseReceived(new AgentResponseEventArgs($"AI request failed: {exc.Message}", true));
			}
			catch(OperationCanceledException)
			{
				this.OnAiResponseReceived(new AgentResponseEventArgs("Operation was cancelled.", true));
			}catch(Exception exc)
			{
				this._trace.TraceData(TraceEventType.Error, 0, exc);
				throw;
			}
		}

		private void OnAiResponseReceived(AgentResponseEventArgs e)
			=> this.AiResponseReceived?.Invoke(this, e);

		private void OnConfirmationRequired(AgentConfirmationEventArgs e)
			=> this.ConfirmationRequired?.Invoke(this, e);

		private void HandleResponse(AgentResponse response)
		{
			String aiResponse = response.ToString();
			this._trace.TraceEvent(TraceEventType.Verbose, 0, "> " + aiResponse);
			this.OnAiResponseReceived(new AgentResponseEventArgs(aiResponse, true));
		}

		private Task<Boolean> RequestConfirmationAsync(String actionDescription)
		{
			if(this.ConfirmationRequired == null)
				return Task.FromResult(false);

			AgentConfirmationEventArgs confirmArgs = new AgentConfirmationEventArgs(actionDescription);
			this.OnConfirmationRequired(confirmArgs);
			return confirmArgs.ConfirmationTask;
		}

		private String BuildAiPrompt(String userMessage)
		{
			String pluginInventory = this._mcpBridge.ListLoadedPluginsFromHost();
			String mcpTools = this._mcpBridge.ListLoadedToolsFromMcpClient();
			return $@"{userMessage}

Loaded SAL plugins:
{pluginInventory}

MCP tools discovered by MCP client:
{mcpTools}";
		}

		private IChatClient BuildChatClient(Settings settings, HttpClient httpClient)
		{
			HttpClientPipelineTransport transport = new HttpClientPipelineTransport(httpClient);
			switch(settings.ProviderType)
			{
			case AiProviderType.AzureOpenAI:
				return new AzureOpenAIClient(
					new Uri(settings.ModelEndpointUrl!),
					new ApiKeyCredential(settings.ApiKey!),
					new AzureOpenAIClientOptions { Transport = transport })
					.GetChatClient(settings.DeploymentName ?? settings.ModelId)
				.AsIChatClient();
			default:
				OpenAIClientOptions clientOptions = new OpenAIClientOptions
				{
					Transport = transport
				};
				if(settings.ModelEndpointUrl != null)
					clientOptions.Endpoint = new Uri(settings.ModelEndpointUrl);

				return new OpenAIClient(
					new ApiKeyCredential(settings.ApiKey ?? "local-no-key"),
					clientOptions)
					.GetChatClient(settings.ModelId)
				.AsIChatClient();
			}
		}

		private String SettingsList([Description("Plugin identifier")] String pluginId)
		{
			this._trace.TraceEvent(TraceEventType.Verbose, 0, $"[tool] SettingsList plugin={pluginId}");
			String result = this._settingsHelper.ListPluginSettings(pluginId);
			this._trace.TraceEvent(TraceEventType.Verbose, 0, "[tool result] " + result);
			return result;
		}

		private String SettingsGet(
			[Description("Plugin identifier")] String pluginId,
			[Description("Setting name")] String settingName)
		{
			this._trace.TraceEvent(TraceEventType.Verbose, 0, $"[tool] SettingsGet plugin={pluginId} setting={settingName}");
			String result = this._settingsHelper.ReadPluginSetting(pluginId, settingName);
			this._trace.TraceEvent(TraceEventType.Verbose, 0, "[tool result] " + result);
			return result;
		}

		private async Task<String> SettingsSet(
			[Description("Plugin identifier")] String pluginId,
			[Description("Setting name")] String settingName,
			[Description("New value as JSON")] String valueJson)
		{
			this._trace.TraceEvent(TraceEventType.Verbose, 0, $"[tool] SettingsSet plugin={pluginId} setting={settingName} value={valueJson}");
			Boolean confirmed = await this.RequestConfirmationAsync($"SETTINGS SET {pluginId} {settingName}={valueJson}");
			String result = confirmed
				? this._settingsHelper.UpdatePluginSetting(pluginId, settingName, valueJson)
				: "Operation declined by user.";
			this._trace.TraceEvent(TraceEventType.Verbose, 0, "[tool result] " + result);
			return result;
		}

		private String MethodsList([Description("Plugin identifier")] String pluginId)
		{
			this._trace.TraceEvent(TraceEventType.Verbose, 0, $"[tool] MethodsList plugin={pluginId}");
			String result = this._methodsHelper.ListPluginMethods(pluginId);
			this._trace.TraceEvent(TraceEventType.Verbose, 0, "[tool result] " + result);
			return result;
		}

		private async Task<String> MethodsInvoke(
			[Description("Plugin identifier")] String pluginId,
			[Description("Method name")] String methodName,
			[Description("Arguments as JSON")] String argsJson)
		{
			this._trace.TraceEvent(TraceEventType.Verbose, 0, $"[tool] MethodsInvoke plugin={pluginId} method={methodName} args={argsJson}");
			Boolean confirmed = await this.RequestConfirmationAsync($"METHODS INVOKE {pluginId} {methodName} {argsJson}");
			String result = confirmed
				? this._methodsHelper.InvokePluginMethodPlaceholder(pluginId, methodName, argsJson)
				: "Operation declined by user.";
			this._trace.TraceEvent(TraceEventType.Verbose, 0, "[tool result] " + result);
			return result;
		}
	}
}
