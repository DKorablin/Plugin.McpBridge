using System.ComponentModel;
using System.Runtime.Serialization;

namespace Plugin.McpBridge.Data;

[DataContract]
public record AzureProviderDto : NetworkProviderDto
{
	private String? _deploymentName;

	/// <summary>The Azure OpenAI deployment name found in Azure OpenAI Studio under Deployments or organization identifier supported by some OpenAI-compatible providers.</summary>
	[DataMember]
	[Category("AI Provider")]
	[DisplayName("Deployment Name")]
	[Description("The deployment name from Azure OpenAI Studio (Deployments section). Required when using the Azure OpenAI provider.")]
	[DefaultValue(null)]
	public String? DeploymentName
	{
		get => _deploymentName;
		set
		{
			if(String.IsNullOrWhiteSpace(value))
				value = null;

			this.SetField(ref _deploymentName, value, nameof(this.DeploymentName));
		}
	}

	public override String ToString()
		=> base.ToString();
}