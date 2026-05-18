using System.ClientModel;
using System.ClientModel.Primitives;
using System.Text;
using Azure.AI.OpenAI;
using GitHub.Copilot.SDK;
using Microsoft.Extensions.AI;
using OpenAI;
using Plugin.McpBridge.Data;
using Plugin.McpBridge.Tools;
using SAL.Flatbed;

namespace Plugin.McpBridge.Agents;

/// <summary>Shared factory methods for building AI agent components, usable by both the WinForms chat path and DevUI.</summary>
internal class AgentFactory
{
	public virtual async Task<AgentHandle> CreateAgent(AiProviderDto providerSettings, HttpClient httpClient, AIFunction[] tools, String systemInstructions, CancellationToken token = default)
	{
		switch(providerSettings.ProviderType)
		{
		case AiProviderType.CoPilot:
			CoPilotProviderDto copilotSettings = (CoPilotProviderDto)providerSettings;
			CopilotClientOptions options = new CopilotClientOptions()
			{
				CliPath = copilotSettings.CoPilotPath,
				GitHubToken = copilotSettings.GitHubToken,
				//UseLoggedInUser = true,
			};
			CopilotClient copilotClient = new CopilotClient(options);

			await copilotClient.StartAsync(token);
			var sessionConfig = new SessionConfig
			{
				OnPermissionRequest = PermissionHandler.ApproveAll,
				Tools = tools,
				SystemMessage = new SystemMessageConfig()
				{
					Mode = SystemMessageMode.Append,
					Content = systemInstructions,
				},
			};

			return AgentHandle.FromCopilotClient(copilotClient.AsAIAgent(sessionConfig), copilotClient);
		default:
			IChatClient chatClient = AgentFactory.CreateChatClient(providerSettings, httpClient);
			return AgentHandle.FromChatClient(
				chatClient.AsAIAgent(
					instructions: systemInstructions,
					tools: tools,
					name: "assistant"),
				chatClient);
		}
	}

	/// <summary>Creates a raw <see cref="IChatClient"/> for the given provider using the supplied <see cref="HttpClient"/>.</summary>
	internal static IChatClient CreateChatClient(AiProviderDto providerSettings, HttpClient httpClient)
	{
		IChatClient chatClient;
		HttpClientPipelineTransport transport = new HttpClientPipelineTransport(httpClient);

		switch(providerSettings.ProviderType)
		{
		case AiProviderType.Stub:
			chatClient = new Tests.StubChatClient();
			break;
		case AiProviderType.AzureOpenAI:
			var azureSettings = (AzureProviderDto)providerSettings;
			chatClient = new AzureOpenAIClient(
				new Uri(providerSettings.ModelEndpointUrl!),
				new ApiKeyCredential(azureSettings.ApiKey!),
				new AzureOpenAIClientOptions { Transport = transport, })
				.GetChatClient(azureSettings.DeploymentName)
				.AsIChatClient();
			break;
		default:
			var networkSettings = (NetworkProviderDto)providerSettings;
			OpenAIClientOptions clientOptions = new OpenAIClientOptions { Transport = transport };
			if(providerSettings.ModelEndpointUrl != null)
				clientOptions.Endpoint = new Uri(networkSettings.ModelEndpointUrl);

			chatClient = new OpenAIClient(new ApiKeyCredential(networkSettings.ApiKey ?? "local-no-key"), clientOptions)
				.GetChatClient(providerSettings.ModelId)
				.AsIChatClient();
			break;
		}
		return ConfigureOptions(chatClient, providerSettings);
	}

	/// <summary>Wraps a <see cref="IChatClient"/> with token, temperature, and reasoning options from <paramref name="settings"/> and <paramref name="provider"/>.</summary>
	private static IChatClient ConfigureOptions(IChatClient chatClient, AiProviderDto provider)
		=> new ChatClientBuilder(chatClient)
			.ConfigureOptions(options =>
			{
				if(provider.MaxTokens.HasValue)
					options.MaxOutputTokens = provider.MaxTokens.Value;
				if(provider.Temperature.HasValue)
					options.Temperature = (Single)provider.Temperature.Value;
				if(provider.ReasoningOutput.HasValue || provider.ReasoningEffort.HasValue)
					options.Reasoning = new ReasoningOptions
					{
						Output = provider.ReasoningOutput ?? ReasoningOutput.None,
						Effort = provider.ReasoningEffort ?? ReasoningEffort.Medium
					};
			})
			.Build();

	private static String? ResolveFromPath(String exeName)
	{
		String? pathVar = Environment.GetEnvironmentVariable("PATH");
		if(pathVar == null)
			return null;
		foreach(String dir in pathVar.Split(Path.PathSeparator))
		{
			String full = Path.Combine(dir.Trim(), exeName);
			if(File.Exists(full))
				return full;
		}
		return null;
	}

	internal static String BuildSystemInstructions(IHost host, Settings settings)
	{
		String pluginInventory = AgentFactory.ListPluginInventory(host, settings.PluginsPermission);
		return AgentFactory.BuildSystemInstructions(settings, pluginInventory);
	}

	/// <summary>Returns a formatted inventory of SAL plugins visible to the agent, filtered by <paramref name="disallowedPlugins"/>.</summary>
	private static String ListPluginInventory(IHost host, String[]? disallowedPlugins)
	{
		StringBuilder pluginsText = new StringBuilder();
		Boolean allAllowed = disallowedPlugins == null || disallowedPlugins.Length == 0;
		foreach(IPluginDescription pluginDescription in host.Plugins)
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

	/// <summary>Builds the system prompt from <paramref name="settings"/>, the tool list, and a pre-built plugin inventory string.</summary>
	private static String BuildSystemInstructions(Settings settings, String pluginInventory, IReadOnlyList<AITool>? tools = null)
	{
		StringBuilder sb = new StringBuilder(settings.AssistantSystemPrompt);

		sb.AppendLine();
		sb.AppendLine();
		if(pluginInventory.Length > 0)
		{
			sb.AppendLine("Loaded SAL plugins:");
			sb.AppendLine(pluginInventory);
		} else
			sb.AppendLine("No SAL plugins are available.");

		/*if(tools.Count > 0)
		{//TODO: This is redundant with the tool descriptions that DevUI will show, and may be too much info for the system prompt. Consider removing or moving to a separate "tool inventory" prompt if needed.
			sb.AppendLine();
			sb.AppendLine("Available AI tools:");
			foreach(AIFunction tool in tools.OfType<AIFunction>())
				sb.AppendLine($"- {tool.Name} : {tool.Description}");
		}*/

		return sb.ToString().TrimEnd();
	}
}
