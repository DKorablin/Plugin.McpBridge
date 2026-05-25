using System.Diagnostics.CodeAnalysis;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Plugin.McpBridge.Agents;
using Plugin.McpBridge.Data;

namespace Plugin.McpBridge.Workflows;

/// <summary>Builds a <see cref="WorkflowHandle"/> from a <see cref="WorkflowConfig"/> loaded at runtime.</summary>
internal sealed class WorkflowLoader
{
	private readonly WorkflowConfig _config;

	internal WorkflowLoader(String workflowPath)
	{
		if(String.IsNullOrWhiteSpace(workflowPath))
			throw new ArgumentException("Workflow path must be a non-empty string.", nameof(workflowPath));
		if(!File.Exists(workflowPath))
			throw new FileNotFoundException($"Workflow config file not found at '{workflowPath}'.", workflowPath);

		this._config = WorkflowConfig.Load(workflowPath);

		if(this._config.Nodes.Count == 0)
			throw new InvalidOperationException("WorkflowConfig must contain at least one node.");
	}

	/// <summary>Builds a <see cref="WorkflowHandle"/> from the loaded config using the supplied providers and tools.</summary>
	public WorkflowHandle Build(AiProviderDto[] providers, TimeSpan connectionTimeout, AIFunction[] tools)
	{
		_ = providers ?? throw new ArgumentNullException(nameof(providers));

		System.Diagnostics.Debugger.Launch();
		(Workflow workflow, List<IDisposable> resources) = this.BuildCore(this._config, providers, connectionTimeout, tools);
		return new WorkflowHandle(workflow, this._config.Name, resources);
	}

	/// <summary>Recursively builds the workflow graph; returns the raw <see cref="Workflow"/> and all its owned disposables.</summary>
	private (Workflow Workflow, List<IDisposable> Resources) BuildCore(
		WorkflowConfig config,
		AiProviderDto[] providers,
		TimeSpan connectionTimeout,
		AIFunction[] tools)
	{
		if(config.Pattern == WorkflowPattern.ConditionalGraph)
			return WorkflowLoader.BuildConditionalGraph(config, providers, connectionTimeout);

		List<IDisposable> ownedResources = new(config.Nodes.Count);
		Dictionary<String, AIAgent> agentsByName = new(config.Nodes.Count, StringComparer.OrdinalIgnoreCase);

		foreach(WorkflowNode node in config.Nodes)
		{
			AIFunction[] nodeTools = node.AvailableTools == null
				? tools
				: tools.Where(t => Array.Exists(node.AvailableTools, n => n == t.Name)).ToArray();

			if(node.Kind == NodeKind.Workflow)
			{
				WorkflowConfig subConfig = new WorkflowConfig
				{
					Name = node.Name,
					Pattern = node.Pattern,
					MaxRounds = node.MaxRounds,
					Nodes = node.Nodes,
				};
				(Workflow subWorkflow, List<IDisposable> subResources) = this.BuildCore(subConfig, providers, connectionTimeout, nodeTools);
				AIAgent subAgent = subWorkflow.AsAIAgent(name: node.Name);
				ownedResources.Add(AgentHandle.FromWorkflow(subAgent, subResources));
				agentsByName[node.Name] = subAgent;
			}
			else
			{
				AiProviderDto provider = providers.FirstOrDefault(p => p.Id == node.ProviderId)
					?? throw new InvalidOperationException($"Provider '{node.ProviderId}' referenced by node '{node.Name}' was not found in the provider list.");

				HttpClient http = new HttpClient { Timeout = connectionTimeout };
				ownedResources.Add(http);

				IChatClient chatClient = AgentFactory.CreateChatClient(provider, http);
				agentsByName[node.Name] = chatClient.AsAIAgent(
					instructions: node.SystemPrompt,
					tools: nodeTools,
					name: node.Name);
			}
		}

		Workflow workflow = config.Pattern switch
		{
			WorkflowPattern.Sequential        => AgentWorkflowBuilder.BuildSequential(config.Name, agentsByName.Values),
			WorkflowPattern.Concurrent        => AgentWorkflowBuilder.BuildConcurrent(config.Name, agentsByName.Values,
				responses => responses.SelectMany(r => r).ToList()),
			WorkflowPattern.GroupChat         => WorkflowLoader.BuildGroupChat(config, agentsByName),
			WorkflowPattern.Handoff           => WorkflowLoader.BuildHandoff(config, agentsByName),
			WorkflowPattern.ConditionalGraph  => throw new InvalidOperationException("ConditionalGraph is handled before this point."),
			_ => throw new NotSupportedException($"Workflow pattern '{config.Pattern}' is not supported."),
		};

		// Normalize: HandoffWorkflowBuilder in MAF v1.6 sets Workflow.Name to its internal executor
		// name ("HandoffStart") instead of exposing WithName(); override via the private setter.
		typeof(Workflow).GetProperty(nameof(Workflow.Name))?.SetValue(workflow, config.Name);

		return (workflow, ownedResources);
	}

