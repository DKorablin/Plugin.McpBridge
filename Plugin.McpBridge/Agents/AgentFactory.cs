using System.ClientModel;
using System.ClientModel.Primitives;
using System.Text;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using OpenAI;
using Plugin.McpBridge.Data;
using Plugin.McpBridge.Tools;
using SAL.Flatbed;

namespace Plugin.McpBridge.Agents;

/// <summary>Shared factory methods for building AI agent components, usable by both the WinForms chat path and DevUI.</summary>
internal static class AgentFactory
{
	/// <summary>Creates a raw <see cref="IChatClient"/> for the given provider using the supplied <see cref="HttpClient"/>.</summary>
	internal static IChatClient CreateChatClient(AiProviderDto provider, HttpClient httpClient)
	{
#if DEBUG
		if(provider.ProviderType == AiProviderType.Stub)
			return new Tests.StubChatClient();
#endif
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
			OpenAIClientOptions clientOptions = new OpenAIClientOptions { Transport = transport };
			if(provider.ModelEndpointUrl != null)
				clientOptions.Endpoint = new Uri(provider.ModelEndpointUrl);

			return new OpenAIClient(new ApiKeyCredential(provider.ApiKey ?? "local-no-key"), clientOptions)
				.GetChatClient(provider.ModelId)
				.AsIChatClient();
		}
	}

	/// <summary>Wraps a <see cref="IChatClient"/> with token, temperature, and reasoning options from <paramref name="settings"/> and <paramref name="provider"/>.</summary>
	internal static IChatClient ConfigureOptions(IChatClient chatClient, Int32? maxTokens, AiProviderDto provider)
		=> new ChatClientBuilder(chatClient)
			.ConfigureOptions(options =>
			{
				if(maxTokens.HasValue)
					options.MaxOutputTokens = maxTokens.Value;
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
