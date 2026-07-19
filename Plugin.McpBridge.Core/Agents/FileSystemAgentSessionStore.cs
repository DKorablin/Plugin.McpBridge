using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Extensions.AI;

namespace Plugin.McpBridge.Agents;

/// <summary>Persists agent sessions as JSON files under a root storage directory.</summary>
public class FileSystemAgentSessionStore : AgentSessionStore
{
	/// <summary>The default agent name used when no explicit agent name is provided.</summary>
	internal const String DefaultAgentName = "assistant";

	private readonly String _storageDirectory;

	public FileSystemAgentSessionStore(String? storageDirectory)
	{
		if(storageDirectory == null)
			this._storageDirectory = Utils.GetAgentStorageDirectory(Utils.SpecialDirectory.SessionStore);
		else if(String.IsNullOrWhiteSpace(storageDirectory))
			throw new ArgumentException("Invalid storage directory path.", nameof(storageDirectory));
		else if(storageDirectory.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
			throw new ArgumentException("Storage directory path contains invalid characters.", nameof(storageDirectory));
		else
			this._storageDirectory = storageDirectory;
	}

	public override async ValueTask<AgentSession> GetSessionAsync(AIAgent agent, String conversationId, CancellationToken cancellationToken = default)
	{
		var json = await this.ReadSessionAsyncI(agent.Name, conversationId, cancellationToken);
		if(json == null)
			return await agent.CreateSessionAsync(cancellationToken);

		return await agent.DeserializeSessionAsync(json.Value, cancellationToken: cancellationToken);
	}

	public override async ValueTask SaveSessionAsync(AIAgent agent, String conversationId, AgentSession session, CancellationToken cancellationToken = default)
	{
		String path = this.GetSessionPath(agent.Name, conversationId, createDirectory: true);

		JsonElement serialized = await agent.SerializeSessionAsync(session, cancellationToken: cancellationToken);
		await File.WriteAllTextAsync(path, serialized.GetRawText(), cancellationToken);
	}

	public IEnumerable<String> ListSessions(String? agentName)
	{
		String directory = this.GetAgentDirectory(agentName);
		if(!Directory.Exists(directory))
			return Array.Empty<String>();

		return Directory.EnumerateFiles(directory, "*.json").Select(Path.GetFileNameWithoutExtension);
	}

	public async IAsyncEnumerable<ChatMessage> ReadSessionAsync(String? agentName, String conversationId, CancellationToken token = default)
	{
		JsonElement? root = await this.ReadSessionAsyncI(agentName, conversationId, token);
		if(root == null)
			yield break;
		
		ChatMessage[]? messages = FileSystemAgentSessionStore.ExtractSessionMessages(root.Value);
		if(messages == null)
			yield break;
		
		foreach(ChatMessage message in messages)
			yield return message;
	}

	public Boolean DeleteSession(String? agentName, String conversationId)
	{
		String path = this.GetSessionPath(agentName, conversationId);
		if(!File.Exists(path))
			return false;

		File.Delete(path);
		return true;
	}

	private String GetAgentDirectory(String? agentName, Boolean createDirectory = false)
	{
		String? agentDirectory = agentName == null ? null : Utils.SanitizePath(agentName);

		String storageRoot = agentDirectory == null
			? Path.GetFullPath(this._storageDirectory)
			: Path.GetFullPath(Path.Combine(this._storageDirectory, agentDirectory));
		if(createDirectory)
			Directory.CreateDirectory(storageRoot);

		if(!storageRoot.EndsWith(Path.DirectorySeparatorChar))
			storageRoot += Path.DirectorySeparatorChar;

		return storageRoot;
	}

	private String GetSessionPath(String? agentName, String conversationId, Boolean createDirectory = false)
	{
		String? agentDirectory = agentName == null ? null : Utils.SanitizePath(agentName);

		String storageRoot = GetAgentDirectory(agentName, createDirectory);

		String fileName = Utils.SanitizePath(conversationId) + ".json";
		String combinedPath = agentDirectory == null
			? Path.Combine(this._storageDirectory, fileName)
			: Path.Combine(this._storageDirectory, agentDirectory, fileName);

		// Path Traversal Guard
		if(!combinedPath.StartsWith(storageRoot, StringComparison.OrdinalIgnoreCase))
			throw new ArgumentException($"Invalid conversationId: '{conversationId}'.", nameof(conversationId));

		return combinedPath;
	}

	/// <summary>Reads the raw session JSON for a given agent and conversation, or <see langword="null"/> if not found.</summary>
	private async Task<JsonElement?> ReadSessionAsyncI(String? agentName, String conversationId, CancellationToken token = default)
	{
		String path = this.GetSessionPath(agentName, conversationId);
		if(!File.Exists(path))
			return null;

		using(FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
			return await JsonSerializer.DeserializeAsync<JsonElement>(fs, cancellationToken: token);
	}

	private static ChatMessage[]? ExtractSessionMessages(JsonElement root)
	{
		// Direct session format used by single-agent path.
		if(root.TryGetProperty("stateBag", out JsonElement stateBag)
			&& stateBag.TryGetProperty("InMemoryChatHistoryProvider", out JsonElement historyState)
			&& historyState.TryGetProperty("messages", out JsonElement directMessages))
			return FileSystemAgentSessionStore.DeserializeMessages(directMessages);

		// Workflow sessions persist multiple nested checkpoints; gather all history candidates and select the richest one.
		List<ChatMessage[]> candidates = new List<ChatMessage[]>();
		FileSystemAgentSessionStore.CollectHistoryCandidates(root, candidates);
		return candidates.Count == 0
			? null
			: candidates
				.OrderByDescending(static m => m.Length)
				.FirstOrDefault();
	}

	private static void CollectHistoryCandidates(JsonElement node, List<ChatMessage[]> output)
	{
		switch(node.ValueKind)
		{
		case JsonValueKind.Object:
			foreach(JsonProperty property in node.EnumerateObject())
			{
				if(property.NameEquals("InMemoryChatHistoryProvider")
					&& property.Value.ValueKind == JsonValueKind.Object
					&& property.Value.TryGetProperty("messages", out JsonElement messagesElement))
				{
					ChatMessage[]? messages = FileSystemAgentSessionStore.DeserializeMessages(messagesElement);
					if(messages?.Length > 0)
						output.Add(messages);
				}

				FileSystemAgentSessionStore.CollectHistoryCandidates(property.Value, output);
			}
			break;
		case JsonValueKind.Array:
			foreach(JsonElement item in node.EnumerateArray())
				FileSystemAgentSessionStore.CollectHistoryCandidates(item, output);
			break;
		}
	}

	private static ChatMessage[]? DeserializeMessages(JsonElement messagesElement)
		=> messagesElement.ValueKind != JsonValueKind.Array
			? null
			: JsonSerializer.Deserialize<ChatMessage[]>(messagesElement, AIJsonUtilities.DefaultOptions);
}