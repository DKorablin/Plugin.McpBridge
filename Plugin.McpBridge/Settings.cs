using System.ComponentModel;

namespace Plugin.McpBridge
{
	public enum AiProviderType
	{
		OpenAI,
		AzureOpenAI,
		OpenAICompatible,
		LocalOpenAICompatible,
		QwenCompatible,
		GrokCompatible,
		GeminiCompatible,
		CustomCompatible,
	}

	/// <summary>Configuration settings for the MCP Bridge plugin.</summary>
	public class Settings : INotifyPropertyChanged
	{
		private static class Defaults
		{
			public const AiProviderType ProviderType = AiProviderType.OpenAI;
			public const String ModelId = "gpt-4o-mini";
			public const String AzureApiVersion = "2024-10-21";
			public const String AssistantSystemPrompt = "You are a SAL automation assistant. Use available MCP tools when useful. Return clear user-facing responses, or a command payload only when automation is required.";
			public const Int32 AgentLoopCap = 3;
		}

		private AiProviderType _providerType = Defaults.ProviderType;
		private String _modelId = Defaults.ModelId;
		private String? _apiKey = null;
		private String? _modelEndpointUrl = null;
		private String? _organizationId;
		private String? _deploymentName = null;
		private String _azureApiVersion = Defaults.AzureApiVersion;
		private String? _assistantSystemPrompt = Defaults.AssistantSystemPrompt;
		private Double? _temperature;
		private Int32? _maxTokens;
		private Int32 _agentLoopCap = Defaults.AgentLoopCap;

		/// <summary>Selects the provider profile used to initialize the AI client.</summary>
		[Category("AI Provider")]
		[DefaultValue(Defaults.ProviderType)]
		[Description("Selects the provider profile (OpenAI, Azure OpenAI, Local/OpenAI-compatible, Qwen-compatible, Grok-compatible, Gemini-compatible, custom compatible).")]
		public AiProviderType ProviderType
		{
			get => this._providerType;
			set => this.SetField(ref this._providerType, value, nameof(this.ProviderType));
		}

		/// <summary>The AI model identifier used for chat completions.</summary>
		[Category("AI Model Settings")]
		[DefaultValue(Defaults.ModelId)]
		[Description("The AI model identifier or Azure OpenAI deployment name used for chat completions (e.g. gpt-4o-mini).")]
		public String ModelId
		{
			get => this._modelId;
			set => this.SetField(ref this._modelId, value, nameof(this.ModelId));
		}

		/// <summary>The API key used to authenticate with the AI provider.</summary>
		[Category("AI Model Settings")]
		[Description("The API key used to authenticate with the AI provider.")]
		public String? ApiKey
		{
			get => this._apiKey;
			set
			{
				if(String.IsNullOrWhiteSpace(value))
					value = null;

				this.SetField(ref this._apiKey, value, nameof(this.ApiKey));
			}
		}

		/// <summary>Optional organization identifier supported by some OpenAI-compatible providers.</summary>
		[Category("AI Provider")]
		[Description("Optional organization identifier supported by some OpenAI-compatible providers.")]
		public String? OrganizationId
		{
			get => this._organizationId;
			set
			{
				if(String.IsNullOrWhiteSpace(value))
					value = null;

				this.SetField(ref this._organizationId, value, nameof(this.OrganizationId));
			}
		}

		/// <summary>The Azure OpenAI deployment name found in Azure OpenAI Studio under Deployments.</summary>
		[Category("Azure OpenAI")]
		[Description("The deployment name from Azure OpenAI Studio (Deployments section). Required when using the Azure OpenAI provider.")]
		public String? DeploymentName
		{
			get => this._deploymentName;
			set
			{
				if(String.IsNullOrWhiteSpace(value))
					value = null;

				this.SetField(ref this._deploymentName, value, nameof(this.DeploymentName));
			}
		}

		/// <summary>Optional custom OpenAI-compatible chat completions endpoint URL.</summary>
		[Category("AI Provider")]
		[Description("Optional custom OpenAI-compatible chat completions endpoint URL. Required for Azure OpenAI and most non-OpenAI providers.")]
		public String? ModelEndpointUrl
		{
			get => this._modelEndpointUrl;
			set
			{
				if(String.IsNullOrWhiteSpace(value))
					value = null;
				 else if(!Uri.IsWellFormedUriString(value, UriKind.Absolute))
					throw new ArgumentException("ModelEndpointUrl must be an absolute URL.", nameof(this.ModelEndpointUrl));

				this.SetField(ref this._modelEndpointUrl, value, nameof(this.ModelEndpointUrl));
			}
		}

		/// <summary>Azure OpenAI API version appended when building Azure-compatible endpoint URL.</summary>
		[Category("AI Provider")]
		[DefaultValue(Defaults.AzureApiVersion)]
		[Description("Azure OpenAI API version appended when building Azure-compatible endpoint URL.")]
		public String AzureApiVersion
		{
			get => this._azureApiVersion;
			set => this.SetField(ref this._azureApiVersion, value, nameof(this.AzureApiVersion));
		}

		/// <summary>The system prompt that defines the assistant's behavior and persona.</summary>
		[Category("AI Model Settings")]
		[DefaultValue(Defaults.AssistantSystemPrompt)]
		[Description("The system prompt that defines the assistant's behavior and persona.")]
		public String? AssistantSystemPrompt
		{
			get => this._assistantSystemPrompt;
			set
			{
				if(String.IsNullOrWhiteSpace(value))
					value = Defaults.AssistantSystemPrompt;

				this.SetField(ref this._assistantSystemPrompt, value, nameof(this.AssistantSystemPrompt));
			}
		}

		/// <summary>The sampling temperature controlling randomness in responses (0.0–2.0).</summary>
		[Category("AI Model Settings")]
		[Description("The sampling temperature controlling randomness in responses. Lower values produce more deterministic output (0.0–2.0).")]
		public Double? Temperature
		{
			get => this._temperature;
			set => this.SetField(ref this._temperature, value, nameof(this.Temperature));
		}

		/// <summary>The maximum number of tokens to generate in a single response.</summary>
		[Category("AI Model Settings")]
		[Description("The maximum number of tokens to generate in a single response. Leave empty for the model default.")]
		public Int32? MaxTokens
		{
			get => this._maxTokens;
			set
			{
				if(value == null || value <= 0)
					value = null;

				this.SetField(ref this._maxTokens, value, nameof(this.MaxTokens));
			}
		}

		/// <summary>The maximum number of agentic tool-call iterations before forcing a final response.</summary>
		[Category("AI Model Settings")]
		[DefaultValue(Defaults.AgentLoopCap)]
		[Description("The maximum number of agentic tool-call iterations before forcing a final response.")]
		public Int32 AgentLoopCap
		{
			get => this._agentLoopCap;
			set
			{
				if(value <= 0)
					value = Defaults.AgentLoopCap;
				this.SetField(ref this._agentLoopCap, value, nameof(this.AgentLoopCap));
			}
		}

		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;
		private Boolean SetField<T>(ref T field, T value, String propertyName)
		{
			if(EqualityComparer<T>.Default.Equals(field, value))
				return false;

			field = value;
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			return true;
		}
		#endregion INotifyPropertyChanged
	}
}