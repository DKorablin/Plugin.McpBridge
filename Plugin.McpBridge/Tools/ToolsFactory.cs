using Microsoft.Extensions.AI;
using Plugin.McpBridge.Data;
using SAL.Flatbed;
using SAL.Windows;

namespace Plugin.McpBridge.Tools;

/// <summary>Creates <see cref="ToolFacade"/> instances from methods decorated with <see cref="ToolAttribute"/> using reflection.</summary>
internal sealed class ToolsFactory
{
	private readonly ToolsDiscoveryBase[] _targets;
	private readonly Settings? _settings;

	public ToolsFactory(IHost host, Settings settings)
	{
		List<ToolsDiscoveryBase> tools = new List<ToolsDiscoveryBase>()
		{
			new PluginSettingsTools(host),
			//new PluginMethodsTools(host),
			new PluginMethodsToolsExtractor(host, settings),
			new ShellTools(),
		};

		if(host is IHostWindows hostWindows)
			tools.Add(new WindowsTools(hostWindows));

		this._targets = tools.ToArray();
		this._settings = settings;
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

	public IEnumerable<AIFunction> CreateTools(ITraceSource trace)
		=> this.CreateTools(trace, this._settings?.ToolsPermission);

	public IEnumerable<AIFunction> CreateTools(ITraceSource trace, String[]? exclusionList)
	{
		_ = trace ?? throw new ArgumentNullException(nameof(trace));

		Boolean allAllowed = exclusionList == null || exclusionList.Length == 0;
		foreach(var method in this.GetTools())
		{
			if(!allAllowed && Array.Exists(exclusionList!, p => p == method.Name))
				continue;

			var tool = new ToolFacade(trace, method.Function, method.ConfirmationRequired);
			yield return method.ConfirmationRequired
				? new ApprovalRequiredAIFunction(tool)
				: tool;
		}
	}
}