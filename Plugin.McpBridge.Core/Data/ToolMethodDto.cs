using Microsoft.Extensions.AI;

namespace Plugin.McpBridge.Data;

/// <summary>Represents metadata and invocation details for a tool method.</summary>
/// <param name="ConfirmationRequired">true if the method requires user confirmation before execution; otherwise, false.</param>
/// <param name="Name">The unique name that identifies the tool method.</param>
/// <param name="Description">A brief description of the tool method's purpose or functionality.</param>
/// <param name="Function">The <see cref="AIFunction"/> delegate that can be invoked to execute the tool method.</param>
internal record ToolMethodDto(Boolean ConfirmationRequired, String Name, String Description, AIFunction Function);