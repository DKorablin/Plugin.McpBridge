using Microsoft.Extensions.AI;
using Plugin.McpBridge.Data;
using SAL.Flatbed;
using SAL.Windows;

namespace Plugin.McpBridge.Tools;

/// <summary>Creates <see cref="ToolFacade"/> instances from methods decorated with <see cref="ToolAttribute"/> using reflection.</summary>
internal sealed class ToolsFactory
{
	private readonly ToolsDiscoveryBase[] _targets;

	public ToolsFactory(IHost host)
	{
		List<ToolsDiscoveryBase> tools = new List<ToolsDiscoveryBase>()
		{
			new PluginSettingsTools(host),
			//new PluginMethodsTools(host),
			new PluginMethodsToolsExtractor(host),
			new ShellTools(),
		};

		if(host is IHostWindows hostWindows)
			tools.Add(new WindowsTools(hostWindows));

		this._targets = tools.ToArray();
	}

	public ToolsFactory(params ToolsDiscoveryBase[] toolsHosts)
	{
		if(toolsHosts == null || toolsHosts.Length == 0)
			throw new ArgumentException("At least one tools host must be provided.", nameof(toolsHosts));

		this._targets = toolsHosts;
	}

	public IEnumerable<ToolMethodDto> GetTools()
	{
		foreach(ToolsDiscoveryBase target in this._targets)
			foreach(var tool in target.GetTools())
				yield return tool;
	}

	public IEnumerable<AITool> CreateTools(ITraceSource trace, String[]? permissions)
	{
		_ = trace ?? throw new ArgumentNullException(nameof(trace));

		Boolean allAllowed = permissions == null || permissions.Length == 0;
		foreach(var method in this.GetTools())
		{
			if(!allAllowed && Array.Exists(permissions!, p => p == method.Name))
				continue;

			var tool = new ToolFacade(trace, method.Function, method.ConfirmationRequired);
			yield return method.ConfirmationRequired
				? new ApprovalRequiredAIFunction(tool)
				: tool;
		}
	}
}