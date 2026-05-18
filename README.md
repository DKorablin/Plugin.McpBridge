# Plugin.McpBridge

A plugin for the SAL host application that connects an AI assistant to the SAL host using [Microsoft Agent Framework](https://github.com/microsoft/agent-framework) and native AI function calling.

[![UI Screenshot](.github/assets/UI-1-200.png)](.github/assets/UI-1.png)
[![UI Screenshot](.github/assets/UI-2-200.png)](.github/assets/UI-2.png)
[![UI Screenshot](.github/assets/UI-3-200.png)](.github/assets/UI-3.png)

## Overview

Plugin.McpBridge is a SAL plugin that gives an AI assistant live access to every plugin loaded in the SAL host.
`Microsoft.Agents.AI` (backed by `Azure.AI.OpenAI` / `Microsoft.Extensions.AI`) powers the assistant, which uses registered AI tools to inspect and automate loaded SAL plugins — all driven from an interactive chat panel docked inside the SAL host window.

The plugin also exposes its tool set as an MCP server (JSON-RPC 2.0 over HTTP/SSE), enabling the optional **DevUI** and **AG-UI** out-of-process web applications to connect and drive the same agent from a browser.

```
User ──► PanelChat ──► AssistantAgent (Microsoft.Agents.AI)
                              │
            ┌─────────────────▼─────────────────────┐
            │  AIAgent + AgentSession               │
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
                              │
              ┌───────────────▼─────────────────────┐
              │   McpServer (HTTP/SSE JSON-RPC 2.0) │  ← optional, localhost:5050
              └───────────────┬─────────────────────┘
                              │
                 ┌────────────┴─────────────┐
                 │                          │
        ┌────────▼─────────┐       ┌────────▼─────────┐
        │      DevUI       │       │      AG-UI       │
        │ (localhost:5051) │       │ (localhost:5052) │
        └──────────────────┘       └──────────────────┘
```

## Features

- **Native AI function calling** — the assistant invokes plugin tools directly through the model's function-calling API; no custom text-parsing commands.
- **SAL plugin exposure** — every loaded SAL plugin is automatically discoverable by the assistant.
- **Plugin settings automation** — the assistant can list, read, and write settings of any loaded plugin on behalf of the user.
- **Plugin method invocation** — the assistant can enumerate and invoke methods exposed by any loaded SAL plugin, with XML doc comments forwarded to the model as tool descriptions.
- **User-confirmation gate** — any action that mutates state (`SettingsSet`, `MethodsInvoke`, `WindowClose`) requires explicit user approval via an inline confirmation strip in the chat panel before execution.
- **Persistent session** — `AgentSession` maintains the full conversation context across turns without a manual loop or iteration cap.
- **WinForms chat panel** — a dockable chat panel accessible from *Tools → OpenAI Chat*.
- **Multiple AI provider profiles** — define any number of named provider profiles (OpenAI, Azure OpenAI, GitHub Copilot CLI, Grok, Gemini, local, stub). Switch between profiles at runtime from the chat panel's Send button dropdown — no settings dialog required.
- **GitHub Copilot CLI provider** — connect through an installed `gh copilot` CLI without managing API keys; authenticate via `GITHUB_TOKEN` or the GitHub CLI.
- **Stub provider** — returns scripted responses locally; no credentials or network required. Intended for UI testing.
- **Reasoning model support** — optional `ReasoningOutput` and `ReasoningEffort` controls per provider profile for models that expose chain-of-thought steps.
- **Image attachments** — paste images directly into the chat input with **Ctrl+V**; a thumbnail strip previews attached images before sending. Images are forwarded to the model as PNG data.
- **Markdown rendering** — assistant responses are rendered with headers, bold, italic, inline code, code blocks, bullet lists, and embedded base64 images.
- **Host window management** — when the SAL host implements `IHostWindows`, the assistant gains `WindowsGet` and `WindowClose` tools for listing and closing open windows.
- **Granular permission controls** — `ToolsPermission` restricts which tools are available; `PluginsPermission` restricts which plugins the assistant can interact with. Both are configurable through a checkbox UI in the property grid.
- **New Conversation button** — resets the agent session and clears the chat history without changing the active provider or settings.
- **MCP server** — optionally starts an embedded HTTP/SSE JSON-RPC 2.0 server (default `http://localhost:5050`) that exposes the full tool set to external MCP clients.
- **DevUI** — optional out-of-process web application for local agent diagnostics (default `http://localhost:5051`). Launched as a child process; exits when the host exits.
- **AG-UI** — optional out-of-process AG-UI–compliant web application (default `http://localhost:5052`). Fetches bridge tools from the MCP server and runs a fully web-hosted agent session.

## Supported AI Providers

| `ProviderType` value | Provider class | Description |
|---|---|---|
| `OpenAI` | `NetworkProviderDto` | OpenAI public API |
| `AzureOpenAI` | `AzureProviderDto` | Azure OpenAI Service |
| `CoPilot` | `CoPilotProviderDto` | GitHub Copilot CLI (`gh copilot`) |
| `Local` | `NetworkProviderDto` | Local OpenAI-compatible server (no API key required) |
| `Grok` | `NetworkProviderDto` | xAI Grok API |
| `Gemini` | `NetworkProviderDto` | Google Gemini via OpenAI-compatible endpoint |
| `Stub` | `StubProviderDto` | Scripted local responses — no credentials or network required |

## Configuration

Settings are managed through the standard SAL plugin settings mechanism (right-click the plugin in the SAL Plugin Manager).

### Global Settings

| Setting | Default | Description |
|---|---|---|
| `AiProviders` | *(empty list)* | The list of AI provider profiles. Managed through the expandable collection editor in the property grid. |
| `SelectedProviderId` | *(first provider)* | The active provider profile. Shown as a dropdown of profile names in the property grid. |
| `AssistantSystemPrompt` | *(see below)* | System-level instruction injected at the start of every chat session. |
| `ConnectionTimeout` | `100s` | Request timeout for network connections to the AI provider. |
| `ToolsPermission` | *(all allowed)* | Allowlist of tool method names the assistant may use. Leave empty to allow all tools. |
| `PluginsPermission` | *(all allowed)* | Allowlist of plugin IDs the assistant may interact with. Leave empty to allow all plugins. |

### MCP Server Settings

| Setting | Default | Description |
|---|---|---|
| `McpServerEnabled` | `false` | Starts the embedded MCP JSON-RPC 2.0 server over HTTP/SSE. Required by DevUI and AG-UI. |
| `McpServerUrl` | `http://localhost:5050` | Base URL the MCP server listens on. |

### DevUI Settings

| Setting | Default | Description |
|---|---|---|
| `DevUIEnabled` | `false` | Starts the DevUI out-of-process web app for local agent diagnostics. Requires `McpServerEnabled`. |
| `DevUIServerUrl` | `http://localhost:5051` | URL the DevUI web app listens on. |

### AG-UI Settings

| Setting | Default | Description |
|---|---|---|
| `AgUIEnabled` | `false` | Starts the AG-UI out-of-process web app. Requires `McpServerEnabled`. |
| `AgUIServerUrl` | `http://localhost:5052` | URL the AG-UI web app listens on. |

### Per-Provider Settings

Each entry in `AiProviders` is an `AiProviderDto` record. The concrete type depends on `ProviderType`.

#### Common fields (`AiProviderDto`)

| Setting | Default | Description |
|---|---|---|
| `ProviderType` | `OpenAI` | Selects the AI service and the concrete DTO subclass. |
| `ModelId` | *(none)* | Model identifier (e.g. `gpt-4o-mini`). |
| `ModelEndpointUrl` | *(none)* | Custom base URL. Required for Azure OpenAI; optional for OpenAI-compatible endpoints. |
| `Temperature` | *(provider default)* | Sampling temperature (0.0 – 2.0). |
| `MaxTokens` | *(provider default)* | Maximum completion tokens per request. |
| `ReasoningOutput` | *(none)* | Includes the model's reasoning trace (`Partial`, `Full`). |
| `ReasoningEffort` | *(none)* | Chain-of-thought effort level (`Low`, `Medium`, `High`). |

#### Network provider fields (`NetworkProviderDto` — OpenAI, Local, Grok, Gemini)

| Setting | Default | Description |
|---|---|---|
| `ApiKey` | *(none)* | API key. Not required for `Local`. |

#### Azure provider fields (`AzureProviderDto`)

| Setting | Default | Description |
|---|---|---|
| `ApiKey` | *(none)* | Azure OpenAI API key. |
| `DeploymentName` | *(none)* | Azure OpenAI deployment name. |

#### GitHub Copilot CLI fields (`CoPilotProviderDto`)

| Setting | Default | Description |
|---|---|---|
| `CoPilotPath` | *(auto-detect)* | Absolute path to the `gh copilot` CLI executable. Leave empty to use `COPILOT_CLI_PATH` or `PATH`. |
| `GitHubToken` | *(auto-detect)* | GitHub authentication token. Leave empty to use `GITHUB_TOKEN` or `gh auth`. |

**Default system prompt:**
> *You are a SAL automation assistant.
Use available MCP tools when useful.
Return clear user-facing responses, or a command payload only when automation is required.
Before using relative dates (today, yesterday, last hour), obtain the current system time from the SystemInformation tool.*

## AI Tools

The assistant interacts with SAL plugins and the host through AI tools registered as `AIFunction` objects. The model invokes them directly through the API's function-calling mechanism — no custom text parsing.

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

`WindowsTools` is only registered when the SAL host implements `IHostWindows`. Tool descriptions for plugin methods are enriched with XML documentation comments sourced from the plugin assemblies via `XmlReflectionReader`. Any tool that mutates state is held until the user approves or denies it via the inline confirmation strip.

## Architecture

### Core plugin

| Class | Responsibility |
|---|---|
| `Plugin` | SAL entry point; registers the *Tools → OpenAI Chat* menu item; owns the `AssistantAgent`, `McpServer`, `ProcessHost` (DevUI/AG-UI), and component lifecycle. |
| `AssistantAgent` | Multi-turn agent orchestrator. Creates an `AIAgent` (via `AgentFactory`), manages an `AgentSession`, streams responses, and surfaces the `ConfirmationRequired` event. |
| `AgentFactory` | Factory building `AgentHandle` from any `AiProviderDto`. Routes to the GitHub Copilot CLI path or the standard `IChatClient` path, then wraps via `AsAIAgent()`. |
| `AgentHandle` | Lifetime container that owns the `AIAgent` and underlying `IChatClient` (or `CopilotClient`); implements `IAsyncDisposable`. |
| `PanelChat` | WinForms `UserControl` — dockable chat UI with provider dropdown, attachment strip, streaming markdown response display, and the inline confirmation bar. |
| `Settings` | Strongly-typed, `INotifyPropertyChanged` settings bag persisted by the SAL settings infrastructure. Covers AI providers, system prompt, timeouts, permissions, MCP/DevUI/AG-UI URLs. |

### Tools

| Class | Responsibility |
|---|---|
| `ToolsFactory` | Creates the `AIFunction[]` array from discovered tool classes; applies `ToolsPermission` and `PluginsPermission` filters. |
| `ToolsDiscoveryBase` | Abstract base; reflects `[Tool]`-decorated methods and converts them to typed `AIFunction` delegates. |
| `ToolAttribute` | Marks a method as an AI tool; carries the `ConfirmationRequired` flag. |
| `ToolFacade` | Wraps an `AIFunction` with structured error handling, tracing, timing, and `destructiveHint` metadata. |
| `PluginMethodAIFunction` | Wraps an `IPluginMethodInfo` as an `AIFunction`; generates a JSON schema from method parameter types and deserialises call arguments at runtime. |
| `PluginMethodsToolsExtractor` | Discovers callable plugin methods honouring `PluginsPermission`; enriches descriptions with XML doc comments via `XmlReflectionReader`. |
| `PluginSettingsTools` | `SettingsList`, `SettingsGet`, `SettingsSet` — reflection-based access to SAL plugin settings. |
| `PluginMethodsTools` | `MethodsList`, `MethodsInvoke` — delegates to `PluginMethodAIFunction`; parses JSON arguments. |
| `ShellTools` | `SystemInformation` — returns OS version, locale date format, current time, and UTC offset. |
| `WindowsTools` | `WindowsGet`, `WindowClose` — registered only when the host implements `IHostWindows`. |
| `XmlReflectionReader` | Loads and caches XML documentation from plugin assemblies; provides method and property summaries and parameter descriptions. |

### MCP bridge

| Class | Responsibility |
|---|---|
| `McpServer` | MCP-compliant HTTP/SSE JSON-RPC 2.0 server. Exposes the in-process tool set to external MCP clients. Listens at the configured URL (default `http://localhost:5050`). |
| `McpClient` | `AIFunction` collection that connects to an MCP server via SSE. `FetchAllAsync()` discovers remote tools; `InvokeCoreAsync()` sends JSON-RPC 2.0 requests. |
| `McpSession` | Owns one SSE connection and correlates JSON-RPC responses by ID using thread-safe ID generation. |

### Out-of-process UI hosts

| Class | Responsibility |
|---|---|
| `ProcessHost` | Launches the DevUI or AG-UI child process; writes a temp JSON `ProcessConfig` file; monitors the parent PID and kills the child if the host exits. |
| `ProcessConfig` | Serialisable snapshot passed to the child process: MCP server URL, UI server URL, system instructions, permissions, connection timeout, and active provider. |

### UI components

| Class | Responsibility |
|---|---|
| `MarkdownTextBox` | `RichTextBox` subclass — renders bold, italic, inline code, fenced code blocks, headers, bullet lists, and embedded base64 images. |
| `AttachmentsPanel` | `FlowLayoutPanel` — thumbnail strip for images pasted via **Ctrl+V**; auto-hides when empty. |
| `ConfirmationPanel` | Yellow warning bar (docked to bottom) shown when a tool requires approval; exposes Allow/Deny buttons and a `ConfirmationHandled` event. |
| `BindingListConverter<T>` | `TypeConverter` that expands a `BindingList<T>` as indexed child items in the PropertyGrid. |
| `AiProviderIdConverter` | `GuidConverter` that surfaces provider names in the PropertyGrid `SelectedProviderId` dropdown. |
| `ToolsPermissionEditor` | `UITypeEditor` drop-down showing a `CheckedListBox` of discovered tools; unchecked items are blocked. |
| `PluginsPermissionEditor` | `UITypeEditor` drop-down showing a `CheckedListBox` of loaded plugins; unchecked items are blocked. |

### Data / DTOs

| Class | Responsibility |
|---|---|
| `AiProviderDto` | Base record; holds `ProviderType`, `ModelId`, `ModelEndpointUrl`, `Temperature`, `MaxTokens`, `ReasoningOutput`, `ReasoningEffort`, and a `Guid` identity. |
| `NetworkProviderDto` | Extends `AiProviderDto`; adds `ApiKey` for network-based providers. |
| `AzureProviderDto` | Extends `NetworkProviderDto`; adds `DeploymentName` for Azure OpenAI. |
| `CoPilotProviderDto` | GitHub Copilot CLI provider; adds `CoPilotPath` and `GitHubToken`. |
| `StubProviderDto` | Read-only provider for UI testing; returns scripted responses with no network calls. |
| `ToolMethodDto` | Record carrying tool metadata: `ConfirmationRequired`, `Name`, `Description`, `Function`. |
| `XmlReflectionDto` | Record holding XML doc comment data: `Summary` and `Parameters` dictionary. |

### AgUI project (`Plugin.McpBridge.AgUI`)

ASP.NET Core web application implementing the AG-UI protocol. Launched as a child process by `ProcessHost`.

| Class | Responsibility |
|---|---|
| `Program` | Loads `ProcessConfig` from a temp JSON file, fetches bridge tools from the MCP server via `McpClient.FetchAllAsync()`, creates an `AIAgent` via `AgentFactory`, and starts the ASP.NET Core host at `/agui`. |
| `ApprovalMiddleware` | Converts `ToolApprovalRequestContent` (Microsoft.Agents.AI) ↔ AG-UI tool calls. Strips interim approval messages from history to prevent protocol violations. |

### DevUI project (`Plugin.McpBridge.DevUI`)

ASP.NET Core web application hosting the Microsoft.Agents.AI DevUI diagnostic interface. Launched as a child process by `ProcessHost`. Mirrors the same startup logic as AG-UI but serves at `/devui`.

## Installation

1. Download the release archive (.zip or .nupkg).
2. Place the plugin assembly into the host application plugin directory (SAL / host supporting Windows environment):
- [Flatbed.Dialog](https://dkorablin.github.io/Flatbed-Dialog/)
- [Flatbed.Dialog (Lite)](https://dkorablin.github.io/Flatbed-Dialog-Lite)
- [Flatbed.MDI](https://dkorablin.github.io/Flatbed-MDI)
- [Flatbed.MDI (WPF)](https://dkorablin.github.io/Flatbed-MDI-Avalon)
- [Flatbed.MDI (AvaloniaUI)](https://dkorablin.github.io/Flatbed-MDI-AvaloniaUI)
3. Restart the host application; Plugin.McpBridge should appear in the plugin list (*Tools → OpenAI Chat*).