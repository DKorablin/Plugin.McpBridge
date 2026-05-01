using System.ClientModel;
using System.ClientModel.Primitives;
using System.Diagnostics;
using System.Text;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using Plugin.McpBridge.Data;
using Plugin.McpBridge.Events;
using Plugin.McpBridge.Tools;
using SAL.Flatbed;

namespace Plugin.McpBridge
{
	/// <summary>Manages the MAF AIAgent instance and drives the multi-turn agent loop.</summary>
	internal sealed class AssistantAgent
	{
		private readonly TraceSource _trace;
		private readonly IHost _host;
		private readonly ToolsFactory _toolsFactory;
		private readonly Func<AiProviderDto, HttpClient, IChatClient> _chatClientFactory;
		private ChatClientAgent? _agent;
		private AgentSession? _session;

		public event EventHandler<AgentResponseEventArgs>? AiResponseReceived;
		public event EventHandler<AgentConfirmationEventArgs>? ConfirmationRequired;

		public AssistantAgent(
			TraceSource trace,
			IHost host,
			ToolsFactory toolsFactory,
			Func<AiProviderDto, HttpClient, IChatClient>? chatClientFactory = null)
		{
			this._trace = trace ?? throw new ArgumentNullException(nameof(trace));
			this._host = host ?? throw new ArgumentNullException(nameof(host));
			this._toolsFactory = toolsFactory ?? throw new ArgumentNullException(nameof(toolsFactory));
			this._chatClientFactory = chatClientFactory ?? this.BuildChatClient;
		}

		public void Initialize(Settings settings, AiProviderDto provider)
		{
			_ = settings ?? throw new ArgumentNullException(nameof(settings));
			_ = provider ?? throw new ArgumentNullException(nameof(provider));

			this._session = null;

			Boolean requiresApiKey = provider.ProviderType != AiProviderType.LocalOpenAICompatible
				&& provider.ProviderType != AiProviderType.Stub;
			if(requiresApiKey && String.IsNullOrWhiteSpace(provider.ApiKey))
			{
				this._agent = null;
				return;
			}

			HttpClient httpClient = new HttpClient { Timeout = settings.ConnectionTimeout };
			IChatClient chatClient = this._chatClientFactory(provider, httpClient);

			IChatClient configuredClient = new ChatClientBuilder(chatClient)
				.ConfigureOptions(options =>
				{
					if(settings.MaxTokens.HasValue)
						options.MaxOutputTokens = settings.MaxTokens.Value;
					if(settings.Temperature.HasValue)
						options.Temperature = (Single)settings.Temperature.Value;
					if(settings.ReasoningOutput != ReasoningOutput.None)
					{
						options.Reasoning = new ReasoningOptions
						{
							Output = settings.ReasoningOutput,
							Effort = settings.ReasoningEffort
						};
					}
				})
				.Build();

			List<AITool> tools = this._toolsFactory.CreateTools(settings.ToolsPermission, (Object? s, AgentConfirmationEventArgs e) => this.OnConfirmationRequired(e)).ToList();
			String instructions = this.BuildSystemInstructions(settings, tools);
			this._agent = configuredClient.AsAIAgent(
				instructions: instructions,
				tools: tools);
			this._trace.TraceEvent(TraceEventType.Verbose, 0, $"Initialized AssistantAgent with instructions '{instructions}'.");
		}

