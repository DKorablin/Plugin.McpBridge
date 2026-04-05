using System.Diagnostics;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Plugin.McpBridge.Helpers;
using SAL.Flatbed;

namespace Plugin.McpBridge
{
	/// <summary>Manages the Semantic Kernel instance and drives the multi-turn agent loop.</summary>
	internal sealed class AssistantAgent
	{
		private readonly TraceSource _trace;
		private readonly McpBridge _mcpBridge;
		private readonly PluginSettingsHelper _settingsHelper;
		private readonly PluginMethodsHelper _methodsHelper;
		private Kernel? _kernel;
		private ChatHistory? _chatHistory;

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
			Boolean requiresApiKey = settings.ProviderType != AiProviderType.LocalOpenAICompatible;
			if(requiresApiKey && String.IsNullOrWhiteSpace(settings.ApiKey))
			{
				this._kernel = null;
				return;
			}

			IKernelBuilder kernelBuilder = Kernel.CreateBuilder();
			HttpClient httpClient = new HttpClient { Timeout = settings.ConnectionTimeout };
			switch(settings.ProviderType)
			{
			case AiProviderType.AzureOpenAI:
				kernelBuilder.AddAzureOpenAIChatCompletion(
					deploymentName: settings.DeploymentName,
					endpoint: settings.ModelEndpointUrl,
					apiKey: settings.ApiKey,
					modelId: settings.ModelId,
					httpClient: httpClient);
				break;
			default:
				if(settings.ModelEndpointUrl == null)
					kernelBuilder.AddOpenAIChatCompletion(
						modelId: settings.ModelId,
						apiKey: "local-no-key",
						orgId: settings.DeploymentName,
						serviceId: null,
						httpClient: httpClient);
				else
					kernelBuilder.AddOpenAIChatCompletion(
						modelId: settings.ModelId,
						endpoint: new Uri(settings.ModelEndpointUrl),
						apiKey: settings.ApiKey,
						orgId: settings.DeploymentName,
						serviceId: null,
						httpClient: httpClient);
				break;
			}

			this._kernel = kernelBuilder.Build();
		}

		public async Task InvokeMessageAsync(String message, Settings settings, CancellationToken cancellationToken = default)
		{
			if(String.IsNullOrWhiteSpace(message))
			{
				this.OnAiResponseReceived(new AgentResponseEventArgs("Message is empty.", true));
				return;
			}

			this._trace.TraceEvent(TraceEventType.Verbose, 0, "< " + message);

			ChatHistory chatHistory = this.CreateChatHistory(message, settings);
			Int32 loopCap = Math.Max(settings.AgentLoopCap, 1);

			for(Int32 loopIndex = 0; loopIndex < loopCap; loopIndex++)
			{
				String aiResponse = await this.GetAssistantResponseAsync(chatHistory, settings, cancellationToken);
				chatHistory.AddAssistantMessage(aiResponse);
				this._trace.TraceEvent(TraceEventType.Verbose, 0, "> " + aiResponse);

				(Boolean isCommand, String commandResult) = await this.TryHandleSystemCommandAsync(aiResponse);
				if(!isCommand)
				{
					this.OnAiResponseReceived(new AgentResponseEventArgs(aiResponse, true));
					return;
				}

				chatHistory.AddSystemMessage(this.BuildCommandResultPrompt(commandResult, loopIndex + 1, loopCap));
			}

			this.OnAiResponseReceived(new AgentResponseEventArgs("Assistant reached the configured agent loop cap before returning a final user response.", true));
		}

		private void OnAiResponseReceived(AgentResponseEventArgs e)
			=> this.AiResponseReceived?.Invoke(this, e);

		private void OnConfirmationRequired(AgentConfirmationEventArgs e)
			=> this.ConfirmationRequired?.Invoke(this, e);

		private ChatHistory CreateChatHistory(String message, Settings settings)
		{
			if(this._chatHistory == null)
			{
				this._chatHistory = new ChatHistory();
				if(!String.IsNullOrWhiteSpace(settings.AssistantSystemPrompt))
					this._chatHistory.AddSystemMessage(settings.AssistantSystemPrompt);
			}

			this._chatHistory.AddUserMessage(this.BuildAiPrompt(message));
			return this._chatHistory;
		}

