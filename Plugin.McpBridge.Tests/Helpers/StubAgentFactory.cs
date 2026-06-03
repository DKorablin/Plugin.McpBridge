using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Plugin.McpBridge.Agents;
using Plugin.McpBridge.Data.AiProvider;

namespace Plugin.McpBridge.Tests.Helpers;

/// <summary>Test double for <see cref="AgentFactory"/> that uses a supplied <see cref="IChatClient"/> instead of a real provider.</summary>
internal sealed class StubAgentFactory : AgentFactory
{
	private readonly IChatClient _client;

	internal StubAgentFactory(IChatClient client) => this._client = client;

	public override Task<AgentHandle> CreateAgent(AiProviderDto providerSettings, TimeSpan connectionTimeout, AIFunction[] tools, String systemInstructions, CancellationToken token = default)
		=> Task.FromResult(AgentHandle.FromChatClient(
			this._client.AsAIAgent(instructions: systemInstructions, tools: tools, name: "stub"),
			this._client));
}
