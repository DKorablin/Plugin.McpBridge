using System.Reflection;
using Plugin.McpBridge.Data;
using SAL.Flatbed;

namespace Plugin.McpBridge.Tools;

internal class PluginMethodsToolsExtractor : ToolsDiscoveryBase
{
	private readonly IHost _host;
	private readonly XmlReflectionReader _xmlReader;

	public PluginMethodsToolsExtractor(IHost host)
	{
		this._host = host ?? throw new ArgumentNullException(nameof(host));
		this._xmlReader = new XmlReflectionReader();
	}

	public override IEnumerable<ToolMethodDto> GetTools()
	{
		foreach(var pluginDescription in this._host.Plugins)
		{
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