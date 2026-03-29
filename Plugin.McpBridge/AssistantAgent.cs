using System.Diagnostics;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace Plugin.McpBridge
{
	/// <summary>Manages the Semantic Kernel instance and drives the multi-turn agent loop.</summary>
	internal sealed class AssistantAgent
	{
		private readonly TraceSource _trace;
		private readonly McpBridge _mcpBridge;
		private readonly PluginSettingsHelper _settingsHelper;
		private Kernel? _kernel;

		public AssistantAgent(TraceSource trace, McpBridge mcpBridge, PluginSettingsHelper settingsHelper)
		{
			this._trace = trace ?? throw new ArgumentNullException(nameof(trace));
			this._mcpBridge = mcpBridge ?? throw new ArgumentNullException(nameof(mcpBridge));
			this._settingsHelper = settingsHelper ?? throw new ArgumentNullException(nameof(settingsHelper));
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
			System.Net.Http.HttpClient httpClient = new System.Net.Http.HttpClient { Timeout = settings.ConnectionTimeout };
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
						orgId: settings.OrganizationId,
						serviceId: null,
						httpClient: httpClient);
				else
					kernelBuilder.AddOpenAIChatCompletion(
						modelId: settings.ModelId,
						endpoint: new Uri(settings.ModelEndpointUrl),
						apiKey: settings.ApiKey,
						orgId: settings.OrganizationId,
						serviceId: null,
						httpClient: httpClient);
				break;
			}

			this._kernel = kernelBuilder.Build();
		}

		public IEnumerable<String> InvokeMessage(String message, Settings settings)
		{
			if(String.IsNullOrWhiteSpace(message))
				return new String[] { "Message is empty." };

			ChatHistory chatHistory = this.CreateChatHistory(message, settings);
			Int32 loopCap = Math.Max(settings.AgentLoopCap, 1);

			for(Int32 loopIndex = 0; loopIndex < loopCap; loopIndex++)
			{
				String aiResponse = this.GetAssistantResponse(chatHistory, settings);
				chatHistory.AddAssistantMessage(aiResponse);

				String commandResult;
				if(!this.TryHandleSystemCommand(aiResponse, out commandResult))
					return new String[] { aiResponse };

				chatHistory.AddSystemMessage(this.BuildCommandResultPrompt(commandResult, loopIndex + 1, loopCap));
			}

			return new String[] { "Assistant reached the configured agent loop cap before returning a final user response." };
		}

		private ChatHistory CreateChatHistory(String message, Settings settings)
		{
			ChatHistory chatHistory = new ChatHistory();
			String? systemPrompt = settings.AssistantSystemPrompt;
			if(!String.IsNullOrWhiteSpace(systemPrompt))
				chatHistory.AddSystemMessage(systemPrompt!);

			chatHistory.AddUserMessage(this.BuildAiPrompt(message));
			return chatHistory;
		}

		private String GetAssistantResponse(ChatHistory chatHistory, Settings settings)
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
				ChatMessageContent response = chatCompletionService.GetChatMessageContentAsync(chatHistory, executionSettings, this._kernel, CancellationToken.None).GetAwaiter().GetResult();
				return String.IsNullOrWhiteSpace(response.Content) ? "Model returned an empty response." : response.Content ?? String.Empty;
			} catch(HttpOperationException exc)
			{
				this._trace.TraceData(TraceEventType.Error, 0, exc);
				return $"AI request failed with status code {exc.StatusCode}: {exc.Message}";
			}
		}

		private String BuildAiPrompt(String userMessage)
		{
			String pluginInventory = this._mcpBridge.ListLoadedPluginsFromHost();
			String mcpTools = this._mcpBridge.ListLoadedToolsFromMcpClient();
			return $"User message: {userMessage}{Environment.NewLine}{Environment.NewLine}Loaded SAL plugins:{Environment.NewLine}{pluginInventory}{Environment.NewLine}{Environment.NewLine}MCP tools discovered by MCP client:{Environment.NewLine}{mcpTools}{Environment.NewLine}{Environment.NewLine}Supported command payloads:{Environment.NewLine}COMMAND: SETTINGS LIST <plugin>{Environment.NewLine}COMMAND: SETTINGS GET <plugin> <setting>{Environment.NewLine}COMMAND: SETTINGS SET <plugin> <setting>=<value>{Environment.NewLine}{Environment.NewLine}If automation command is required, return a supported payload prefixed with COMMAND:. Otherwise return a normal response for the user.";
		}

		private String BuildCommandResultPrompt(String commandResult, Int32 loopIndex, Int32 loopCap)
			=> $"System command result {loopIndex}/{loopCap}:{Environment.NewLine}{commandResult}{Environment.NewLine}{Environment.NewLine}If additional automation is required, return another payload prefixed with COMMAND:. Otherwise return the final user-facing response only.";

		private Boolean TryHandleSystemCommand(String aiResponse, out String commandResult)
		{
			if(!aiResponse.StartsWith("COMMAND:", StringComparison.OrdinalIgnoreCase))
			{
				commandResult = String.Empty;
				return false;
			}

			String commandPayload = aiResponse.Substring("COMMAND:".Length).Trim();
			if(this.TryHandleSettingsCommand(commandPayload, out commandResult))
				return true;

			commandResult = String.IsNullOrWhiteSpace(commandPayload)
				? "Command payload is empty."
				: $"Command interception placeholder: {commandPayload}";

			return true;
		}

		private Boolean TryHandleSettingsCommand(String commandPayload, out String commandResult)
		{
			commandResult = String.Empty;
			if(!commandPayload.StartsWith("SETTINGS ", StringComparison.OrdinalIgnoreCase))
				return false;

			String settingsPayload = commandPayload.Substring("SETTINGS ".Length).Trim();
			if(settingsPayload.StartsWith("LIST ", StringComparison.OrdinalIgnoreCase))
			{
				String pluginId = settingsPayload.Substring("LIST ".Length).Trim();
				commandResult = this._settingsHelper.ListPluginSettings(pluginId);
				return true;
			}

			if(settingsPayload.StartsWith("GET ", StringComparison.OrdinalIgnoreCase))
			{
				String readPayload = settingsPayload.Substring("GET ".Length).Trim();
				Int32 separatorIndex = readPayload.IndexOf(' ');
				if(separatorIndex <= 0 || separatorIndex >= readPayload.Length - 1)
				{
					commandResult = "SETTINGS GET syntax: COMMAND: SETTINGS GET <plugin> <setting>";
					return true;
				}

				String pluginId = readPayload.Substring(0, separatorIndex).Trim();
				String settingName = readPayload.Substring(separatorIndex + 1).Trim();
				commandResult = this._settingsHelper.ReadPluginSetting(pluginId, settingName);
				return true;
			}

			if(settingsPayload.StartsWith("SET ", StringComparison.OrdinalIgnoreCase))
			{
				String writePayload = settingsPayload.Substring("SET ".Length).Trim();
				Int32 separatorIndex = writePayload.IndexOf(' ');
				Int32 equalsIndex = writePayload.IndexOf('=');
				if(separatorIndex <= 0 || equalsIndex <= separatorIndex + 1 || equalsIndex >= writePayload.Length)
				{
					commandResult = "SETTINGS SET syntax: COMMAND: SETTINGS SET <plugin> <setting>=<value>";
					return true;
				}

				String pluginId = writePayload.Substring(0, separatorIndex).Trim();
				String settingName = writePayload.Substring(separatorIndex + 1, equalsIndex - separatorIndex - 1).Trim();
				String settingValue = writePayload.Substring(equalsIndex + 1).Trim();
				commandResult = this._settingsHelper.UpdatePluginSetting(pluginId, settingName, settingValue);
				return true;
			}

			commandResult = "Supported settings commands: SETTINGS LIST <plugin>, SETTINGS GET <plugin> <setting>, SETTINGS SET <plugin> <setting>=<value>";
			return true;
		}
	}
}
