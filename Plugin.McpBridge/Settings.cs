using System.Text;
using Plugin.McpBridge.Tools;
using SAL.Flatbed;

namespace Plugin.McpBridge
{
	/// <summary>Configuration settings for the MCP Bridge plugin.</summary>
	public class Settings : SettingsBase
	{
		private static class Defaults
		{
			public const String AgentStateFileName = "agentState.json";
		}
		internal Plugin Plugin { get; }

		public Settings() : this(null!) { }

		internal Settings(Plugin plugin)
			=> this.Plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));

		internal void SaveAgentSession(String? sessionJson)
		{
			if(String.IsNullOrWhiteSpace(sessionJson))
				this.Plugin.Host.Plugins.Settings(this.Plugin).RemoveAssemblyBlob(Defaults.AgentStateFileName);
			else
				using(MemoryStream ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(sessionJson)))
					this.Plugin.Host.Plugins.Settings(this.Plugin).SaveAssemblyBlob(Defaults.AgentStateFileName, ms);
		}

		internal String? LoadAgentSession()
		{
			using(Stream stream = this.Plugin.Host.Plugins.Settings(this.Plugin).LoadAssemblyBlob(Defaults.AgentStateFileName))
				return stream == null
					? null
					: new StreamReader(stream).ReadToEnd();
		}

		public static String BuildSystemInstructions(SettingsBase settings, Data.AiAgentDto agent, IHost host)
		{
			String pluginInventory = ListPluginInventory(agent, host);
			return BuildSystemInstructions(agent.AssistantSystemPrompt, pluginInventory);

			String BuildSystemInstructions(String? systemPrompt, String pluginInventory)
			{
				StringBuilder sb = new StringBuilder(systemPrompt);

				sb.AppendLine();
				sb.AppendLine();
				if(pluginInventory.Length > 0)
				{
					sb.AppendLine("Loaded SAL plugins:");
					sb.AppendLine(pluginInventory);
				} else
					sb.AppendLine("No SAL plugins are available.");

				return sb.ToString().TrimEnd();
			}

			String ListPluginInventory(Data.AiAgentDto agent, IHost host)
			{
				var disallowedPlugins = agent.PluginsPermission;
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
		}
	}
}