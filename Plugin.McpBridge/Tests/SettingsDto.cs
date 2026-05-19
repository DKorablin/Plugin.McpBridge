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

	/// <summary>Gets or sets the maximum interval to wait while establishing a connection before the attempt times out.</summary>
	public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(100);

	/// <summary>Gets or sets the collection of available AI providers.</summary>
	public required AiProviderDto[] AiProviders { get; set; }

	/// <summary>Gets or sets the selected AI provider configuration for this instance.</summary>
	public required Guid? SelectedProviderId { get; set; }

	public AiProviderDto? GetSelectedProvider()
		=> this.SelectedProviderId.HasValue
			? this.AiProviders.FirstOrDefault(p => p.Id == this.SelectedProviderId.Value)
			: null;
}