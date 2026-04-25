using System.ClientModel;
using System.ClientModel.Primitives;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using Plugin.McpBridge.Helpers;
using SAL.Flatbed;

namespace Plugin.McpBridge
{
	/// <summary>Manages the MAF AIAgent instance and drives the multi-turn agent loop.</summary>
	internal sealed class AssistantAgent
	{
		private readonly TraceSource _trace;
		private readonly IHost _host;
		private readonly PluginSettingsHelper _settingsHelper;
		private readonly PluginMethodsHelper _methodsHelper;
		private readonly Func<Settings, HttpClient, IChatClient> _chatClientFactory;
		private readonly TimeProvider _timeProvider;
		private ChatClientAgent? _agent;
		private AgentSession? _session;
		private Int32 _maxToolResultLength;


		public event EventHandler<AgentResponseEventArgs>? AiResponseReceived;
		public event EventHandler<AgentConfirmationEventArgs>? ConfirmationRequired;

		public AssistantAgent(
			TraceSource trace,
			IHost host,
			PluginSettingsHelper settingsHelper,
			PluginMethodsHelper methodsHelper,
			Func<Settings, HttpClient, IChatClient>? chatClientFactory = null,
			TimeProvider? timeProvider = null)
		{
			this._trace = trace ?? throw new ArgumentNullException(nameof(trace));
			this._host = host ?? throw new ArgumentNullException(nameof(host));
			this._settingsHelper = settingsHelper ?? throw new ArgumentNullException(nameof(settingsHelper));
			this._methodsHelper = methodsHelper ?? throw new ArgumentNullException(nameof(methodsHelper));
			this._chatClientFactory = chatClientFactory ?? this.BuildChatClient;
			this._timeProvider = timeProvider ?? TimeProvider.System;
		}

