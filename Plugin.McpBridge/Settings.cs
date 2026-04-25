using System.ComponentModel;
using System.Drawing.Design;
using Microsoft.Extensions.AI;
using Plugin.EventLog.UI;

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
#if DEBUG
		/// <summary>Returns scripted responses locally. No credentials or network required. Intended for UI testing.</summary>
		Stub,
#endif
	}

	/// <summary>Configuration settings for the MCP Bridge plugin.</summary>
	public class Settings : INotifyPropertyChanged
	{
		private static class Defaults
		{
			public const AiProviderType ProviderType = AiProviderType.OpenAI;
			public const String ModelId = "gpt-4o-mini";
			public const String AssistantSystemPrompt = @"You are a SAL automation assistant.
Use available MCP tools when useful.
Return clear user-facing responses, or a command payload only when automation is required.
Before using relative dates (today, yesterday, last hour), obtain the current system time from the SystemInformation tool.";
			public static readonly TimeSpan ConnectionTimeout = TimeSpan.FromSeconds(100);
			public const Int32 MaxToolResultLength = 8000;
			/// <summary>Represents a combination of all available tool permissions.</summary>
			/// <remarks>
			/// Use this value to grant access to all tool-related operations.
			/// This is typically used when full access is required, such as for administrative users or system-level operations.
			/// </remarks>

			public const Tools ToolsPermission = Tools.SystemInformation | Tools.SettingsList | Tools.SettingsGet | Tools.SettingsSet | Tools.MethodsList | Tools.MethodsInvoke;
		}

		/// <summary>
		/// Specifies the set of tools or permissions that can be granted to a large language model (LLM) for interacting with
		/// system and plugin features.
		/// </summary>
		/// <remarks>Each value in the enumeration represents a specific capability that can be enabled for the LLM,
		/// such as accessing system information, retrieving or modifying settings, or invoking plugin methods. Multiple
		/// permissions can be combined using a bitwise OR operation to grant the LLM access to multiple tools.</remarks>
		[Flags]
		public enum Tools : UInt32
		{
			/// <summary>Represents permission for LLM to access system information such as current date and time, which can be essential for generating accurate and contextually relevant responses, especially when dealing with time-sensitive queries or when the assistant's behavior depends on the current system state.</summary>
			SystemInformation = 1 << 0,
			/// <summary>Represents permission for LLM to retrieve the list of available plugin settings and their metadata (name, description, type).</summary>
			SettingsList = 1 << 1,
			/// <summary>Represents permission to retrieve plugin settings value.</summary>
			SettingsGet = 1 << 2,
			/// <summary>
			/// Permission for LLM to modify application setting value.
			/// This is a powerful permission that allows the LLM to change the behavior of the application, so it should be granted with caution.
			/// </summary>
			SettingsSet = 1 << 3,
			/// <summary>Permission for LLM to retrieve the list of available plugins and their methods, along with method metadata (name, description, parameters).</summary>
			MethodsList = 1 << 4,
			/// <summary>
			/// Permission for LLM to invoke plugin methods.
			/// This allows the LLM to execute actions and retrieve information from plugins, enabling dynamic interactions and automation based on user queries.
			/// Like SettingsSet, this is a powerful permission that should be granted with caution, as it allows the LLM to perform operations that can affect the system or access sensitive data through plugins.
			/// </summary>
			MethodsInvoke = 1 << 5,
		}

		private AiProviderType _providerType = Defaults.ProviderType;
		private String _modelId = Defaults.ModelId;
		private String? _apiKey = null;
		private String? _modelEndpointUrl = null;
		private String? _deploymentName = null;
		private String? _assistantSystemPrompt = Defaults.AssistantSystemPrompt;
		private Double? _temperature;
		private Int32? _maxTokens;
		private TimeSpan _connectionTimeout = Defaults.ConnectionTimeout;
		private Int32 _maxToolResultLength = Defaults.MaxToolResultLength;
		private Tools _toolsPermission = Defaults.ToolsPermission;

		private ReasoningOutput? _reasoningOutput = null;
		private ReasoningEffort? _reasoningEffort = null;

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

		/// <summary>The Azure OpenAI deployment name found in Azure OpenAI Studio under Deployments or organization identifier supported by some OpenAI-compatible providers.</summary>
		[Category("AI Provider")]
		[DisplayName("Deplymanet Name / Organization ID")]
		[Description("The deployment name from Azure OpenAI Studio (Deployments section). Required when using the Azure OpenAI provider.\r\nOrganization identifier supported by some OpenAI-compatible providers.")]
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

		/// <summary>The system prompt that defines the assistant's behavior and persona.</summary>
		[Category("Prompt Settings")]
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
		[Category("Prompt Settings")]
		[Description("The sampling temperature controlling randomness in responses. Lower values produce more deterministic output (0.0–2.0).")]
		public Double? Temperature
		{
			get => this._temperature;
			set => this.SetField(ref this._temperature, value, nameof(this.Temperature));
		}

		/// <summary>The maximum number of tokens to generate in a single response.</summary>
		[Category("Prompt Settings")]
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

		/// <summary>The maximum number of characters returned by a single tool invocation result.</summary>
		[Category("Prompt Settings")]
		[DefaultValue(Defaults.MaxToolResultLength)]
		[Description("The maximum number of characters returned by a single tool invocation result. Prevents large plugin responses from exceeding the model context limit.")]
		public Int32 MaxToolResultLength
		{
			get => this._maxToolResultLength;
			set
			{
				if(value <= 0)
					value = Defaults.MaxToolResultLength;
				this.SetField(ref this._maxToolResultLength, value, nameof(this.MaxToolResultLength));
			}
		}

		[Category("Prompt Settings")]
		[Editor(typeof(ColumnEditor<Tools>), typeof(UITypeEditor))]
		[DefaultValue(Defaults.ToolsPermission)]
		[Description("Controls the permissions granted to the assistant for using various tools and accessing system information. Use bitwise combination of flags to grant multiple permissions.")]
		public Tools ToolsPermission
		{
			get => this._toolsPermission;
			set
			{
				if((value & ~Defaults.ToolsPermission) != 0 || value == (Tools)0)
					value = Defaults.ToolsPermission;

				this.SetField(ref this._toolsPermission, value, nameof(this.ToolsPermission));
			}
		}

		[Category("Debugging")]
		[Description("When enabled, the plugin will include the reasoning steps taken by the assistant in the response. This can be useful for debugging and understanding how the assistant arrived at its conclusions.")]
		public ReasoningOutput? ReasoningOutput
		{
			get => this._reasoningOutput;
			set
			{
				if(value == Microsoft.Extensions.AI.ReasoningOutput.None)
					value = null;

				this.SetField(ref this._reasoningOutput, value, nameof(this.ReasoningOutput));
			}
		}

		[Category("Debugging")]
		[Description("Controls the level of effort the assistant should use when reasoning through a problem.")]
		public ReasoningEffort? ReasoningEffort
		{
			get => this._reasoningEffort;
			set
			{
				if(value == Microsoft.Extensions.AI.ReasoningEffort.None)
					value = null;
				this.SetField(ref this._reasoningEffort, value, nameof(this.ReasoningEffort));
			}
		}

		[Category("Network")]
		[DefaultValue(typeof(TimeSpan), "00:01:40")]
		[Description("The timeout duration for network connections to the AI provider.")]
		public TimeSpan ConnectionTimeout
		{
			get => this._connectionTimeout;
			set
			{
				if(value <= TimeSpan.Zero)
					value = Defaults.ConnectionTimeout;
				this.SetField(ref this._connectionTimeout, value, nameof(this.ConnectionTimeout));
			}
		}

		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler? PropertyChanged;
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