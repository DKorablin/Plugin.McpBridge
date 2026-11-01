using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using McpBridge.Core.Data;

namespace McpBridge.Core.Tools;

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

	public IEnumerable<AIFunction> CreateTools(ILogger logger)
		=> this.CreateTools(logger, this._agent.ToolsPermission);

	public IEnumerable<AIFunction> CreateTools(ILogger logger, String[]? availableTools)
	{
		_ = logger ?? throw new ArgumentNullException(nameof(logger));

		Boolean allAllowed = availableTools == null;
		var allowedSet = allAllowed ? null : new HashSet<String>(availableTools!);

		foreach(var method in this.GetTools())
			if(allAllowed || allowedSet!.Contains(method.Name))
			{
				var tool = new ToolFacade(logger, method.Function, method.ConfirmationRequired);
				yield return method.ConfirmationRequired
					? new ApprovalRequiredAIFunction(tool)
					: tool;
			}
	}
}