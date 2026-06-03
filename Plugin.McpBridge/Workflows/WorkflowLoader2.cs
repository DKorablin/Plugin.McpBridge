using System.Text.RegularExpressions;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Plugin.McpBridge.Agents;
using Plugin.McpBridge.Data;

namespace Plugin.McpBridge.Workflows;

/// <summary>Builds a <see cref="WorkflowHandle"/> from a <see cref="WorkflowDto"/> loaded at runtime.</summary>
internal sealed class WorkflowLoader2
{
	private readonly SettingsBase _settings;
	private readonly WorkflowDto _config;
	private readonly AgentFactory _agentFactory = new AgentFactory();

	internal WorkflowLoader2(SettingsBase settings, String workflowPath)
	{
		if(String.IsNullOrWhiteSpace(workflowPath))
			throw new ArgumentException("Workflow path must be a non-empty string.", nameof(workflowPath));
		if(!File.Exists(workflowPath))
			throw new FileNotFoundException($"Workflow config file not found at '{workflowPath}'.", workflowPath);

		this._settings = settings ?? throw new ArgumentNullException(nameof(settings));
		this._config = WorkflowDto.Load(workflowPath);

		if(this._config.Nodes.Count == 0)
			throw new InvalidOperationException("WorkflowConfig must contain at least one node.");
	}

	/// <summary>Builds a <see cref="WorkflowHandle"/> from the loaded config using the supplied providers and tools.</summary>
	public async Task<WorkflowHandle> BuildAsync(IEnumerable<AiProviderDto> providers, AIFunction[] tools, CancellationToken cancellationToken = default)
	{
		_ = providers ?? throw new ArgumentNullException(nameof(providers));

		(Workflow workflow, List<IDisposable> resources) = await this.BuildCoreAsync(
			this._config,
			providers.ToArray(),
			tools,
			cancellationToken);

		return new WorkflowHandle(workflow, this._config.Name, resources);
	}

	private async Task<(Workflow Workflow, List<IDisposable> Resources)> BuildCoreAsync(
		WorkflowDto config,
		AiProviderDto[] providers,
		AIFunction[] tools,
		CancellationToken cancellationToken)
	{
		if(config.Pattern == WorkflowPattern.ConditionalGraph)
			return await this.BuildConditionalGraphAsync(config, providers, tools, cancellationToken);

		List<IDisposable> ownedResources = new(config.Nodes.Count);
		Dictionary<String, AIAgent> agentsByName = new(config.Nodes.Count, StringComparer.OrdinalIgnoreCase);

		foreach(WorkflowNode node in config.Nodes)
		{
			AIFunction[] nodeTools = node.AvailableTools == null
				? tools
				: tools.Where(t => Array.Exists(node.AvailableTools, n => n == t.Name)).ToArray();

			if(node.Kind == NodeKind.Workflow)
			{
				WorkflowDto subConfig = new WorkflowDto
				{
					Name = node.Name,
					Pattern = node.Pattern,
					MaxRounds = node.MaxRounds,
					Nodes = node.Nodes,
				};
				(Workflow subWorkflow, List<IDisposable> subResources) = await this.BuildCoreAsync(subConfig, providers, nodeTools, cancellationToken);
				AIAgent subAgent = subWorkflow.AsAIAgent(name: node.Name);
				ownedResources.Add(AgentHandle.FromWorkflow(subAgent, subResources));
				agentsByName[node.Name] = subAgent;
			} else
			{
				AiProviderDto provider = providers.FirstOrDefault(p => p.Id == node.ProviderId)
					?? throw new InvalidOperationException($"Provider '{node.ProviderId}' referenced by node '{node.Name}' was not found.");

				var agentResult = await _agentFactory.CreateAgent(
					provider,
					nodeTools,
					node.SystemPrompt ?? String.Empty,
					node.Name);

				ownedResources.Add(agentResult);
				agentsByName[node.Name] = agentResult.Agent;
			}
		}

		Workflow workflow = config.Pattern switch
		{
			WorkflowPattern.Sequential => AgentWorkflowBuilder.BuildSequential(config.Name, agentsByName.Values),
			WorkflowPattern.Concurrent => AgentWorkflowBuilder.BuildConcurrent(config.Name, agentsByName.Values, r => r.SelectMany(m => m).ToList()),
			WorkflowPattern.GroupChat => WorkflowLoader2.BuildGroupChat(config, agentsByName),
			WorkflowPattern.Handoff => WorkflowLoader2.BuildHandoff(config, agentsByName),
			WorkflowPattern.Magentic => WorkflowLoader2.BuildMagentic(config, agentsByName),
			_ => throw new NotSupportedException($"Workflow pattern '{config.Pattern}' is not supported."),
		};

		typeof(Workflow).GetProperty(nameof(Workflow.Name))?.SetValue(workflow, config.Name);
		return (workflow, ownedResources);
	}

	private static Workflow BuildGroupChat(WorkflowDto config, Dictionary<String, AIAgent> agents)
	{
		Int32 maxRounds = config.MaxRounds ?? 10;
		return AgentWorkflowBuilder
			.CreateGroupChatBuilderWith(a => new RoundRobinGroupChatManager(a) { MaximumIterationCount = maxRounds })
			.AddParticipants([.. agents.Values])
			.WithName(config.Name)
			.WithDescription(config.Description ?? String.Empty)
			.Build();
	}

