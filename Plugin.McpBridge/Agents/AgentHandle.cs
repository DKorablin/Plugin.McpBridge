using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace Plugin.McpBridge.Agents;

/// <summary>Owns the lifetime of an AI agent and its underlying client resource.</summary>
internal sealed class AgentHandle : IAsyncDisposable, IDisposable
{
	private readonly IDisposable? _syncDisposable;
	private readonly IAsyncDisposable? _asyncDisposable;

	/// <summary>The configured AI agent ready for multi-turn inference.</summary>
	internal AIAgent Agent { get; }

	/// <summary>The underlying <see cref="IChatClient"/>, or <see langword="null"/> when backed by a Copilot client.</summary>
	internal IChatClient? ChatClient { get; }

	private AgentHandle(AIAgent agent, IChatClient chatClient)
	{
		this.Agent = agent;
		this.ChatClient = chatClient;
		this._syncDisposable = chatClient;
	}

	private AgentHandle(AIAgent agent, IAsyncDisposable asyncDisposable)
	{
		this.Agent = agent;
		this._asyncDisposable = asyncDisposable;
	}

	/// <summary>Creates a handle backed by an <see cref="IChatClient"/>.</summary>
	internal static AgentHandle FromChatClient(AIAgent agent, IChatClient chatClient)
		=> new AgentHandle(agent, chatClient);

	/// <summary>Creates a handle backed by an async-disposable Copilot client.</summary>
	internal static AgentHandle FromCopilotClient(AIAgent agent, IAsyncDisposable copilotClient)
		=> new AgentHandle(agent, copilotClient);

	/// <inheritdoc/>
	public void Dispose()
		=> this._syncDisposable?.Dispose();

	/// <inheritdoc/>
	public async ValueTask DisposeAsync()
	{
		this._syncDisposable?.Dispose();
		if(this._asyncDisposable != null)
			await this._asyncDisposable.DisposeAsync();
	}
}