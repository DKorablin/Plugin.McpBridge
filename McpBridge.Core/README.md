# McpBridge.Core

The provider-agnostic core library behind [Plugin.McpBridge](https://github.com/DKorablin/Plugin.McpBridge): AI agent orchestration, MCP client/server primitives, RAG indexing, and DTOs built on [Microsoft.Agents.AI](https://github.com/microsoft/agent-framework). It has no dependency on SAL or WinForms and can be reused by any host that wants to build an `Microsoft.Agents.AI` agent, talk MCP, or run the RAG sidecar pipeline.

## Overview

`McpBridge.Core` factors out everything in the MCP bridge that isn't specific to the SAL WinForms plugin: agent construction, provider configuration, settings persistence, MCP client/session transport, RAG text-search storage, out-of-process host launching, and the reflection-based tool discovery framework. [Plugin.McpBridge](https://github.com/DKorablin/Plugin.McpBridge), the DevUI host, the AG-UI host, and the RAG sidecar all consume this package instead of duplicating the logic.

## What's included

### Agents (`Agents/`)

| Class | Responsibility |
|---|---|
| `AgentFactory` | Builds an agent from a provider profile, attaches tools, and wires skills/RAG/context providers. Shared by the WinForms chat path, DevUI, and AG-UI. |
| `AssistantAgent` | Multi-turn agent orchestrator that streams responses and raises confirmation events for state-changing tool calls. |
| `AgentHandle` | Lifetime container for a built agent and its underlying chat client. |
| `FileSystemAgentSessionStore` | Persists agent chat session state to disk. |
| `StubChatClient` | Scripted, offline-friendly `IChatClient` implementation used by the `Stub` provider. |

### Provider and agent configuration (`Data/`)

| Class | Responsibility |
|---|---|
| `AiAgentDto` | Per-agent configuration: prompt, permissions, skills directory, RAG settings, provider selection. |
| `AiProviderDto` / `AiProviderType` / `ProviderCapabilities` / `ProviderSettings` | Base provider settings, supported provider families, and capability flags (chat vs. embeddings). |
| `NetworkProviderDto` | Settings for OpenAI-compatible network endpoints (OpenAI, Local, Grok, Gemini). |
| `AzureProviderDto` | Azure OpenAI Service settings. |
| `CoPilotProviderDto` | GitHub Copilot CLI (`gh copilot`) settings. |
| `StubProviderDto` | Read-only stub provider for local/offline testing. |
| `ToolMethodDto` / `XmlReflectionDto` | Metadata describing a reflected tool method, including XML doc comments. |

### MCP transport (`Mcp/`)

| Class | Responsibility |
|---|---|
| `McpClient` | Maps a remote MCP tool into an `AIFunction` so it can be added to an agent's tool set. |
| `McpSession` | Manages one SSE connection and correlates JSON-RPC 2.0 requests/responses. |

### RAG (`RAG/`)

| Class | Responsibility |
|---|---|
| `TextSearchStore` / `TextSearchLazyStore` | SQLite-vector-backed store for indexed RAG documents, used for retrieval-augmented generation. |
| `TextSearchDocument` / `TextSearchRecord` | Document and vector record shapes persisted by the store. |
| `EmbeddingModel` | Embedding generation abstraction used when indexing and querying RAG content. |

### Tool discovery framework (`Tools/`)

| Class | Responsibility |
|---|---|
| `ToolsDiscoveryBase` | Base class for reflection-based discovery of callable tool methods. |
| `ToolsFactory` | Builds the `AIFunction[]` tool set and applies permission allowlists. |
| `ToolAttribute` | Marks a method as an agent-callable tool. |
| `ShellTools` | Host/system information tools (OS version, current date/time, UTC offset). |

### Out-of-process hosting (`Remoting/`)

| Class | Responsibility |
|---|---|
| `ProcessHost` | Launches a companion executable (DevUI, AG-UI, or RAG sidecar) as a child process and stops it when the parent exits. |
| `SettingsDto` | Serializable settings snapshot passed to the child process at startup. |
| `ProcessType` | Identifies which companion process is being hosted. |

### Workflows (`Workflows/`)

| Class | Responsibility |
|---|---|
| `WorkflowLoader2` | Loads workflow definitions from a directory of JSON files. |
| `WorkflowFactoryItem` / `WorkflowHandle` / `WorkflowDto` | Workflow metadata and runtime handles built on `Microsoft.Agents.AI.Workflows` (sequential, concurrent, group-chat, handoff, magentic, conditional graphs). |

### Settings, events, and shared UI editors

| Class | Responsibility |
|---|---|
| `Settings` | Persisted settings bag for providers, agents, permissions, MCP, DevUI, AG-UI, and workflow configuration. |
| `AgentEventArgs` | Event payload raised by agents (streaming updates, confirmation requests). |
| `IMcpTrace` | Tracing abstraction implemented by hosts to receive diagnostic events from the core library. |
| `UI/PropertyGrid/Converters/*` | Shared `TypeConverter`/UI editors (tools permission list, session storage directory, evaluation cache directory, binding list) reused by any host that exposes `Settings` in a property grid. |

## Dependencies

Built for `net8.0`. Pulls in `Azure.AI.OpenAI`, `Microsoft.Agents.AI` (+ `.DurableTask`, `.Hosting`, `.Workflows`, `.GitHub.Copilot`), `Microsoft.Extensions.AI.OpenAI`, `Microsoft.Extensions.AI.Evaluation.Reporting`, `Microsoft.SemanticKernel.Connectors.InMemory`, `Microsoft.SemanticKernel.Connectors.SqliteVec`, and the Durable Task gRPC client/worker packages. It does not reference `SAL.Windows`, WinForms, or any SAL plugin host API.

## Consumers

- [Plugin.McpBridge](https://github.com/DKorablin/Plugin.McpBridge) — the SAL WinForms plugin; adds the dockable chat panel, SAL plugin/settings/window tools, and the embedded MCP server.
- `Plugin.McpBridge.DevUI` — diagnostics host.
- `Plugin.McpBridge.AgUI` — AG-UI web host.
- `Plugin.McpBridge.RAG` — RAG sidecar process that keeps per-agent SQLite vector indexes in sync with configured RAG folders.

See the [Plugin.McpBridge README](https://github.com/DKorablin/Plugin.McpBridge/blob/master/README.md) for end-user installation, configuration, and feature documentation.