		public void Initialize(Settings settings)
		{
			this._session = null;
			this._maxToolResultLength = settings.MaxToolResultLength;

			Boolean requiresApiKey = settings.ProviderType != AiProviderType.LocalOpenAICompatible
				&& settings.ProviderType != AiProviderType.Stub;
			if(requiresApiKey && String.IsNullOrWhiteSpace(settings.ApiKey))
			{
				this._agent = null;
				return;
			}

			HttpClient httpClient = new HttpClient { Timeout = settings.ConnectionTimeout };
			IChatClient chatClient = this._chatClientFactory(settings, httpClient);

			IChatClient configuredClient = new ChatClientBuilder(chatClient)
				.ConfigureOptions(options =>
				{
					if(settings.MaxTokens.HasValue)
						options.MaxOutputTokens = settings.MaxTokens.Value;
					if(settings.Temperature.HasValue)
						options.Temperature = (Single)settings.Temperature.Value;
					if(settings.ReasoningOutput.HasValue || settings.ReasoningEffort.HasValue)
					{
						options.Reasoning = new ReasoningOptions
						{
							Output = settings.ReasoningOutput ?? ReasoningOutput.None,
							Effort = settings.ReasoningEffort ?? ReasoningEffort.Medium
						};
					}
				})
				.Build();

			List<AITool> tools = GetTools().ToList();
			String instructions = this.BuildSystemInstructions(settings, tools);
			this._agent = configuredClient.AsAIAgent(
				instructions: instructions,
				tools: tools);

			IEnumerable<AITool> GetTools()
			{
				var permissions = settings.ToolsPermission;
				if(permissions.HasFlag(Settings.Tools.SystemInformation))
					yield return AIFunctionFactory.Create(this.SystemInformation, nameof(this.SystemInformation), "Get the current host environment system information including OS version, DateTime format and UTC");

				if(permissions.HasFlag(Settings.Tools.SettingsList))
					yield return AIFunctionFactory.Create(this.SettingsList, nameof(this.SettingsList), "List all available settings for a plugin");

				if(permissions.HasFlag(Settings.Tools.SettingsGet))
					yield return AIFunctionFactory.Create(this.SettingsGet, nameof(this.SettingsGet), "Get the current value of a specific plugin setting");

				if(permissions.HasFlag(Settings.Tools.SettingsSet))
					yield return AIFunctionFactory.Create(this.SettingsSet, nameof(this.SettingsSet), "Update a plugin setting value; requires user confirmation");

				if(permissions.HasFlag(Settings.Tools.MethodsList))
					yield return AIFunctionFactory.Create(this.MethodsList, nameof(this.MethodsList), "List all available methods for a plugin");

				if(permissions.HasFlag(Settings.Tools.MethodsInvoke))
					yield return AIFunctionFactory.Create(this.MethodsInvoke, nameof(this.MethodsInvoke), "Invoke a plugin method; requires user confirmation");
			}
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
				AgentResponse response = await this._agent.RunAsync(message, this._session, null, cancellationToken);
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

		private String BuildSystemInstructions(Settings settings, IReadOnlyList<AITool> tools)
		{
			StringBuilder sb = new StringBuilder(settings.AssistantSystemPrompt);

			String pluginInventory = this.ListPluginInventory();
			sb.AppendLine();
			sb.AppendLine();
			sb.AppendLine("Loaded SAL plugins:");
			sb.AppendLine(pluginInventory);

			if(tools.Count > 0)
			{
				sb.AppendLine();
				sb.AppendLine("Available AI tools:");
				foreach(AIFunction tool in tools.OfType<AIFunction>())
					sb.AppendLine($"- {tool.Name} : {tool.Description}");
			}

			return sb.ToString().TrimEnd();
		}

		private String ListPluginInventory()
		{
			if(this._host.Plugins.Count <= 0)
				return "No plugins loaded.";

			StringBuilder pluginsText = new StringBuilder();
			foreach(IPluginDescription pluginDescription in this._host.Plugins)
			{
				pluginsText.Append("- ");
				pluginsText.Append(pluginDescription.ID);
				pluginsText.Append(" | ");
				pluginsText.Append(pluginDescription.Name);
				pluginsText.Append(" | ");
				pluginsText.Append(pluginDescription.Version?.ToString());
				pluginsText.Append(" | Settings: ");
				pluginsText.Append(PluginSettingsHelper.HasPluginSettings(pluginDescription) ? "yes" : "no");
				pluginsText.Append(" | Members: ");
				pluginsText.Append(PluginMethodsHelper.HasCallableMembers(pluginDescription) ? "yes" : "no");
				pluginsText.AppendLine();
			}
			return pluginsText.ToString().Trim();
		}

		private IChatClient BuildChatClient(Settings settings, HttpClient httpClient)
		{
			if(settings.ProviderType == AiProviderType.Stub)
				return new StubChatClient();

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

		private async Task ConfirmOrThrowAsync(String actionDescription)
		{
			if(!await this.RequestConfirmationAsync(actionDescription))
			{
				ArgumentException exc = new ArgumentException("Operation declined by user.");
				exc.Data.Add(nameof(actionDescription), actionDescription);
				throw exc;
			}
		}

		private async Task<String> EnforceResultLengthAsync(String result)
		{
			if(result.Length <= this._maxToolResultLength)
				return result;

			Boolean confirmed = await this.RequestConfirmationAsync($"The result is {result.Length:N0} characters long, which exceeds the configured limit of {this._maxToolResultLength:N0}. Do you want to send a truncated result?");
			if(confirmed)
				return result.Substring(0, this._maxToolResultLength) + $"\n[Result truncated: {result.Length} chars total, limit is {this._maxToolResultLength}]";

			ArgumentException exc = new ArgumentException("Operation declined by user due to result length.");
			exc.Data.Add("ResultLength", result.Length);
			exc.Data.Add(nameof(this._maxToolResultLength), this._maxToolResultLength);
			throw exc;
		}

		internal async Task<String> SettingsList([Description("Plugin identifier")] String pluginId)
		{
			this._trace.TraceEvent(TraceEventType.Verbose, 0, $"[tool] SettingsList plugin={pluginId}");
			String result = this._settingsHelper.ListPluginSettings(pluginId);

			result = await this.EnforceResultLengthAsync(result);
			this._trace.TraceEvent(TraceEventType.Verbose, 0, "[tool result] " + result);
			return result;
		}

		internal async Task<String> SettingsGet(
			[Description("Plugin identifier")] String pluginId,
			[Description("Setting name")] String settingName)
		{
			this._trace.TraceEvent(TraceEventType.Verbose, 0, $"[tool] SettingsGet plugin={pluginId} setting={settingName}");
			String result = this._settingsHelper.ReadPluginSetting(pluginId, settingName);

			result = await this.EnforceResultLengthAsync(result);
			this._trace.TraceEvent(TraceEventType.Verbose, 0, "[tool result] " + result);
			return result;
		}

		internal async Task<String> SettingsSet(
			[Description("Plugin identifier")] String pluginId,
			[Description("Setting name")] String settingName,
			[Description("New value as JSON")] String valueJson)
		{
			this._trace.TraceEvent(TraceEventType.Verbose, 0, $"[tool] SettingsSet plugin={pluginId} setting={settingName} value={valueJson}");

			await this.ConfirmOrThrowAsync($"SETTINGS SET {pluginId} {settingName}={valueJson}");
			String result = this._settingsHelper.UpdatePluginSetting(pluginId, settingName, valueJson);

			result = await this.EnforceResultLengthAsync(result);
			this._trace.TraceEvent(TraceEventType.Verbose, 0, "[tool result] " + result);
			return result;
		}

		internal async Task<String> MethodsList([Description("Plugin identifier")] String pluginId)
		{
			this._trace.TraceEvent(TraceEventType.Verbose, 0, $"[tool] MethodsList plugin={pluginId}");
			String result = this._methodsHelper.ListPluginMethods(pluginId);

			result = await this.EnforceResultLengthAsync(result);
			this._trace.TraceEvent(TraceEventType.Verbose, 0, "[tool result] " + result);
			return result;
		}

		internal async Task<String> MethodsInvoke(
			[Description("Plugin identifier")] String pluginId,
			[Description("Method name")] String methodName,
			[Description("Arguments as JSON")] String argsJson)
		{
			this._trace.TraceEvent(TraceEventType.Verbose, 0, $"[tool] MethodsInvoke plugin={pluginId} method={methodName} args={argsJson}");

			await this.ConfirmOrThrowAsync($"METHODS INVOKE {pluginId} {methodName} {argsJson}");
			String result = this._methodsHelper.InvokePluginMethod(pluginId, methodName, argsJson);

			result = await this.EnforceResultLengthAsync(result);
			this._trace.TraceEvent(TraceEventType.Verbose, 0, "[tool result] " + result);
			return result;
		}

		internal async Task<String> SystemInformation()
		{
			this._trace.TraceEvent(TraceEventType.Verbose, 0, $"[tool] SystemInformation");

			DateTimeFormatInfo formatPreferences = CultureInfo.CurrentCulture.DateTimeFormat;
			String result = @$"
Short date pattern: {formatPreferences.ShortDatePattern}
Long date pattern; {formatPreferences.LongTimePattern}
Current time: {this._timeProvider.GetLocalNow()}
OS Version: {Environment.OSVersion}";

			return result;
		}
	}
}