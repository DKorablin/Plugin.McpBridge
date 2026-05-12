using System.ComponentModel;
using System.Reflection;

namespace Plugin.McpBridge.Tools;

internal abstract class ToolsDiscoveryBase
{
	public virtual IEnumerable<(Object Target, ToolAttribute Tool, MethodInfo Method, String Description)> GetTools()
	{
		foreach(MethodInfo method in this.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public))
		{
			var attr = method.GetCustomAttribute<ToolAttribute>();

			if(attr != null)
			{
				String description = method.GetCustomAttribute<DescriptionAttribute>()?.Description ?? String.Empty;

				yield return (this, attr, method, description);
			}
		}
	}
}