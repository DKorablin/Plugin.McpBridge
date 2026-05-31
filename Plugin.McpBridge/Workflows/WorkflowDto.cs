using System.Text.Json;
using System.Text.Json.Serialization;

namespace Plugin.McpBridge.Workflows;

/// <summary>Supported multi-agent workflow topology patterns.</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
internal enum WorkflowPattern
{
	/// <summary>Agents run one after another; each receives the full conversation so far.</summary>
	Sequential,
	/// <summary>All agents run in parallel on the same input; their responses are aggregated.</summary>
	Concurrent,
	/// <summary>A manager agent (round-robin or custom) selects who speaks next each turn.</summary>
	GroupChat,
	/// <summary>Agents delegate to each other via tool calls; the initial agent is the entry point.</summary>
	Handoff,
	/// <summary>Agents execute in a directed graph; outgoing edges carry optional text-match conditions that control routing.</summary>
	ConditionalGraph,
	/// <summary>An LLM-powered manager agent dynamically plans tasks and delegates to specialist agents, replanning as needed.</summary>
	Magentic,
}

/// <summary>Discriminates between leaf agent nodes and composite sub-workflow nodes in the workflow graph.</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
internal enum NodeKind
{
	/// <summary>A single agent backed by an <see cref="IChatClient"/> from a configured provider.</summary>
	Agent,
	/// <summary>A nested workflow whose internal nodes run as a unit and expose a single entry point to the parent.</summary>
	Workflow,
}

/// <summary>A node in the workflow graph — either a leaf <see cref="NodeKind.Agent"/> or a nested <see cref="NodeKind.Workflow"/>.</summary>
internal sealed record WorkflowNode
{
	// ── Common ───────────────────────────────────────────────────────────────

	/// <summary>Unique name used to identify this node and resolve <see cref="Targets"/> references.</summary>
	public String Name { get; set; } = String.Empty;

	/// <summary>Discriminates between a leaf agent and a nested sub-workflow. Defaults to <see cref="NodeKind.Agent"/>.</summary>
	public NodeKind Kind { get; set; } = NodeKind.Agent;

	/// <summary>Tool names this node may call. <see langword="null"/> enables all tools; an empty array disables all tools.</summary>
	public String[]? AvailableTools { get; set; }

	/// <summary>
	/// Outgoing connections to other nodes by name.
	/// In a <see cref="WorkflowPattern.Handoff"/> parent these become handoff targets.
	/// Ignored by Sequential, Concurrent, and GroupChat parents (node order governs those topologies).
	/// </summary>
	public String[]? Targets { get; set; }

	// ── Agent-specific (Kind == Agent) ───────────────────────────────────────

	/// <summary>ID of the <c>AiProviderDto</c> that backs this agent. Required when <see cref="Kind"/> is <see cref="NodeKind.Agent"/>.</summary>
	public Guid ProviderId { get; set; }

	/// <summary>System prompt. <see langword="null"/> falls back to the global settings prompt.</summary>
	public String? SystemPrompt { get; set; }

	// ── ConditionalGraph-specific (Pattern == ConditionalGraph) ────────────────

	/// <summary>
	/// Outgoing conditional edges for <see cref="WorkflowPattern.ConditionalGraph"/> nodes.
	/// Edges are evaluated in declaration order; the first match wins.
	/// A <see langword="null"/> <see cref="NodeEdge.When"/> acts as the unconditional default.
	/// </summary>
	public List<NodeEdge> Edges { get; set; } = [];

	// ── Workflow-specific (Kind == Workflow) ─────────────────────────────────

	/// <summary>Topology of the nested sub-workflow. Required when <see cref="Kind"/> is <see cref="NodeKind.Workflow"/>.</summary>
	public WorkflowPattern Pattern { get; set; }

	/// <summary>Maximum group-chat rounds. Only used when <see cref="Pattern"/> is <see cref="WorkflowPattern.GroupChat"/>.</summary>
	public Int32? MaxRounds { get; set; }

	/// <summary>Reserved for a future LLM-backed group-chat orchestrator.</summary>
	public Boolean IsOrchestrator { get; set; }

	/// <summary>Child nodes of this sub-workflow. Required when <see cref="Kind"/> is <see cref="NodeKind.Workflow"/>.</summary>
	public List<WorkflowNode> Nodes { get; set; } = [];
}

/// <summary>A directed edge in a <see cref="WorkflowPattern.ConditionalGraph"/> node's routing table.</summary>
internal sealed record NodeEdge
{
	/// <summary>Name of the target node. <see langword="null"/> signals end-of-workflow (routes to <c>HandoffEnd</c>).</summary>
	public String? To { get; set; }

	/// <summary>
	/// Optional routing condition in <c>Type:Value</c> format, e.g. <c>MessageContains:ACCEPTED</c>.
	/// <see langword="null"/> means unconditional — this edge matches when no earlier edge did.
	/// Supported types: <c>MessageContains</c>, <c>MessageStartsWith</c>, <c>MessageEndsWith</c>.
	/// </summary>
	public String? When { get; set; }
}

/// <summary>Top-level descriptor for a multi-agent workflow loaded from JSON.</summary>
internal record WorkflowDto
{
	/// <summary>Logical name for the workflow; also used as the <c>AIAgent</c> name exposed to callers.</summary>
	public String Name { get; set; } = String.Empty;

	/// <summary>Topology pattern that controls how agents are wired together.</summary>
	public WorkflowPattern Pattern { get; set; }

	/// <summary>Maximum number of group-chat rounds before the workflow stops. Only used in <see cref="WorkflowPattern.GroupChat"/>.</summary>
	public Int32? MaxRounds { get; set; }

	/// <summary>Name of the node that receives the first user message in a <see cref="WorkflowPattern.Handoff"/> workflow. Defaults to the first node when absent.</summary>
	public String? Entrypoint { get; set; }

	/// <summary>Human-readable description of the workflow; surfaced to callers and used as the workflow agent description.</summary>
	public String? Description { get; set; }

	/// <summary>Ordered list of graph nodes — leaf agents or nested sub-workflows.</summary>
	public List<WorkflowNode> Nodes { get; set; } = [];

	private static readonly JsonSerializerOptions _options = new()
	{
		ReadCommentHandling = JsonCommentHandling.Skip,
		PropertyNameCaseInsensitive = true,
		AllowTrailingCommas = true,// C#-like syntax
		Converters = { new JsonStringEnumConverter() },
	};

	/// <summary>Loads and deserializes a <see cref="WorkflowDto"/> from a JSON file at <paramref name="filePath"/>.</summary>
	internal static WorkflowDto Load(String filePath)
	{
		String json = File.ReadAllText(filePath);
		return JsonSerializer.Deserialize<WorkflowDto>(json, _options)
			?? throw new InvalidOperationException($"Failed to deserialize workflow config from '{filePath}'.");
	}
}