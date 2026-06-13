# Plugin.McpBridge

A SAL plugin that connects an AI assistant to the host application using [Microsoft.Agents.AI](https://github.com/microsoft/agent-framework) and native function calling.

[![UI Screenshot](.github/assets/UI-1-200.png)](.github/assets/UI-1.png)
[![UI Screenshot](.github/assets/UI-2-200.png)](.github/assets/UI-2.png)
[![UI Screenshot](.github/assets/UI-3-200.png)](.github/assets/UI-3.png)

## Overview

Plugin.McpBridge gives an AI assistant live access to loaded SAL plugins, their settings, and their callable methods from an interactive WinForms chat panel docked inside the host window.

The core agent uses `Microsoft.Agents.AI` with `Azure.AI.OpenAI` and `Microsoft.Extensions.AI` backends. It can also expose the same tool set over MCP so the optional DevUI and AG-UI web hosts can connect to the bridge from outside the desktop process.

Per-agent configuration now goes beyond provider selection: each agent can have its own system prompt, skills directory, RAG knowledge base, tool/plugin permissions, and selected provider profile.

```
User -> PanelChat -> AssistantAgent (Microsoft.Agents.AI)
          |
          +-> AgentSession (persistent chat state)
          |
          +-> Tools
            - SettingsList / SettingsGet / SettingsSet
            - MethodsList / MethodsInvoke
            - SystemInformation
            - WindowsGet / WindowClose (when IHostWindows is available)
            - MCP bridge tools
          |
          +-> Optional context providers
            - SkillsDirectory
            - RAG knowledge base
            - Workflows loaded from JSON
          |
          +-> MCP server (http://localhost:5050)
            +-> DevUI (http://localhost:5051)
            +-> AG-UI (http://localhost:5052)
```

## Features

- Native AI function calling through `Microsoft.Agents.AI`; no custom text parsing layer.
- Multiple AI agent profiles in one plugin, each with its own provider, prompt, permissions, skills directory, and optional RAG knowledge base.
- Runtime agent and provider switching from the chat panel Send dropdown.
- Automatic discovery of loaded SAL plugins, their settings, and their callable methods.
- Setting updates and plugin method invocations require explicit user approval before execution.
- Persistent chat sessions in the WinForms panel, plus a New Conversation button to reset history.
- Image and file paste support in the chat input with `Ctrl+V`.
- Markdown rendering for assistant responses, including headings, formatting, code blocks, lists, and embedded images.
- Host window tools (`WindowsGet` and `WindowClose`) when the SAL host implements `IHostWindows`.
- Permission allowlists for both tool usage and plugin access.
- Embedded MCP server over HTTP/SSE JSON-RPC 2.0 for external MCP clients.
- Optional DevUI diagnostics host launched as a child process.
- Optional AG-UI host launched as a child process with optional session persistence.
- Optional RAG sidecar host that keeps per-agent SQLite vector indexes synchronized with RAG folders.
- Workflow loading from a configured directory of JSON files, using Microsoft.Agents.AI.Workflows.
- GitHub Copilot CLI provider support through `gh copilot`.
- Stub provider for offline UI testing.

## Supported AI Providers

| `ProviderType` value | Provider class | Notes |
|---|---|---|
| `OpenAI` | `NetworkProviderDto` | OpenAI public API or any OpenAI-compatible endpoint. |
| `Azure` | `AzureProviderDto` | Azure OpenAI Service. |
| `CoPilot` | `CoPilotProviderDto` | GitHub Copilot CLI (`gh copilot`). |
| `Local` | `NetworkProviderDto` | Local OpenAI-compatible server; no API key required. |
| `Grok` | `NetworkProviderDto` | xAI Grok-compatible endpoint via OpenAI-compatible API surface. |
| `Gemini` | `NetworkProviderDto` | Gemini-compatible endpoint via OpenAI-compatible API surface. |
| `Stub` | `StubProviderDto` | Scripted local responses for UI testing. |

## Configuration

Settings are managed through the standard SAL plugin settings UI. Most values are exposed in the property grid with rich editors and dropdowns.

### Agent Settings

Each entry in `AiAgents` is an `AiAgentDto` record. The chat panel can switch between agents at runtime.

| Setting | Default | Description |
|---|---|---|
| `SelectedProviderId` | *(first provider)* | Chat provider profile for this agent. |
| `EmbeddingProviderId` | *(same as `SelectedProviderId`)* | Optional embeddings provider profile for this agent's RAG indexing/querying. |
| `AssistantSystemPrompt` | *(see below)* | System prompt injected into the agent session. |
| `SkillsDirectory` | *(none)* | Optional directory of skills files that the agent can read and write. |
| `RagToolName` | *(none)* | Name of the generated RAG search tool. |
| `RagToolDescription` | *(none)* | Description for the RAG search tool. |
| `RagSupportedExtensions` | `.txt`, `.md` | Configurable list of supported RAG file extensions. Duplicate entries are removed automatically; invalid entries throw validation errors in the settings model. |
| `RagDirectory` | *(none)* | Optional knowledge base directory. Files are indexed only when their extension matches `RagSupportedExtensions`. |
| `RagCitationsPrompt` | *(none)* | Optional citation guidance appended to RAG responses. |
| `ToolsPermission` | *(all allowed)* | Allowlist of tool method names the agent may use. |
| `PluginsPermission` | *(all allowed)* | Allowlist of plugin IDs the agent may use. |

### Global Settings

| Setting | Default | Description |
|---|---|---|
| `AiProviders` | *(empty list)* | Provider profiles available to all agents. |
| `AiAgents` | *(one default agent)* | Collection of agent profiles. |
| `SelectedAgentId` | *(first agent)* | Active agent profile used by the WinForms panel and child hosts. |
| `WorkflowsDirectory` | *(none)* | Optional directory of workflow JSON definitions loaded at runtime. |
| `McpServerEnabled` | `false` | Starts the embedded MCP server. Automatically enabled when DevUI or AG-UI is enabled. |
| `McpServerUrl` | `http://localhost:5050` | URL used by the MCP server. |
| `DevUIEnabled` | `false` | Starts the DevUI child process for diagnostics. Requires the DevUI executable beside the plugin assembly. |
| `DevUIServerUrl` | `http://localhost:5051` | URL used by DevUI. |
| `AgUIEnabled` | `false` | Starts the AG-UI child process for web-hosted interaction. Requires the AG-UI executable beside the plugin assembly. |
| `AgUIServerUrl` | `http://localhost:5052` | URL used by AG-UI. |
| `AgUISessionStorageDirectory` | *(none)* | Optional directory for persistent AG-UI session storage. |
| `RagProcessEnabled` | `false` | Starts the RAG sidecar child process that performs full and incremental sync of configured RAG folders into local SQLite vector indexes. |

### Provider Settings

Each provider entry can expose different fields depending on its concrete type.

#### Common provider fields (`AiProviderDto`)

| Setting | Default | Description |
|---|---|---|
| `Description` | *(none)* | Optional label shown in provider pickers. |
| `ProviderType` | `OpenAI` | Provider family used to instantiate the client. |
| `Capabilities` | `Chat, Embeddings` | Enabled capability flags for this provider profile. Provider selection is filtered by capability (chat vs embeddings). |
| `Connection.Timeout` | `100s` | Request timeout for network/process connections. |
| `Chat.ModelId` | *(none)* | Chat model or deployment name. Required when chat capability is enabled (except `Stub`/`CoPilot`). |
| `Connection.EndpointUrl` | *(none)* | Optional absolute endpoint URL for compatible providers; required for Azure and most self-hosted endpoints. |
| `Temperature` | *(provider default)* | Sampling temperature. |
| `MaxTokens` | *(provider default)* | Maximum completion tokens per request. |
| `Chat.ReasoningOutput` | *(none)* | Optional reasoning trace output. |
| `Chat.ReasoningEffort` | *(none)* | Optional reasoning effort level. |
| `Embeddings.ModelId` | *(none)* | Embedding model/deployment name. Required when embeddings capability is enabled. |
| `Embeddings.Dimension` | *(inferred when possible)* | Embedding vector size used by the RAG store. |
| `Embeddings.TopResults` | `3` | Number of top embedding matches returned by the RAG text search provider. |

#### Network provider fields (`NetworkProviderDto`)

| Setting | Default | Description |
|---|---|---|
| `Connection.ApiKey` | *(none)* | API key for network-based providers. Not required for `Local`. |

#### Azure provider fields (`AzureProviderDto`)

| Setting | Default | Description |
|---|---|---|
| `Connection.EndpointUrl` | *(required)* | Azure OpenAI endpoint URL. |
| `Connection.ApiKey` | *(required)* | Azure OpenAI API key. |
| `Chat.ModelId` | *(required)* | Azure deployment/model name used for chat. |

#### GitHub Copilot CLI fields (`CoPilotProviderDto`)

| Setting | Default | Description |
|---|---|---|
| `Connection.CoPilotPath` | *(auto-detect)* | Absolute path to the `gh copilot` CLI executable. |
| `Connection.GitHubToken` | *(auto-detect)* | GitHub token used by the Copilot CLI. |

#### Provider validation behavior

- Provider objects are validated before agent creation and before embedding generator creation.
- Validation errors include actionable messages such as `Chat capability is disabled`, `Chat model is required`, or `Embedding model is required`.
- `CoPilot` and `Stub` are chat-only providers and cannot be selected for embeddings.

**Default system prompt:**

> You are a SAL automation assistant.
> Use available MCP tools when useful.
> Return clear user-facing responses, or a command payload only when automation is required.
> Before using relative dates (today, yesterday, last hour), obtain the current system time from the SystemInformation tool.

## AI Tools

The assistant interacts with SAL plugins and the host through `AIFunction` tools registered by the bridge.

| Tool | Class | Description |
|---|---|---|
| `SettingsList` | `PluginSettingsTools` | List all settings exposed by a plugin. |
| `SettingsGet` | `PluginSettingsTools` | Read the current value of a specific setting. |
| `SettingsSet` | `PluginSettingsTools` | Update a setting value; requires user confirmation. |
| `MethodsList` | `PluginMethodsTools` | List all callable methods exposed by a plugin. |
| `MethodsInvoke` | `PluginMethodsTools` | Invoke a plugin method; requires user confirmation. |
| `SystemInformation` | `ShellTools` | Returns OS version, current date/time, and UTC offset. |
| `WindowsGet` | `WindowsTools` | List open windows and their captions. Requires `IHostWindows`. |
| `WindowClose` | `WindowsTools` | Close a window by caption; requires user confirmation and `IHostWindows`. |

Tool descriptions for plugin methods are enriched from XML documentation comments when available. Any tool that mutates state stays behind the inline confirmation bar until the user approves it.

## Architecture

### Core plugin

| Class | Responsibility |
|---|---|
| `Plugin` | SAL entry point; registers the `Tools -> OpenAI Chat` menu item and manages the agent, MCP server, and child-process hosts. |
| `AssistantAgent` | Multi-turn agent orchestrator that streams responses and raises confirmation events. |
| `AgentFactory` | Builds agents from any supported provider, attaches tools, and wires skills, RAG, and workflows. |
| `AgentHandle` | Lifetime container for the built agent and underlying client. |
| `PanelChat` | Dockable WinForms chat UI with agent/provider switching, attachments, streaming markdown, and confirmations. |
| `Settings` | Persisted settings bag for providers, agents, permissions, MCP, DevUI, AG-UI, and workflow configuration. |

### Tools and discovery

| Class | Responsibility |
|---|---|
| `ToolsFactory` | Builds the `AIFunction[]` tool set and applies permission filters. |
| `PluginSettingsTools` | Reflection-based read/write access to SAL plugin settings. |
| `PluginMethodsTools` | Lists and invokes callable plugin methods. |
| `PluginMethodsToolsExtractor` | Discovers callable plugin methods and applies XML doc metadata. |
| `ShellTools` | Exposes host/system information. |
| `WindowsTools` | Exposes host window inspection and closing when supported. |

### MCP bridge

| Class | Responsibility |
|---|---|
| `McpServer` | Exposes the in-process tool set over HTTP/SSE JSON-RPC 2.0. |
| `McpClient` | Connects to a remote MCP server and maps remote tools back into `AIFunction` objects. |
| `McpSession` | Manages one SSE connection and correlates JSON-RPC responses. |

### Out-of-process hosts

| Class | Responsibility |
|---|---|
| `ProcessHost` | Launches DevUI, AG-UI, or RAG sidecar as a child process and stops it when the host exits. |
| `SettingsDto` | Serializable config passed to the child process at startup. |

### RAG sidecar project (`Plugin.McpBridge.RAG`)

When `RagProcessEnabled` is true, a separate RAG sidecar process is launched and keeps per-agent vector indexes in sync.

- Performs a full sync at startup for each configured agent RAG folder.
- Watches RAG folders recursively for the extensions configured in each agent's `RagSupportedExtensions` list.
- Debounces file events and applies incremental add/update/remove operations.
- Uses file metadata plus SHA-256 hashes to skip unchanged content and avoid unnecessary embedding calls.
- Writes per-agent SQLite vector databases under the plugin's `.RagStore` directory.

### UI components

| Class | Responsibility |
|---|---|
| `MarkdownTextBox` | Renders assistant responses with markdown formatting and embedded images. |
| `AttachmentsPanel` | Manages pasted attachments in the chat UI. |
| `ConfirmationPanel` | Inline approval bar for state-changing tool calls. |
| `ToolsPermissionEditor` | Checkbox editor for tool allowlists. |
| `PluginsPermissionEditor` | Checkbox editor for plugin allowlists. |

### Data and DTOs

| Class | Responsibility |
|---|---|
| `AiAgentDto` | Per-agent configuration, including prompt, permissions, skills, and RAG settings. |
| `AiProviderDto` | Base provider settings. |
| `NetworkProviderDto` | Network provider settings plus timeouts and embeddings. |
| `AzureProviderDto` | Azure OpenAI-specific settings. |
| `CoPilotProviderDto` | GitHub Copilot CLI settings. |
| `StubProviderDto` | Read-only stub provider for local testing. |

### Workflow loading

`WorkflowsDirectory` can point to a folder of JSON workflow definitions. At runtime the bridge can load sequential, concurrent, group-chat, handoff, magentic, and conditional graph workflows through `Microsoft.Agents.AI.Workflows`.

### AG-UI project (`Plugin.McpBridge.AgUI`)

The AG-UI project is an ASP.NET Core host launched as a child process. It connects to the embedded MCP server, builds an agent from the selected agent profile, loads any workflows from `WorkflowsDirectory`, and serves the web UI at `/agui`.

When `AgUISessionStorageDirectory` is set, AG-UI persists serialized sessions to disk and enables `GET /history/{threadId}` for restoring conversation history in the web client.
The session store is configured with shared-session mode (`withIsolation: false`), so connected clients see real-time updates to the same conversation.

### DevUI project (`Plugin.McpBridge.DevUI`)

The DevUI project is a separate ASP.NET Core host for diagnostics. It is launched as a child process and uses the same bridge settings and MCP-backed tool set.

## Installation

1. Build the solution for `net8.0-windows` or use a release package.
2. Copy `Plugin.McpBridge.dll` into the host application's plugin directory.
3. Copy `Plugin.McpBridge.DevUI.exe`, `Plugin.McpBridge.AgUI.exe`, and `Plugin.McpBridge.RAG.exe` beside the plugin assembly if you want DevUI, AG-UI, or the RAG sidecar to launch.
4. Choose one of the following host applications to run this plugin:
  - [Flatbed.Dialog (Lite)](https://dkorablin.github.io/Flatbed-Dialog-Lite)
  - [Flatbed.MDI](https://dkorablin.github.io/Flatbed-MDI)
  - [Flatbed.MDI (WPF)](https://dkorablin.github.io/Flatbed-MDI-Avalon)
  - [Flatbed.MDI (AvaloniaUI)](https://dkorablin.github.io/Flatbed-MDI-AvaloniaUI)
5. Configure agents, providers, permissions, and optional RAG or workflow folders from the SAL [plugin settings UI](https://github.com/DKorablin/Plugin.Configuration).
6. Plugin.McpBridge should appear in the plugin list (*Tools → OpenAI Chat*).

## Notes

- `McpServerEnabled` is automatically treated as enabled when DevUI or AG-UI is enabled.
- The default MCP/DevUI/AG-UI URLs are `http://localhost:5050`, `http://localhost:5051`, and `http://localhost:5052`.
- AG-UI session persistence and the `/history/{threadId}` endpoint are available only when `AgUISessionStorageDirectory` is configured.
- RAG sync sidecar requires `RagProcessEnabled = true` and `Plugin.McpBridge.RAG.exe` beside the plugin assembly.
- RAG knowledge bases default to `.txt` and `.md`, but each agent can override the supported extension list with `RagSupportedExtensions`.