		private String BuildAiPrompt(String userMessage)
		{
			String pluginInventory = this._mcpBridge.ListLoadedPluginsFromHost();
			String mcpTools = this._mcpBridge.ListLoadedToolsFromMcpClient();
			return $"User message: {userMessage}{Environment.NewLine}{Environment.NewLine}Loaded SAL plugins:{Environment.NewLine}{pluginInventory}{Environment.NewLine}{Environment.NewLine}MCP tools discovered by MCP client:{Environment.NewLine}{mcpTools}{Environment.NewLine}{Environment.NewLine}Supported command payloads:{Environment.NewLine}COMMAND: SETTINGS LIST <plugin>{Environment.NewLine}COMMAND: SETTINGS GET <plugin> <setting>{Environment.NewLine}COMMAND: SETTINGS SET <plugin> <setting>=<value>{Environment.NewLine}COMMAND: METHODS LIST <plugin>{Environment.NewLine}COMMAND: METHODS INVOKE <plugin> <method> <argsJson>{Environment.NewLine}{Environment.NewLine}If automation command is required, return a supported payload prefixed with COMMAND:. Otherwise return a normal response for the user.";
		}

		private String BuildCommandResultPrompt(String commandResult, Int32 loopIndex, Int32 loopCap)
			=> $"System command result {loopIndex}/{loopCap}:{Environment.NewLine}{commandResult}{Environment.NewLine}{Environment.NewLine}If additional automation is required, return another payload prefixed with COMMAND:. Otherwise return the final user-facing response only.";

		private async Task<String> GetAssistantResponseAsync(ChatHistory chatHistory, Settings settings, CancellationToken cancellationToken)
		{
			if(this._kernel == null)
				return "AI is not configured. Set ApiKey and ModelId in plugin settings.";

			IChatCompletionService chatCompletionService = this._kernel.GetRequiredService<IChatCompletionService>();
			OpenAIPromptExecutionSettings executionSettings = new OpenAIPromptExecutionSettings();

			if(settings.MaxTokens.HasValue)
				executionSettings.MaxTokens = settings.MaxTokens.Value;
			if(settings.Temperature.HasValue)
				executionSettings.Temperature = settings.Temperature.Value;

			try
			{
				ChatMessageContent response = await chatCompletionService.GetChatMessageContentAsync(chatHistory, executionSettings, this._kernel, cancellationToken);
				return String.IsNullOrWhiteSpace(response.Content) ? "Model returned an empty response." : response.Content ?? String.Empty;
			} catch(HttpOperationException exc)
			{
				this._trace.TraceData(TraceEventType.Error, 0, exc);
				return $"AI request failed with status code {exc.StatusCode}: {exc.Message}";
			}
		}

		private async Task<(Boolean isCommand, String commandResult)> TryHandleSystemCommandAsync(String aiResponse)
		{
			if(!aiResponse.StartsWith("COMMAND:", StringComparison.OrdinalIgnoreCase))
				return (false, String.Empty);

			String commandPayload = aiResponse.Substring("COMMAND:".Length).Trim();
			(Boolean handled, String? result) = await this.TryHandleSettingsCommandAsync(commandPayload);
			if(handled)
				return (true, result);

			(handled, result) = await this.TryHandleMethodsCommandAsync(commandPayload);
			if(handled)
				return (true, result);

			String commandResult = String.IsNullOrWhiteSpace(commandPayload)
				? "Command payload is empty."
				: $"Command interception placeholder: {commandPayload}";

			return (true, commandResult);
		}

