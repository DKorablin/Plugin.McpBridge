namespace Plugin.McpBridge.Tools;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
internal class ToolAttribute : Attribute
{
	public Boolean ConfirmationRequired { get; }

	public ToolAttribute(Boolean confirmationRequired = false)
		=> this.ConfirmationRequired = confirmationRequired;
}