namespace Plugin.McpBridge.Helpers;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
internal class ToolAttribute : Attribute
{
	public Settings.Tools ToolName { get; }

	public Boolean ConfirmationRequired { get; }

	public ToolAttribute(Settings.Tools toolName, Boolean confirmationRequired = false)
	{
		this.ToolName = toolName;
		this.ConfirmationRequired = confirmationRequired;
	}
}