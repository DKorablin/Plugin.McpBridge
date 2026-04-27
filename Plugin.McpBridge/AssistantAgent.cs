using System.ClientModel;
using System.ClientModel.Primitives;
using System.Diagnostics;
using System.Text;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using Plugin.McpBridge.Helpers;
using Plugin.McpBridge.Tools;
using SAL.Flatbed;

namespace Plugin.McpBridge
{
	/// <summary>Manages the MAF AIAgent instance and drives the multi-turn agent loop.</summary>
	internal sealed class AssistantAgent
	{
		private readonly TraceSource _trace;
		private readonly IHost _host;
		private readonly ToolFactory _toolsFactory;
		private readonly Func<Settings, HttpClient, IChatClient> _chatClientFactory;
		private ChatClientAgent? _agent;
		private AgentSession? _session;

		public event EventHandler<AgentResponseEventArgs>? AiResponseReceived;
		public event EventHandler<AgentConfirmationEventArgs>? ConfirmationRequired;

		public AssistantAgent(
			TraceSource trace,
			IHost host,
			ToolFactory toolsFactory,
			Func<Settings, HttpClient, IChatClient>? chatClientFactory = null)
		{
			this._trace = trace ?? throw new ArgumentNullException(nameof(trace));
			this._host = host ?? throw new ArgumentNullException(nameof(host));
			this._toolsFactory = toolsFactory ?? throw new ArgumentNullException(nameof(toolsFactory));
			this._chatClientFactory = chatClientFactory ?? this.BuildChatClient;
		}

		public void Initialize(Settings settings)
		{
			this._session = null;

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

			ToolFactory toolFactory = this._toolsFactory;
			List<AITool> tools = toolFactory.CreateTools(settings.ToolsPermission, (Object? s, AgentConfirmationEventArgs e) => this.OnConfirmationRequired(e)).ToList();
			String instructions = this.BuildSystemInstructions(settings, tools);
			this._agent = configuredClient.AsAIAgent(
				instructions: instructions,
				tools: tools);
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
				pluginsText.Append(PluginSettingsTools.HasPluginSettings(pluginDescription) ? "yes" : "no");
				pluginsText.Append(" | Members: ");
				pluginsText.Append(PluginMethodsTools.HasCallableMembers(pluginDescription) ? "yes" : "no");
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
	}
}