		public async Task InvokeMessageAsync(String message, DataContent[]? images = null, CancellationToken cancellationToken = default)
		{
			if(String.IsNullOrWhiteSpace(message))
			{
				this.OnAiResponseReceived(new AgentResponseEventArgs("Message is empty.", true));
				return;
			}

			this._trace.TraceEvent(TraceEventType.Verbose, 0, "< " + message);

			if(this._agent == null)
			{
				this.OnAiResponseReceived(new AgentResponseEventArgs("AI is not configured. Add LLM configuration options in plugin settings.", true));
				return;
			}

			if(this._session == null)
				this._session = await this._agent.CreateSessionAsync(cancellationToken);

		try
		{
			AgentResponse response = await this._agent.RunAsync(AssistantAgent.BuildUserMessage(message, images), this._session, null, cancellationToken);
			this.HandleResponse(response);

			/*IAsyncEnumerable <AgentResponseUpdate> stream = this._agent.RunStreamingAsync(AssistantAgent.BuildUserMessage(message, images), this._session, null, cancellationToken);
			await this.HandleStreamingResponseAsync(stream, cancellationToken);*/
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
			String aiResponse = response.Text;
			this._trace.TraceEvent(TraceEventType.Verbose, 0, "> " + aiResponse);
			if(response.Usage != null)
				this._trace.TraceEvent(TraceEventType.Verbose, 0, $"Tokens: {String.Join(Environment.NewLine, Utils.ParseTokenUsageCount(response.Usage))}");

			this.OnAiResponseReceived(new AgentResponseEventArgs(aiResponse, true));
		}

		private async Task HandleStreamingResponseAsync(IAsyncEnumerable<AgentResponseUpdate> stream, CancellationToken cancellationToken)
		{
			StringBuilder textBuilder = new StringBuilder();
			Boolean hasReasoning = false;
			UsageDetails? usage = null;

			await foreach(AgentResponseUpdate update in stream.WithCancellation(cancellationToken))
			{
				if(update.Contents == null)
					continue;
				foreach(AIContent content in update.Contents)
				{
					if(content is TextReasoningContent reasoningContent && !String.IsNullOrEmpty(reasoningContent.Text))
					{
						if(!hasReasoning)
						{
							hasReasoning = true;
							this.OnAiResponseReceived(new AgentResponseEventArgs("> *Thinking...*\n\n", false));
						}
						this.OnAiResponseReceived(new AgentResponseEventArgs(reasoningContent.Text, false));
					}
					else if(content is TextContent textContent && !String.IsNullOrEmpty(textContent.Text))
						textBuilder.Append(textContent.Text);
					else if(content is UsageContent usageContent)
						usage = usageContent.Details;
				}
			}

			String aiResponse = textBuilder.ToString();
			this._trace.TraceEvent(TraceEventType.Verbose, 0, "> " + aiResponse);
			if(usage != null)
				this._trace.TraceEvent(TraceEventType.Verbose, 0, $"Tokens: {String.Join(Environment.NewLine, Utils.ParseTokenUsageCount(usage))}");

			if(hasReasoning)
				this.OnAiResponseReceived(new AgentResponseEventArgs("\n\n---\n\n", false));
			this.OnAiResponseReceived(new AgentResponseEventArgs(aiResponse, true));
		}

		private String BuildSystemInstructions(Settings settings, IReadOnlyList<AITool> tools)
		{
			StringBuilder sb = new StringBuilder(settings.AssistantSystemPrompt);

			String pluginInventory = this.ListPluginInventory(settings.PluginsPermission);
			sb.AppendLine();
			sb.AppendLine();
			if(pluginInventory.Length > 0)
			{
				sb.AppendLine("Loaded SAL plugins:");
				sb.AppendLine(pluginInventory);
			}else
				sb.AppendLine("No SAL plugins are available.");

			if(tools.Count > 0)
			{
				sb.AppendLine();
				sb.AppendLine("Available AI tools:");
				foreach(AIFunction tool in tools.OfType<AIFunction>())
					sb.AppendLine($"- {tool.Name} : {tool.Description}");
			}

			return sb.ToString().TrimEnd();
		}

		private String ListPluginInventory(String[]? disallowedPlugins)
		{
			StringBuilder pluginsText = new StringBuilder();
			Boolean allAllowed = disallowedPlugins == null || disallowedPlugins.Length == 0;
			foreach(IPluginDescription pluginDescription in this._host.Plugins)
			{
				if(!allAllowed && Array.Exists(disallowedPlugins!, p => p == pluginDescription.ID))
					continue;

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

		private static ChatMessage BuildUserMessage(String text, DataContent[]? images = null)
		{
			if(images == null || images.Length == 0)
				return new ChatMessage(ChatRole.User, text);

			List<AIContent> contents = new List<AIContent> { new TextContent(text) };
			foreach(DataContent image in images)
				contents.Add(image);
			return new ChatMessage(ChatRole.User, contents);
		}

		private IChatClient BuildChatClient(AiProviderDto provider, HttpClient httpClient)
		{
			if(provider.ProviderType == AiProviderType.Stub)
				return new StubChatClient();

			HttpClientPipelineTransport transport = new HttpClientPipelineTransport(httpClient);
			switch(provider.ProviderType)
			{
			case AiProviderType.AzureOpenAI:
				return new AzureOpenAIClient(
					new Uri(provider.ModelEndpointUrl!),
					new ApiKeyCredential(provider.ApiKey!),
					new AzureOpenAIClientOptions { Transport = transport })
					.GetChatClient(provider.DeploymentName ?? provider.ModelId)
				.AsIChatClient();
			default:
				OpenAIClientOptions clientOptions = new OpenAIClientOptions
				{
					Transport = transport
				};
				if(provider.ModelEndpointUrl != null)
					clientOptions.Endpoint = new Uri(provider.ModelEndpointUrl);

				return new OpenAIClient(new ApiKeyCredential(provider.ApiKey ?? "local-no-key"), clientOptions)
					.GetChatClient(provider.ModelId)
					.AsIChatClient();
			}
		}
	}
}