	/// <summary>Builds a round-robin group-chat workflow; the manager rotates through all nodes in declaration order up to <see cref="WorkflowConfig.MaxRounds"/> rounds.</summary>
	private static Workflow BuildGroupChat(WorkflowConfig config, Dictionary<String, AIAgent> agents)
	{
		Int32 maxRounds = config.MaxRounds ?? 10;
		return AgentWorkflowBuilder
			.CreateGroupChatBuilderWith(a => new RoundRobinGroupChatManager(a) { MaximumIterationCount = maxRounds })
			.AddParticipants([.. agents.Values])
			.WithName(config.Name)
			.Build();
	}

	/// <summary>Builds a conditional-graph workflow: each node's <see cref="WorkflowNode.Edges"/> determine the next step via text-match predicates over the agent's last message.</summary>
	private static (Workflow Workflow, List<IDisposable> Resources) BuildConditionalGraph(
		WorkflowConfig config,
		AiProviderDto[] providers,
		TimeSpan connectionTimeout)
	{
		List<IDisposable> ownedResources = new(config.Nodes.Count);
		Dictionary<String, (IChatClient Client, String? Prompt, NodeEdge[] Edges)> steps =
			new(config.Nodes.Count, StringComparer.OrdinalIgnoreCase);

		foreach(WorkflowNode node in config.Nodes)
		{
			if(node.Kind == NodeKind.Workflow)
			{
				if(node.Pattern != WorkflowPattern.GroupChat)
					throw new NotSupportedException($"Only GroupChat sub-workflows are supported inside ConditionalGraph; node '{node.Name}' uses '{node.Pattern}'.");

				List<(IChatClient Client, String? Prompt)> participants = new(node.Nodes.Count);

				foreach(WorkflowNode subNode in node.Nodes)
				{
					if(subNode.Kind != NodeKind.Agent)
						throw new NotSupportedException($"Only Agent sub-nodes are supported inside a ConditionalGraph sub-workflow; '{subNode.Name}' has Kind={subNode.Kind}.");

					AiProviderDto subProvider = providers.FirstOrDefault(p => p.Id == subNode.ProviderId)
						?? throw new InvalidOperationException($"Provider '{subNode.ProviderId}' referenced by sub-node '{subNode.Name}' was not found.");

					HttpClient subHttp = new HttpClient { Timeout = connectionTimeout };
					ownedResources.Add(subHttp);

					participants.Add((AgentFactory.CreateChatClient(subProvider, subHttp), subNode.SystemPrompt));
				}

				steps[node.Name] = (new GroupChatAdapter(participants, node.MaxRounds ?? 3), null, [.. node.Edges]);
			}
			else
			{
				AiProviderDto provider = providers.FirstOrDefault(p => p.Id == node.ProviderId)
					?? throw new InvalidOperationException($"Provider '{node.ProviderId}' referenced by node '{node.Name}' was not found.");

				HttpClient http = new HttpClient { Timeout = connectionTimeout };
				ownedResources.Add(http);

				IChatClient chatClient = AgentFactory.CreateChatClient(provider, http);
				steps[node.Name] = (chatClient, node.SystemPrompt, [.. node.Edges]);
			}
		}

		String entrypoint = config.Entrypoint ?? config.Nodes[0].Name;
		if(!steps.ContainsKey(entrypoint))
			throw new InvalidOperationException($"Entrypoint '{entrypoint}' was not found in the node list.");

		ConditionalGraphClient graphClient = new(steps, entrypoint);
		AIAgent graphAgent = graphClient.AsAIAgent(name: config.Name);
		Workflow workflow = AgentWorkflowBuilder.BuildSequential(config.Name, graphAgent);
		typeof(Workflow).GetProperty(nameof(Workflow.Name))?.SetValue(workflow, config.Name);

		return (workflow, ownedResources);
	}

	/// <summary>Builds a handoff workflow wired from each node's <see cref="WorkflowNode.Targets"/>; entry point is <see cref="WorkflowConfig.Entrypoint"/> or the first node.</summary>
	[SuppressMessage("Reliability", "MAAIW001", Justification = "Experimental MAF workflow APIs intentionally used here.")]
	private static Workflow BuildHandoff(WorkflowConfig config, Dictionary<String, AIAgent> agents)
	{
#pragma warning disable MAAIW001
		String entryName = config.Entrypoint ?? config.Nodes[0].Name;
		HandoffWorkflowBuilder builder = AgentWorkflowBuilder.CreateHandoffBuilderWith(agents[entryName]);

		foreach(WorkflowNode node in config.Nodes)
		{
			if(node.Targets == null || node.Targets.Length == 0)
				continue;

			AIAgent from = agents[node.Name];
			IEnumerable<AIAgent> targets = node.Targets.Select(n =>
				agents.TryGetValue(n, out AIAgent? target)
					? target
					: throw new InvalidOperationException($"Node '{node.Name}' references unknown target '{n}'."));

			builder.WithHandoffs(from, targets);
		}

		return builder.Build();
#pragma warning restore MAAIW001
	}

