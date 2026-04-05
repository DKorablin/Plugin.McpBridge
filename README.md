# Plugin.McpBridge

A [SAL](https://github.com/DKorablin/SystemApplicationLoader) plugin that bridges the [Model Context Protocol (MCP)](https://modelcontextprotocol.io/) with AI assistants powered by [Microsoft Semantic Kernel](https://github.com/microsoft/semantic-kernel).

## Overview

Plugin.McpBridge embeds a fully in-process MCP server and MCP client connected over named-pipe transport. The MCP server exposes all loaded SAL plugins as callable MCP tools. A Semantic Kernel–backed assistant uses those tools to answer user questions and automate plugin settings — all driven from an interactive chat panel docked inside the SAL host window.

```
User ──► PanelChat ──► AssistantAgent (Semantic Kernel)
                              │
                    ┌─────────▼─────────┐
                    │    McpBridge      │
                    │  ┌─────────────┐  │
                    │  │  MCP Server │  │  ← SAL plugins as tools
                    │  └──────┬──────┘  │
                    │  Named Pipes      │
                    │  ┌──────▼──────┐  │
                    │  │  MCP Client │  │
                    │  └─────────────┘  │
                    └───────────────────┘
```

## Features

- **In-process MCP server/client** — zero external processes; server and client communicate over anonymous named pipes in the same application.
- **SAL plugin exposure** — every loaded SAL plugin is automatically registered as an MCP tool the AI can invoke.
- **Plugin settings automation** — the assistant can list, read, and write settings of any loaded plugin on behalf of the user.
- **Multi-turn agent loop** — the assistant iterates up to a configurable cap, issuing `COMMAND:` payloads and incorporating results before delivering the final response.
- **WinForms chat panel** — a dockable chat panel accessible from *Tools → OpenAI Chat*.
- **Multiple AI providers** — OpenAI, Azure OpenAI, and any OpenAI-compatible endpoint (Qwen, Grok, Gemini, custom).

## Supported AI Providers

| `ProviderType` value | Description |
|---|---|
| `OpenAI` | OpenAI public API |
| `AzureOpenAI` | Azure OpenAI Service |
| `OpenAICompatible` | Third-party OpenAI-compatible API with a custom endpoint |
| `LocalOpenAICompatible` | Local model server (no API key required) |
| `QwenCompatible` | Alibaba Qwen API |
| `GrokCompatible` | xAI Grok API |
| `GeminiCompatible` | Google Gemini via OpenAI-compatible endpoint |
| `CustomCompatible` | Any other custom endpoint |

## Configuration

Settings are managed through the standard SAL plugin settings mechanism (right-click the plugin in the SAL Plugin Manager).

| Setting | Default | Description |
|---|---|---|
| `ProviderType` | `OpenAI` | Selects the AI provider profile. |
| `ModelId` | `gpt-4o-mini` | Model identifier or Azure deployment name. |
| `ApiKey` | *(none)* | API key for the selected provider. Not required for `LocalOpenAICompatible`. |
| `ModelEndpointUrl` | *(none)* | Custom base URL for OpenAI-compatible providers. |
| `OrganizationId` | *(none)* | Optional OpenAI organization identifier. |
| `DeploymentName` | *(none)* | Azure OpenAI deployment name. |
| `AzureApiVersion` | `2024-10-21` | Azure OpenAI REST API version. |
| `AssistantSystemPrompt` | *(see below)* | System-level instruction injected at the start of every chat session. |
| `Temperature` | *(provider default)* | Sampling temperature (0.0 – 2.0). |
| `MaxTokens` | *(provider default)* | Maximum completion tokens per request. |
| `AgentLoopCap` | `3` | Maximum number of automated command/result iterations before giving up. |
| `ConnectionTimeout` | `100` | Request timeout in seconds. |

**Default system prompt:**
> *You are a SAL automation assistant. Use available MCP tools when useful. Return clear user-facing responses, or a command payload only when automation is required.*

## Agent Commands

The assistant can automate plugin settings using structured commands:

```
COMMAND: SETTINGS LIST <plugin>
COMMAND: SETTINGS GET <plugin> <setting>
COMMAND: SETTINGS SET <plugin> <setting>=<value>
```

When the model returns one of these payloads, `AssistantAgent` executes it via `PluginSettingsHelper`, feeds the result back into the chat history, and loops until a final user-facing response is produced or `AgentLoopCap` is reached.

## Architecture

| Class | Responsibility |
|---|---|
| `Plugin` | SAL entry point; registers the *Tools → OpenAI Chat* menu item and owns the component lifecycle. |
| `McpBridge` | Creates the named-pipe transport, hosts the `Microsoft.Extensions.Hosting` MCP server, and manages the `McpClient`. Implements `IDisposable`. |
| `AssistantAgent` | Builds the Semantic Kernel `Kernel`, runs the multi-turn chat loop, and dispatches automation commands. |
| `PanelChat` | WinForms `UserControl` — the dockable chat UI. |
| `PluginSettingsHelper` | Reflection-based helper to enumerate, read, and write settings on any loaded SAL plugin. |
| `Settings` | Strongly-typed, `INotifyPropertyChanged` settings bag persisted by the SAL settings infrastructure. |

## Requirements

- .NET Framework 4.8 or .NET 8.0 (Windows)
- SAL host with `SAL.Windows` support
- NuGet packages: `Microsoft.SemanticKernel`, `Microsoft.SemanticKernel.Connectors.OpenAI`, `Microsoft.Extensions.Hosting`, `ModelContextProtocol`

## License

MIT — see [LICENSE](LICENSE) for details.