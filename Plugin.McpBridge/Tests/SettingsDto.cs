using Plugin.McpBridge.Data;

namespace Plugin.McpBridge.Tests;

/// <summary>Serializable snapshot of the settings needed to start the UI process.</summary>
public sealed class SettingsDto
{
	/// <summary>Gets or sets the URL of the user interface server.</summary>
	public required String UiServerUrl { get; set; }

	/// <summary>Base URL of the in-process tool bridge server (e.g. http://localhost:12345).</summary>
	public required String McpServerUrl { get; set; }

	/// <summary>Gets or sets the instructions associated with this instance.</summary>
	public required String Instructions { get; set; }

	/// <summary>Gets or sets the list of tool permissions assigned to the user.</summary>
	public String[]? ToolsPermission { get; set; }

	/// <summary>Gets or sets the list of plugin permissions granted to the user or application.</summary>
	public String[]? PluginsPermission { get; set; }

	/// <summary>Gets or sets the collection of available AI providers.</summary>
	public required AiProviderDto[] AiProviders { get; set; }

	/// <summary>Gets or sets the selected AI provider configuration for this instance.</summary>
	public required Guid? SelectedProviderId { get; set; }

	/// <summary>Gets or sets the path to the directory containing skill definitions.</summary>
	public String? SkillsDirectory { get; set; }

	/// <summary>Gets or sets the directory path used for storing session data for the AG-UI.</summary>
	public String? AgUISessionStorageDirectory { get; set; }

	/// <summary>Gets or sets the directory path where the RAG knowledge base files are stored.</summary>
	public String? RagKnowledgeBaseDirectory { get; set; }

	/// <summary>Gets or sets the path to the directory where workflow files are stored.</summary>
	public String? WorkflowsDirectory { get; set; }

	public AiProviderDto? GetSelectedProvider()
		=> this.SelectedProviderId.HasValue
			? this.AiProviders.FirstOrDefault(p => p.Id == this.SelectedProviderId.Value)
			: null;
}