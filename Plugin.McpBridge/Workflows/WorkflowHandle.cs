using Microsoft.Agents.AI.Workflows;

namespace Plugin.McpBridge.Workflows;

/// <summary>Owns the lifetime of a <see cref="Microsoft.Agents.AI.Workflows.Workflow"/> and all its underlying disposable resources.</summary>
internal sealed class WorkflowHandle : IDisposable
{
	private readonly IReadOnlyList<IDisposable> _ownedResources;
	private readonly String _configName;
	private Boolean _disposed;

	/// <summary>The constructed workflow, ready for DI registration.</summary>
	internal Workflow Workflow { get; }

	/// <summary>The workflow's name; falls back to the config-level name when the builder does not support naming (e.g. Handoff in MAF v1.6).</summary>
	internal String Name => !String.IsNullOrEmpty(this.Workflow.Name) ? this.Workflow.Name : this._configName;

	internal WorkflowHandle(Workflow workflow, String configName, IReadOnlyList<IDisposable> ownedResources)
	{
		this.Workflow = workflow;
		this._configName = configName;
		this._ownedResources = ownedResources;
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		if(this._disposed)
			return;
		this._disposed = true;
		foreach(IDisposable r in this._ownedResources)
			r.Dispose();
	}
}
