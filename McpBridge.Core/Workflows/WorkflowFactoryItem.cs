namespace McpBridge.Core.Workflows;

public class WorkflowFactoryItem
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