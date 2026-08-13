namespace McpBridge.Core.Data;

/// <summary>Specifies the available AI provider types that can be used for generating responses.</summary>
/// <remarks>
/// Use this enumeration to select the AI service or backend for processing requests.
/// The available options include cloud-based providers, local engines, and a stub for testing purposes.
/// The choice of provider may affect capabilities, required credentials, and network connectivity.
/// </remarks>
public enum AiProviderType
{
	OpenAI,
	Azure,
	CoPilot,
	Local,
	Grok,
	Gemini,
	/// <summary>Returns scripted responses locally. No credentials or network required. Intended for UI testing.</summary>
	Stub,
}