	private async Task<(Workflow Workflow, List<IDisposable> Resources)> BuildConditionalGraphAsync(
		WorkflowDto config,
		AiProviderDto[] providers,
		AIFunction[] tools,
		CancellationToken cancellationToken)
	{
		List<IDisposable> ownedResources = new(config.Nodes.Count);
		Dictionary<String, AIAgent> agentsByName = new(config.Nodes.Count, StringComparer.OrdinalIgnoreCase);

		foreach(WorkflowNode node in config.Nodes)
		{
			var nodeTools = node.AvailableTools == null
				? tools
				: tools.Where(t => Array.Exists(node.AvailableTools, n => n == t.Name)).ToArray();

			if(node.Kind == NodeKind.Workflow)
			{
				WorkflowDto subConfig = new WorkflowDto
				{
					Name = node.Name,
					Pattern = node.Pattern,
					MaxRounds = node.MaxRounds,
					Nodes = node.Nodes,
				};
				(Workflow subWorkflow, List<IDisposable> subResources) = await this.BuildCoreAsync(subConfig, providers, nodeTools, cancellationToken);
				AIAgent subAgent = subWorkflow.AsAIAgent(name: node.Name);
				ownedResources.Add(AgentHandle.FromWorkflow(subAgent, subResources));
				agentsByName[node.Name] = subAgent;
				continue;
			}

			AiProviderDto provider = providers.FirstOrDefault(p => p.Id == node.ProviderId)
				?? throw new InvalidOperationException($"Provider '{node.ProviderId}' referenced by node '{node.Name}' was not found.");

			// Wrap the Chat Client into a framework recognized AIAgent component
			var handle = await _agentFactory.CreateAgent(
				provider,
				nodeTools,
				node.SystemPrompt ?? String.Empty,
				agentRole: node.Name,
				token: cancellationToken);
			ownedResources.Add(handle);
			agentsByName[node.Name] = handle.Agent;
		}

		String entrypoint = config.Entrypoint ?? config.Nodes[0].Name;
		if(!agentsByName.TryGetValue(entrypoint, out AIAgent? entryAgent))
			throw new InvalidOperationException($"Entrypoint node '{entrypoint}' was not found in the workflow configuration.");

		WorkflowBuilder graphBuilder = new WorkflowBuilder(entryAgent)
			.WithDescription(config.Description ?? String.Empty);

		foreach(WorkflowNode node in config.Nodes)
		{
			AIAgent sourceAgent = agentsByName[node.Name];

			foreach(NodeEdge edge in node.Edges)
			{
				if(edge.To == null && edge.When == null)
					continue;

				AIAgent? targetAgent = null;
				if(edge.To == null)
					targetAgent = sourceAgent; // Self-loop edge for conditional fallback to the same node
				else if(!agentsByName.TryGetValue(edge.To, out targetAgent))
					throw new InvalidOperationException($"Node '{node.Name}' references an invalid edge target destination '{edge.To}'.");

				if(edge.When == null)// Registers an unconditional sequential fallback connection edge
					graphBuilder.AddEdge(sourceAgent, targetAgent);
				else
				{
					String regexPattern = edge.When;
					try
					{
						_ = Regex.Match(String.Empty, regexPattern);
					} catch(ArgumentException ex)
					{
						throw new InvalidOperationException($"Invalid regular expression pattern '{regexPattern}' defined on node '{node.Name}'.", ex);
					}

					/* This field is required, because condition workflow invokes callback 3 times:
					 * 1) Before the source agent executes, with the entire conversation
					 * 2) After the source agent executes, with only output message
					 * 3) context is null which is used to determine the default/fallback path
					 */
					Boolean? cachedResult = null;
					graphBuilder.AddEdge<IList<ChatMessage>>(
						sourceAgent,
						targetAgent,
						condition: (context) =>
						{
							Boolean result;
							String? lastMessageText = context?.LastOrDefault()?.Text;
							if(String.IsNullOrEmpty(lastMessageText))
								result = cachedResult == true;
							else
							{
								result = Regex.IsMatch(lastMessageText, regexPattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
								cachedResult = result;
							}

							System.Diagnostics.Debug.WriteLine($"[Edge {node.Name}→{edge.To}] Pattern='{regexPattern}' Text='{lastMessageText?.Substring(0, Math.Min(60, lastMessageText?.Length ?? 0)) ?? "(null)"}...' Result={result}");
							return result;
						});
				}
			}
		}

		Workflow workflow = graphBuilder.Build();
		typeof(Workflow).GetProperty(nameof(Workflow.Name))?.SetValue(workflow, config.Name);

		return (workflow, ownedResources);
	}

	private static Workflow BuildMagentic(WorkflowDto config, Dictionary<String, AIAgent> agents)
	{
		String managerName = config.Entrypoint
			?? config.Nodes.FirstOrDefault(n => n.IsOrchestrator)?.Name
			?? config.Nodes[0].Name;

		if(!agents.TryGetValue(managerName, out AIAgent? manager))
			throw new InvalidOperationException($"Manager node '{managerName}' was not found in the workflow configuration.");

		IEnumerable<AIAgent> participants = agents.Values.Where(a => a != manager);

		return new MagenticWorkflowBuilder(manager)
			.AddParticipants(participants)
			.WithMaxRounds(config.MaxRounds)
			.WithName(config.Name)
			.WithDescription(config.Description ?? String.Empty)
			.Build();
	}

	private static Workflow BuildHandoff(WorkflowDto config, Dictionary<String, AIAgent> agents)
	{
		String entryName = config.Entrypoint ?? config.Nodes[0].Name;
		HandoffWorkflowBuilder builder = AgentWorkflowBuilder.CreateHandoffBuilderWith(agents[entryName]);

		foreach(WorkflowNode node in config.Nodes)
		{
			if(node.Targets == null || node.Targets.Length == 0)
				continue;

			AIAgent from = agents[node.Name];
			IEnumerable<AIAgent> targets = node.Targets.Select(n => agents[n]);
			builder.WithHandoffs(from, targets);
		}

		return builder.Build();
	}
}