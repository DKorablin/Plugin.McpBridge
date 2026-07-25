using Microsoft.Extensions.AI;
using Plugin.McpBridge.Core;
using Plugin.McpBridge.Data;

namespace Plugin.McpBridge.Tools;

/// <summary>Creates <see cref="ToolFacade"/> instances from methods decorated with <see cref="ToolAttribute"/> using reflection.</summary>
public class ToolsFactory
{
	private readonly ToolsDiscoveryBase[] _targets;
	private readonly AiAgentDto _agent;

	public ToolsFactory(Settings settings, AiAgentDto agent, ToolsDiscoveryBase[] targets)
	{
		this._targets = targets ?? throw new ArgumentNullException(nameof(targets));
		this._agent = agent;
	}

	public IEnumerable<ToolMethodDto> GetTools()
	{
		foreach(ToolsDiscoveryBase target in this._targets)
			foreach(var tool in target.GetTools())
				yield return tool;
	}

	public IEnumerable<AIFunction> CreateTools(IMcpTrace trace)
		=> this.CreateTools(trace, this._agent.ToolsPermission);

	public IEnumerable<AIFunction> CreateTools(IMcpTrace trace, String[]? availableTools)
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