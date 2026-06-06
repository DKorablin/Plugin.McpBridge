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
	private readonly AiAgentDto _agent;

	public ToolsFactory(IHost host, Settings settings, AiAgentDto agent)
	{
		List<ToolsDiscoveryBase> tools = new List<ToolsDiscoveryBase>()
		{
			new PluginSettingsTools(host),
			//new PluginMethodsTools(host),
			new PluginMethodsToolsExtractor(host, settings, agent),
			new ShellTools(),
		};

		if(host is IHostWindows hostWindows)
			tools.Add(new WindowsTools(hostWindows));

		this._targets = tools.ToArray();
		this._settings = settings;
		this._agent = agent;
	}

	public IEnumerable<ToolMethodDto> GetTools()
	{
		foreach(ToolsDiscoveryBase target in this._targets)
			foreach(var tool in target.GetTools())
				yield return tool;
	}

	public IEnumerable<AIFunction> CreateTools(ITraceSource trace)
		=> this.CreateTools(trace, this._agent.ToolsPermission);

	public IEnumerable<AIFunction> CreateTools(ITraceSource trace, String[]? availableTools)
	{
		_ = trace ?? throw new ArgumentNullException(nameof(trace));

		Boolean allAllowed = availableTools == null;
		var allowedSet = allAllowed ? null : new HashSet<String>(availableTools!);

		foreach(var method in this.GetTools())
			if(allAllowed || allowedSet!.Contains(method.Name))
			{
				var tool = new ToolFacade(trace, method.Function, method.ConfirmationRequired);
				yield return method.ConfirmationRequired
					? new ApprovalRequiredAIFunction(tool)
					: tool;
			}
	}
}