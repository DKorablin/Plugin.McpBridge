using System.Reflection;
using Plugin.McpBridge.Data;
using SAL.Flatbed;

namespace Plugin.McpBridge.Tools;

internal class PluginMethodsToolsExtractor : ToolsDiscoveryBase
{
	private readonly IHost _host;
	private readonly Settings _settings;
	private readonly XmlReflectionReader _xmlReader;

	public PluginMethodsToolsExtractor(IHost host, Settings settings)
	{
		this._host = host ?? throw new ArgumentNullException(nameof(host));
		this._settings = settings ?? throw new ArgumentNullException(nameof(settings));
		this._xmlReader = new XmlReflectionReader();
	}

	public override IEnumerable<ToolMethodDto> GetTools()
	{
		var disallowedPlugins = this._settings.PluginsPermission;
		Boolean allAllowed = disallowedPlugins == null || disallowedPlugins.Length == 0;
		foreach(var pluginDescription in this._host.Plugins)
		{
			if(!allAllowed && Array.Exists(disallowedPlugins!, p => p == pluginDescription.ID))
				continue;

			IEnumerable<IPluginMemberInfo> callableMembers = PluginMethodsTools.GetCallableMembers(pluginDescription);
			foreach(var member in callableMembers)
			{
				if(member.MemberType != MemberTypes.Method)
					continue;

				IPluginMethodInfo method = (IPluginMethodInfo)member;
				XmlReflectionDto? docs = this._xmlReader.FindDocumentation(pluginDescription, member);
				String description = docs?.Summary ?? String.Empty;

				PluginMethodAIFunction function = new PluginMethodAIFunction(
					$"{pluginDescription.ID}_{method.Name}",
					description,
					method,
					docs?.Parameters);

				yield return new ToolMethodDto(true, function.Name, description, function);
			}
		}
	}
}