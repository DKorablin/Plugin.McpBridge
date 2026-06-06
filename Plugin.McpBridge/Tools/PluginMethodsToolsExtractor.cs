using System.Reflection;
using Plugin.McpBridge.Data;
using SAL.Flatbed;

namespace Plugin.McpBridge.Tools;

internal class PluginMethodsToolsExtractor : ToolsDiscoveryBase
{
	private readonly IHost _host;
	private readonly Settings _settings;
	private readonly AiAgentDto _agent;
	private readonly XmlReflectionReader _xmlReader;

	public PluginMethodsToolsExtractor(IHost host, Settings settings, AiAgentDto agent)
	{
		this._host = host ?? throw new ArgumentNullException(nameof(host));
		this._settings = settings ?? throw new ArgumentNullException(nameof(settings));
		this._agent = agent ?? throw new ArgumentNullException(nameof(agent));
		this._xmlReader = new XmlReflectionReader();
	}

	public override IEnumerable<ToolMethodDto> GetTools()
	{
		var allowedPlugins = this._agent.PluginsPermission;
		Boolean allAllowed = allowedPlugins == null;
		var allowedSet = allAllowed ? null : new HashSet<String>(allowedPlugins!);

		foreach(var pluginDescription in this._host.Plugins)
			if(allAllowed || allowedSet!.Contains(pluginDescription.ID))
			{
				IEnumerable<IPluginMemberInfo> callableMembers = PluginMethodsToolsExtractor.GetCallableMembers(pluginDescription);
				foreach(var member in callableMembers)
				{
					if(member.MemberType != MemberTypes.Method)
						continue;

					var method = (IPluginMethodInfo)member;
					var docs = this._xmlReader.FindDocumentation(pluginDescription, member);

					// Allowed tool name: ^[a-zA-Z0-9_]+$
					var toolName = $"{pluginDescription.Name.Replace("-", String.Empty)}.{method.Name}";
					var description = docs?.Summary ?? String.Empty;

					PluginMethodAIFunction function = new PluginMethodAIFunction(
						toolName,
						description,
						method,
						docs?.Parameters);

					yield return new ToolMethodDto(true, function.Name, description, function);
				}
			}
	}

	public static Boolean HasCallableMembers(IPluginDescription pluginDescription)
		=> GetCallableMembers(pluginDescription).Any();

	public static IEnumerable<IPluginMemberInfo> GetCallableMembers(IPluginDescription pluginDescription)
	{
		if(pluginDescription.Type != null && pluginDescription.Type.Members != null)
			foreach(IPluginMemberInfo pluginMember in pluginDescription.Type.Members)
				if(pluginMember != null && pluginMember.MemberType == System.Reflection.MemberTypes.Method)
					yield return pluginMember;
	}
}