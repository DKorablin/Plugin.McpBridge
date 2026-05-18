using System.ComponentModel;
using System.Runtime.Serialization;

namespace Plugin.McpBridge.Data;

/// <summary>Provider configuration for the GitHub Copilot CLI.</summary>
[DataContract]
[TypeConverter(typeof(ExpandableObjectConverter))]
public sealed record CoPilotProviderDto : AiProviderDto
{
	private String? _coPilotPath;

	private String? _gitHubToken;

	/// <summary>
	/// Gets or sets the absolute path to the GitHub Copilot CLI executable.
	/// </summary>
	/// <remarks>
	/// If not set or set to null or an empty string, the application will attempt to locate the Copilot
	/// CLI using the COPILOT_CLI_PATH environment variable or the system PATH.
	/// This property is typically used to override the default discovery behavior when a specific executable location is required.
	/// </remarks>
	[DataMember]
	[Category("Configuration")]
	[Description("The absolute path to the GitHub Copilot CLI executable. Leave empty to use the COPILOT_CLI_PATH environment variable or system PATH.")]
	[DefaultValue(null)]
	public String? CoPilotPath
	{
		get => this._coPilotPath;
		set
		{
			if(String.IsNullOrWhiteSpace(value))
				value = null;
			this.SetField(ref this._coPilotPath, value, nameof(this.CoPilotPath));
		}
	}

	/// <summary>Gets or sets the GitHub token used to authenticate with the GitHub Copilot CLI.</summary>
	/// <remarks>
	/// If not set or left empty, the authentication process will use the GITHUB_TOKEN environment variable or credentials provided by the GitHub CLI.
	/// This property is typically required only when explicit authentication is needed.
	/// </remarks>
	[DataMember]
	[Category("Authentication")]
	[Description("The GitHub token used to authenticate with the GitHub Copilot CLI. Leave empty to use the GITHUB_TOKEN environment variable or authentication through the GitHub CLI.")]
	[DisplayName("GitHub Token")]
	[DefaultValue(null)]
	public String? GitHubToken
	{
		get => this._gitHubToken;
		set
		{
			if(String.IsNullOrWhiteSpace(value))
				value = null;
			this.SetField(ref this._gitHubToken, value, nameof(this.GitHubToken));
		}
	}

	public override String ToString()
		=> base.ToString();
}