		private async Task<(Boolean handled, String commandResult)> TryHandleSettingsCommandAsync(String commandPayload)
		{
			if(!commandPayload.StartsWith("SETTINGS ", StringComparison.OrdinalIgnoreCase))
				return (false, String.Empty);

			String settingsPayload = commandPayload.Substring("SETTINGS ".Length).Trim();
			if(settingsPayload.StartsWith("LIST ", StringComparison.OrdinalIgnoreCase))
			{
				String pluginId = settingsPayload.Substring("LIST ".Length).Trim();
				return (true, this._settingsHelper.ListPluginSettings(pluginId));
			}

			if(settingsPayload.StartsWith("GET ", StringComparison.OrdinalIgnoreCase))
			{
				String readPayload = settingsPayload.Substring("GET ".Length).Trim();
				Int32 separatorIndex = readPayload.IndexOf(' ');
				if(separatorIndex <= 0 || separatorIndex >= readPayload.Length - 1)
					return (true, "SETTINGS GET syntax: COMMAND: SETTINGS GET <plugin> <setting>");

				String pluginId = readPayload.Substring(0, separatorIndex).Trim();
				String settingName = readPayload.Substring(separatorIndex + 1).Trim();
				return (true, this._settingsHelper.ReadPluginSetting(pluginId, settingName));
			}

			if(settingsPayload.StartsWith("SET ", StringComparison.OrdinalIgnoreCase))
			{
				String writePayload = settingsPayload.Substring("SET ".Length).Trim();
				Int32 separatorIndex = writePayload.IndexOf(' ');
				Int32 equalsIndex = writePayload.IndexOf('=');
				if(separatorIndex <= 0 || equalsIndex <= separatorIndex + 1 || equalsIndex >= writePayload.Length)
					return (true, "SETTINGS SET syntax: COMMAND: SETTINGS SET <plugin> <setting>=<value>");

				String pluginId = writePayload.Substring(0, separatorIndex).Trim();
				String settingName = writePayload.Substring(separatorIndex + 1, equalsIndex - separatorIndex - 1).Trim();
				String settingValue = writePayload.Substring(equalsIndex + 1).Trim();
				Boolean confirmed = await this.RequestConfirmationAsync($"SETTINGS SET {pluginId} {settingName}={settingValue}");
				return confirmed
					? (true, this._settingsHelper.UpdatePluginSetting(pluginId, settingName, settingValue))
					: (true, "Operation was cancelled.");
			}

			return (true, "Supported settings commands: SETTINGS LIST <plugin>, SETTINGS GET <plugin> <setting>, SETTINGS SET <plugin> <setting>=<value>");
		}

		private async Task<(Boolean handled, String commandResult)> TryHandleMethodsCommandAsync(String commandPayload)
		{
			if(!commandPayload.StartsWith("METHODS ", StringComparison.OrdinalIgnoreCase))
				return (false, String.Empty);

			String methodsPayload = commandPayload.Substring("METHODS ".Length).Trim();
			if(methodsPayload.StartsWith("LIST ", StringComparison.OrdinalIgnoreCase))
			{
				String pluginId = methodsPayload.Substring("LIST ".Length).Trim();
				return (true, this._methodsHelper.ListPluginMethods(pluginId));
			}

			if(methodsPayload.StartsWith("INVOKE ", StringComparison.OrdinalIgnoreCase))
			{
				String invokePayload = methodsPayload.Substring("INVOKE ".Length).Trim();
				Int32 firstSpace = invokePayload.IndexOf(' ');
				if(firstSpace <= 0 || firstSpace >= invokePayload.Length - 1)
					return (true, "METHODS INVOKE syntax: COMMAND: METHODS INVOKE <plugin> <method> <argsJson>");

				String pluginId = invokePayload.Substring(0, firstSpace).Trim();
				String remainder = invokePayload.Substring(firstSpace + 1).Trim();
				Int32 secondSpace = remainder.IndexOf(' ');
				String methodName = secondSpace < 0 ? remainder : remainder.Substring(0, secondSpace).Trim();
				String argumentsJson = secondSpace < 0 ? "{}" : remainder.Substring(secondSpace + 1).Trim();
				Boolean confirmed = await this.RequestConfirmationAsync($"METHODS INVOKE {pluginId} {methodName} {argumentsJson}");
				return confirmed
					? (true, this._methodsHelper.InvokePluginMethodPlaceholder(pluginId, methodName, argumentsJson))
					: (true, "Operation was cancelled.");
			}

			return (true, "Supported methods commands: METHODS LIST <plugin>, METHODS INVOKE <plugin> <method> <argsJson>");
		}

		private Task<Boolean> RequestConfirmationAsync(String actionDescription)
		{
			if(this.ConfirmationRequired == null)
				return Task.FromResult(false);

			AgentConfirmationEventArgs confirmArgs = new AgentConfirmationEventArgs(actionDescription);
			this.OnConfirmationRequired(confirmArgs);
			return confirmArgs.ConfirmationTask;
		}
	}
}