using System.Diagnostics;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Plugin.McpBridge.Helpers;

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

				AgentCommand command = AgentCommand.Parse(aiResponse);
				if(command.IsCommand)
				{
					String commandResult = await this.ExecuteAsync(command);
					this._trace.TraceEvent(TraceEventType.Verbose, 0, "< " + commandResult);
					chatHistory.AddSystemMessage(AssistantAgent.BuildCommandResultPrompt(commandResult, loopIndex + 1, loopCap));
				} else
				{
					this.OnAiResponseReceived(new AgentResponseEventArgs(aiResponse, true));
					return;
				}
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
			return $@"User message: {userMessage}

Loaded SAL plugins:
{pluginInventory}

MCP tools discovered by MCP client:
{mcpTools}

Supported command payloads:
COMMAND: SETTINGS LIST <plugin>
COMMAND: SETTINGS GET <plugin> <setting>
COMMAND: SETTINGS SET <plugin> <setting>=<valueJson>
COMMAND: METHODS LIST <plugin>
COMMAND: METHODS INVOKE <plugin> <method> <argsJson>

If automation command is required, return a supported payload prefixed with COMMAND:. Otherwise return the final user-facing response only.";
		}

		private static String BuildCommandResultPrompt(String commandResult, Int32 loopIndex, Int32 loopCap)
			=> $@"System command result {loopIndex}/{loopCap}:
{commandResult}

If additional automation is required, return another payload prefixed with COMMAND:. Otherwise return the final user-facing response only.";

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

		private Task<Boolean> RequestConfirmationAsync(String actionDescription)
		{
			if(this.ConfirmationRequired == null)
				return Task.FromResult(false);

			AgentConfirmationEventArgs confirmArgs = new AgentConfirmationEventArgs(actionDescription);
			this.OnConfirmationRequired(confirmArgs);
			return confirmArgs.ConfirmationTask;
		}

		private async Task<String> ExecuteAsync(AgentCommand command)
		{
			try
			{
				switch(command.CommandGroup)
				{
				case "SETTINGS":
					return await this.ExecuteSettingsCommandAsync(command);
				case "METHODS":
					return await this.ExecuteMethodsCommandAsync(command);
				default:
					return String.IsNullOrWhiteSpace(command.CommandGroup)
						? "Command payload is empty."
						: $"Command interception placeholder: {command.CommandGroup}";
				}
			}catch(ArgumentException exc)
			{
				this._trace.TraceData(TraceEventType.Warning, 0, exc);
				return exc.Message;
			}catch(OperationCanceledException exc)
			{
				this._trace.TraceData(TraceEventType.Stop, 0, exc);
				return "Operation was cancelled.";
			}
		}

		private async Task<String> ExecuteSettingsCommandAsync(AgentCommand command)
		{
			switch(command.Command)
			{
			case "LIST":
				return String.IsNullOrWhiteSpace(command.PluginId)
					? throw new ArgumentException("SETTINGS LIST syntax: COMMAND: SETTINGS LIST <plugin>")
					: this._settingsHelper.ListPluginSettings(command.PluginId);
			case "GET":
				return String.IsNullOrWhiteSpace(command.PluginId) || String.IsNullOrWhiteSpace(command.MemberName)
					? throw new ArgumentException("SETTINGS GET syntax: COMMAND: SETTINGS GET <plugin> <setting>")
					: this._settingsHelper.ReadPluginSetting(command.PluginId, command.MemberName);
			case "SET":
				if(String.IsNullOrWhiteSpace(command.PluginId) || String.IsNullOrWhiteSpace(command.MemberName))
					throw new ArgumentException("SETTINGS SET syntax: COMMAND: SETTINGS SET <plugin> <setting>=<valueJson>");

				Boolean setConfirmed = await this.RequestConfirmationAsync($"SETTINGS SET {command.PluginId} {command.MemberName}={command.Arguments}");
				return setConfirmed
					? this._settingsHelper.UpdatePluginSetting(command.PluginId, command.MemberName, command.Arguments)
					: throw new OperationCanceledException();
			default:
				throw new ArgumentException("Supported settings commands: SETTINGS LIST <plugin>, SETTINGS GET <plugin> <setting>, SETTINGS SET <plugin> <setting>=<value>");
			}
		}

		private async Task<String> ExecuteMethodsCommandAsync(AgentCommand command)
		{
			switch(command.Command)
			{
			case "LIST":
				return String.IsNullOrWhiteSpace(command.PluginId)
					? throw new ArgumentException("METHODS LIST syntax: COMMAND: METHODS LIST <plugin>")
					: this._methodsHelper.ListPluginMethods(command.PluginId);
			case "INVOKE":
				if(String.IsNullOrWhiteSpace(command.PluginId) || String.IsNullOrWhiteSpace(command.MemberName))
					throw new ArgumentException("METHODS INVOKE syntax: COMMAND: METHODS INVOKE <plugin> <method> <argsJson>");

				Boolean invokeConfirmed = await this.RequestConfirmationAsync($"METHODS INVOKE {command.PluginId} {command.MemberName} {command.Arguments}");
				return invokeConfirmed
					? this._methodsHelper.InvokePluginMethodPlaceholder(command.PluginId, command.MemberName, command.Arguments)
					: throw new OperationCanceledException();
			default:
				throw new ArgumentException("Supported methods commands: METHODS LIST <plugin>, METHODS INVOKE <plugin> <method> <argsJson>");
			}
		}
	}
}