# Plugin.McpBridge

A plugin for the SAL host application that connects an AI assistant to the SAL host using [Microsoft Agent Framework](https://github.com/microsoft/agent-framework) and native AI function calling.

[![UI Screenshot](.github/assets/UI-1-200.png)](.github/assets/UI-1.png)
[![UI Screenshot](.github/assets/UI-2-200.png)](.github/assets/UI-2.png)
[![UI Screenshot](.github/assets/UI-3-200.png)](.github/assets/UI-3.png)

## Overview

Plugin.McpBridge is a SAL plugin that gives an AI assistant live access to every plugin loaded in the SAL host.
`Microsoft.Agents.AI` (backed by `Azure.AI.OpenAI` / `Microsoft.Extensions.AI`) powers the assistant, which uses registered AI tools to inspect and automate loaded SAL plugins — all driven from an interactive chat panel docked inside the SAL host window.

```
User ──► PanelChat ──► AssistantAgent (Microsoft.Agents.AI)
                              │
                    ┌─────────▼─────────────────────────────┐
                    │  ChatClientAgent + AgentSession       │
                    │  ┌──────────────────────────────────┐ │
                    │  │  AI tools (function calling)     │ │
                    │  │  - SettingsList                  │ │
                    │  │  - SettingsGet                   │ │
                    │  │  - SettingsSet  ← confirmation   │ │
                    │  │  - MethodsList                   │ │
                    │  │  - MethodsInvoke ← confirmation  │ │
                    │  │  - SystemInformation             │ │
                    │  │  - WindowsGet   (IHostWindows)   │ │
                    │  │  - WindowClose  (IHostWindows)   │ │
                    │  └──────────────────────────────────┘ │
                    └───────────────────────────────────────┘
```

## Features

- **Native AI function calling** — the assistant invokes plugin tools directly through the model’s function-calling API; no custom text-parsing commands.
- **SAL plugin exposure** — every loaded SAL plugin is automatically discoverable by the assistant.
- **Plugin settings automation** — the assistant can list, read, and write settings of any loaded plugin on behalf of the user.
- **Plugin method invocation** — the assistant can enumerate and invoke methods exposed by any loaded SAL plugin.
- **User-confirmation gate** — any action that mutates state (`SettingsSet`, `MethodsInvoke`, `WindowClose`) requires explicit user approval via an inline confirmation strip in the chat panel before execution.
- **Persistent session** — `AgentSession` maintains the full conversation context across turns without a manual loop or iteration cap.
- **WinForms chat panel** — a dockable chat panel accessible from *Tools → OpenAI Chat*.
- **Multiple AI provider profiles** — define any number of named provider profiles (OpenAI, Azure OpenAI, Qwen, Grok, Gemini, local, custom). Switch between profiles at runtime from the chat panel's Send button dropdown — no settings dialog required.
- **Reasoning model support** — optional `ReasoningOutput` and `ReasoningEffort` controls per provider profile for models that expose chain-of-thought steps.
- **Image attachments** — paste images directly into the chat input with **Ctrl+V**; a thumbnail strip previews attached images before sending. Images are forwarded to the model as PNG data.
- **Markdown rendering** — assistant responses are rendered with headers, bold, italic, inline code, code blocks, bullet lists, and embedded base64 images.
- **Host window management** — when the SAL host implements `IHostWindows`, the assistant gains `WindowsGet` and `WindowClose` tools for listing and closing open windows.
- **Granular permission controls** — `ToolsPermission` restricts which tools are available; `PluginsPermission` restricts which plugins the assistant can interact with. Both are configurable through a checkbox UI in the property grid.
- **New Conversation button** — resets the agent session and clears the chat history without changing the active provider or settings.

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

### Global Settings

These settings apply to the plugin as a whole and are shared across all provider profiles.

| Setting | Default | Description |
|---|---|---|
| `AiProviders` | *(empty list)* | The list of AI provider profiles. Managed through the expandable collection editor in the property grid. |
| `SelectedProviderId` | *(first provider)* | The active provider profile. Shown as a dropdown of profile names in the property grid. |
| `AssistantSystemPrompt` | *(see below)* | System-level instruction injected at the start of every chat session. |
| `MaxTokens` | *(provider default)* | Maximum completion tokens per request. |
| `ConnectionTimeout` | `100s` | Request timeout for network connections to the AI provider. |
| `ToolsPermission` | *(all allowed)* | Allowlist of tool method names the assistant may use. Leave empty to allow all tools. |
| `PluginsPermission` | *(all allowed)* | Allowlist of plugin IDs the assistant may interact with. Leave empty to allow all plugins. |

### Per-Provider Settings

Each entry in `AiProviders` is an `AiProviderDto` record with the following fields. Create multiple profiles to switch between models or services without re-entering credentials.

| Setting | Default | Description |
|---|---|---|
| `ProviderType` | `OpenAI` | Selects the AI service. See [Supported AI Providers](#supported-ai-providers). |
| `ModelId` | *(none)* | Model identifier or Azure deployment name (e.g. `gpt-4o-mini`). |
| `ApiKey` | *(none)* | API key for the selected provider. Not required for `LocalOpenAICompatible`. |
| `ModelEndpointUrl` | *(none)* | Custom base URL for OpenAI-compatible providers. Required for Azure OpenAI. |
| `DeploymentName` | *(none)* | Azure OpenAI deployment name, or organization identifier for OpenAI-compatible providers. |
| `Temperature` | *(provider default)* | Sampling temperature (0.0 – 2.0). |
| `ReasoningOutput` | *(none)* | When set, includes the model's reasoning trace in the response (`Partial`, `Full`). |
| `ReasoningEffort` | *(none)* | Controls chain-of-thought reasoning effort (`Low`, `Medium`, `High`). |

**Default system prompt:**
> *You are a SAL automation assistant.
Use available MCP tools when useful.
Return clear user-facing responses, or a command payload only when automation is required.
Before using relative dates (today, yesterday, last hour), obtain the current system time from the SystemInformation tool.*

## AI Tools

The assistant interacts with SAL plugins and the host environment through AI tools registered via `AIFunctionFactory.Create()`. The model invokes them directly through the API's function-calling mechanism — no custom text parsing.

| Tool | Class | Description |
|---|---|---|
| `SettingsList` | `PluginSettingsTools` | List all settings exposed by a plugin. |
| `SettingsGet` | `PluginSettingsTools` | Read the current value of a specific setting. |
| `SettingsSet` | `PluginSettingsTools` | Update a setting value ← requires user confirmation |
| `MethodsList` | `PluginMethodsTools` | List all callable methods exposed by a plugin. |
| `MethodsInvoke` | `PluginMethodsTools` | Invoke a plugin method ← requires user confirmation |
| `SystemInformation` | `ShellTools` | Returns OS version, current date/time, and UTC offset. |
| `WindowsGet` | `WindowsTools` | List all open windows and their captions. *(requires `IHostWindows`)* |
| `WindowClose` | `WindowsTools` | Close an open window by caption ← requires user confirmation *(requires `IHostWindows`)* |

`WindowsTools` is only registered when the SAL host implements the `IHostWindows` interface. Any tool that mutates state is held until the user approves or denies it via the inline confirmation strip.

## Architecture

| Class | Responsibility |
|---|---|
| `Plugin` | SAL entry point; registers the *Tools → OpenAI Chat* menu item and owns the component lifecycle. |
| `AssistantAgent` | Builds the `ChatClientAgent` (Microsoft.Agents.AI), manages the `AgentSession`, and wires AI tools via `AIFunctionFactory`. |
| `PanelChat` | WinForms `UserControl` — the dockable chat UI with provider dropdown, attachment strip, markdown response display, and the inline confirmation bar. |
| `AiProviderDto` | Per-provider configuration record (type, model, credentials, temperature, reasoning settings). Serialised to JSON and restored across sessions. |
| `ToolsFactory` | Discovers and instantiates `AITool` objects via reflection on `[Tool]`-decorated methods; applies `ToolsPermission` and `PluginsPermission` filters. |
| `ToolFacade` | Wraps each tool with confirmation gating, structured error handling, execution tracing, and timing. |
| `ToolAttribute` | Marks a method as an AI tool and declares whether user confirmation is required before execution. |
| `PluginSettingsTools` | AI tool class — `SettingsList`, `SettingsGet`, `SettingsSet`. Reflection-based access to SAL plugin settings. |
| `PluginMethodsTools` | AI tool class — `MethodsList`, `MethodsInvoke`. Reflection-based access to SAL plugin methods. |
| `ShellTools` | AI tool class — `SystemInformation`. Returns OS version, date/time, and UTC offset. |
| `WindowsTools` | AI tool class — `WindowsGet`, `WindowClose`. Registered only when the host implements `IHostWindows`. |
| `Settings` | Strongly-typed, `INotifyPropertyChanged` settings bag (global settings) persisted by the SAL settings infrastructure. |
| `MarkdownTextBox` | `RichTextBox` subclass — renders markdown (headers, bold, italic, code blocks, lists, embedded base64 images). |
| `AttachmentsPanel` | `FlowLayoutPanel` — thumbnail strip for images pasted via Ctrl+V; auto-hides when empty. |
| `ConfirmationPanel` | Yellow warning bar shown when a tool requires approval; exposes Allow/Deny buttons and a `ConfirmationHandled` event. |

## Installation

1. Download the release archive (.zip or .nupkg).
2. Place the plugin assembly into the host application plugin directory (SAL / host supporting Windows environment):
	- [Flatbed.Dialog](https://dkorablin.github.io/Flatbed-Dialog/)
	- [Flatbed.Dialog (Lite)](https://dkorablin.github.io/Flatbed-Dialog-Lite)
	- [Flatbed.MDI](https://dkorablin.github.io/Flatbed-MDI)
	- [Flatbed.MDI (WPF)](https://dkorablin.github.io/Flatbed-MDI-Avalon)
	- [Flatbed.MDI (AvaloniaUI)](https://dkorablin.github.io/Flatbed-MDI-AvaloniaUI)
3. Restart the host application; Plugin.McpBridge should appear in the plugin list (Tools -> OpenAI Chat).