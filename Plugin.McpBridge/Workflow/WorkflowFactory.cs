using McpBridge.Core.Workflows;
using SAL.Flatbed;

namespace Plugin.McpBridge.Workflow;

internal sealed class WorkflowFactory : IDisposable
{
	private readonly String _workflowsDirectory;
	private readonly ITraceSource _trace;
	private readonly Object _syncRoot = new Object();
	private List<WorkflowFactoryItem> _workflows = new List<WorkflowFactoryItem>();

	internal WorkflowFactory(String workflowsDirectory, ITraceSource trace)
	{
		this._workflowsDirectory = workflowsDirectory ?? throw new ArgumentNullException(nameof(workflowsDirectory));
		this._trace = trace ?? throw new ArgumentNullException(nameof(trace));
	}

	public WorkflowFactoryItem[] GetWorkflows()
	{
		lock(this._syncRoot)
			return this._workflows.ToArray();
	}

	public WorkflowFactoryItem? GetWorkflow(String? workflowName)
	{
		if(workflowName == null)
			return null;

		lock(this._syncRoot)
			return this._workflows.FirstOrDefault(w => String.Equals(w.Name, workflowName, StringComparison.Ordinal));
	}

	public void Reload()
	{
		List<WorkflowFactoryItem> loadedWorkflows = new List<WorkflowFactoryItem>();

		String? workflowsDirectory = this._workflowsDirectory;
		if(workflowsDirectory != null && Directory.Exists(workflowsDirectory))
			foreach(String workflowFile in Directory.EnumerateFiles(workflowsDirectory, "*.json"))
				try
				{
					WorkflowDto config = WorkflowDto.Load(workflowFile);
					String workflowName = String.IsNullOrWhiteSpace(config.Name)
						? Path.GetFileNameWithoutExtension(workflowFile)
						: config.Name;
					loadedWorkflows.Add(new WorkflowFactoryItem(workflowName, workflowFile));
				} catch(Exception exc)
				{
					this._trace.TraceData(System.Diagnostics.TraceEventType.Error, 0, exc);
				}

		List<WorkflowFactoryItem> previousWorkflows;
		lock(this._syncRoot)
		{
			previousWorkflows = this._workflows;
			this._workflows = loadedWorkflows;
		}

		foreach(WorkflowFactoryItem workflow in previousWorkflows)
			workflow.Handle?.Dispose();
	}

	public void Dispose()
	{
		List<WorkflowFactoryItem> workflows;
		lock(this._syncRoot)
		{
			workflows = this._workflows;
			this._workflows = new List<WorkflowFactoryItem>();
		}

		foreach(WorkflowFactoryItem workflow in workflows)
			workflow.Handle?.Dispose();
	}
}