	/// <summary>
	/// An <see cref="IChatClient"/> that drives a conditional-graph routing loop.
	/// Each call to <see cref="GetResponseAsync"/> runs the entire graph from the entrypoint to a terminal node and
	/// returns all messages produced during traversal.
	/// </summary>
	private sealed class ConditionalGraphClient(
		IReadOnlyDictionary<String, (IChatClient Client, String? Prompt, NodeEdge[] Edges)> steps,
		String entrypoint) : IChatClient
	{
		public ChatClientMetadata Metadata { get; } = new("ConditionalGraph", null, null);
		public void Dispose() { }

		public async Task<ChatResponse> GetResponseAsync(
			IEnumerable<ChatMessage> messages,
			ChatOptions? options = null,
			CancellationToken cancellationToken = default)
		{
			List<ChatMessage> inputMessages = new(messages);
			List<ChatMessage> history = [.. inputMessages];
			String currentStep = entrypoint;

			while(true)
			{
				(IChatClient client, String? prompt, NodeEdge[] edges) = steps[currentStep];

				// Prepend system prompt for this specific agent; pass null options to avoid
				// injecting outer-workflow tools (e.g. handoff tool calls) into inner agents.
				IEnumerable<ChatMessage> stepInput = prompt is null
					? history
					: history.Prepend(new ChatMessage(ChatRole.System, prompt));

				ChatResponse stepResponse = await client.GetResponseAsync(stepInput, options: null, cancellationToken);
				history.AddRange(stepResponse.Messages);

				String lastText = stepResponse.Messages
					.LastOrDefault(static m => m.Role == ChatRole.Assistant)
					?.Text ?? String.Empty;

				NodeEdge? edge = ConditionalGraphClient.FindMatchingEdge(edges, lastText);
				if(edge is null || edge.To is null)
					break;

				currentStep = edge.To;
			}

			// Return only the messages produced during this traversal, not the caller's input.
			return new ChatResponse(history.Skip(inputMessages.Count).ToList());
		}

		public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
			IEnumerable<ChatMessage> messages,
			ChatOptions? options = null,
			[System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
		{
			ChatResponse response = await this.GetResponseAsync(messages, options, cancellationToken);
			foreach(ChatResponseUpdate update in response.ToChatResponseUpdates())
				yield return update;
		}

		public Object? GetService(Type serviceType, Object? serviceKey = null) => null;

		/// <summary>Returns the first edge whose condition matches <paramref name="lastText"/>, or <see langword="null"/> if none match.</summary>
		private static NodeEdge? FindMatchingEdge(NodeEdge[] edges, String lastText)
		{
			foreach(NodeEdge edge in edges)
			{
				if(edge.When is null)
					return edge;

				Int32 colon = edge.When.IndexOf(':', StringComparison.Ordinal);
				if(colon < 0)
					continue;

				String conditionType = edge.When[..colon];
				String conditionValue = edge.When[(colon + 1)..];

				Boolean match = conditionType switch
				{
					"MessageContains"   => lastText.Contains(conditionValue, StringComparison.OrdinalIgnoreCase),
					"MessageStartsWith" => lastText.StartsWith(conditionValue, StringComparison.OrdinalIgnoreCase),
					"MessageEndsWith"   => lastText.EndsWith(conditionValue, StringComparison.OrdinalIgnoreCase),
					_                   => false,
				};

				if(match)
					return edge;
			}
			return null;
		}
	}

	/// <summary>
	/// An <see cref="IChatClient"/> that simulates a round-robin group chat by cycling through
	/// a fixed list of participant <see cref="IChatClient"/>s for a set number of rounds.
	/// Each participant sees the full running history (including prior participants' turns in the same round).
	/// </summary>
	private sealed class GroupChatAdapter(
		IReadOnlyList<(IChatClient Client, String? Prompt)> participants,
		Int32 maxRounds) : IChatClient
	{
		public ChatClientMetadata Metadata { get; } = new("GroupChatAdapter", null, null);
		public void Dispose() { }

		public async Task<ChatResponse> GetResponseAsync(
			IEnumerable<ChatMessage> messages,
			ChatOptions? options = null,
			CancellationToken cancellationToken = default)
		{
			List<ChatMessage> inputMessages = new(messages);
			List<ChatMessage> history = [.. inputMessages];

			for(Int32 round = 0; round < maxRounds; round++)
			{
				foreach((IChatClient client, String? prompt) in participants)
				{
					IEnumerable<ChatMessage> stepInput = prompt is null
						? history
						: history.Prepend(new ChatMessage(ChatRole.System, prompt));

					ChatResponse stepResponse = await client.GetResponseAsync(stepInput, options: null, cancellationToken);
					history.AddRange(stepResponse.Messages);
				}
			}

			return new ChatResponse(history.Skip(inputMessages.Count).ToList());
		}

		public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
			IEnumerable<ChatMessage> messages,
			ChatOptions? options = null,
			[System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
		{
			ChatResponse response = await this.GetResponseAsync(messages, options, cancellationToken);
			foreach(ChatResponseUpdate update in response.ToChatResponseUpdates())
				yield return update;
		}

		public Object? GetService(Type serviceType, Object? serviceKey = null) => null;
	}
}
