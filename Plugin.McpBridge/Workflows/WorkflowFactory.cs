using Microsoft.Extensions.AI;
using Plugin.McpBridge.Tools;
using SAL.Flatbed;

namespace Plugin.McpBridge.Workflows;

internal sealed class WorkflowFactoryItem
{
	public String Name { get; }

	public String WorkflowPath { get; }

	internal WorkflowHandle? Handle { get; set; }

	internal WorkflowFactoryItem(String name, String workflowPath)
	{
		this.Name = name;
		this.WorkflowPath = workflowPath;
	}
}

internal sealed class WorkflowFactory : IDisposable
{
	private readonly IHost _host;
	private readonly Settings _settings;
	private readonly ITraceSource _trace;
	private readonly Object _syncRoot = new Object();
	private List<WorkflowFactoryItem> _workflows = new List<WorkflowFactoryItem>();

	internal WorkflowFactory(IHost host, Settings settings, ITraceSource trace)
	{
		this._host = host ?? throw new ArgumentNullException(nameof(host));
		this._settings = settings ?? throw new ArgumentNullException(nameof(settings));
		this._trace = trace ?? throw new ArgumentNullException(nameof(trace));
	}

	public IReadOnlyList<WorkflowFactoryItem> GetWorkflows()
	{
		lock(this._syncRoot)
			return this._workflows.ToList();
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

		String? workflowsDirectory = this._settings.WorkflowsDirectory;
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

	public async Task<WorkflowHandle> GetHandleAsync(WorkflowFactoryItem workflow, CancellationToken token)
	{
		if(workflow.Handle != null)
			return workflow.Handle;

		ToolsFactory toolsFactory = new ToolsFactory(this._host, this._settings, this._settings.SelectedAgent);
		AIFunction[] tools = toolsFactory.CreateTools(this._trace).ToArray();

		WorkflowLoader2 loader = new WorkflowLoader2(this._settings, workflow.WorkflowPath);
		WorkflowHandle loadedHandle = await loader.BuildAsync(this._settings.AiProviders, tools, token);

		lock(this._syncRoot)
		{
			if(workflow.Handle != null)
			{
				loadedHandle.Dispose();
				return workflow.Handle;
			}

			if(!this._workflows.Contains(workflow))
			{
				loadedHandle.Dispose();
				throw new InvalidOperationException("Selected workflow is no longer available.");
			}

			workflow.Handle = loadedHandle;
			return loadedHandle;
		}
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