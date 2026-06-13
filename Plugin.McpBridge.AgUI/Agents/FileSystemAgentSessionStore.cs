using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;

namespace Plugin.McpBridge.AgUI.Agents;

public class FileSystemAgentSessionStore : AgentSessionStore
{
	private static readonly String DefaultStoragePath = Path.Combine(AppContext.BaseDirectory, ".SessionStore");
	private static readonly Char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();

	private readonly String _storageDirectory;

	public FileSystemAgentSessionStore(String? storageDirectory = null)
	{
		this._storageDirectory = storageDirectory ?? FileSystemAgentSessionStore.DefaultStoragePath;
	}

	public override async ValueTask<AgentSession> GetSessionAsync(AIAgent agent, String conversationId, CancellationToken cancellationToken = default)
	{
		var json = await this.ReadChatHistory(agent.Name, conversationId, cancellationToken);
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

	public async Task<JsonElement?> ReadChatHistory(String? agentName, String conversationId, CancellationToken token = default)
	{
		String path = this.GetSessionPath(agentName, conversationId);
		if(!File.Exists(path))
			return null;

		using(FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
			return await JsonSerializer.DeserializeAsync<JsonElement>(fs, cancellationToken: token);
	}

	private String GetSessionPath(String? agentName, String conversationId, Boolean createDirectory = false)
	{
		String fileName = SanitizePath(conversationId) + ".json";
		String? agentDirectory = agentName == null ? null : SanitizePath(agentName);

		String storageRoot = agentDirectory == null
			? Path.GetFullPath(this._storageDirectory)
			: Path.GetFullPath(Path.Combine(this._storageDirectory, agentDirectory));
		if(createDirectory)
			Directory.CreateDirectory(storageRoot);

		if(!storageRoot.EndsWith(Path.DirectorySeparatorChar))
			storageRoot += Path.DirectorySeparatorChar;

		String combinedPath = agentDirectory == null
			? Path.Combine(this._storageDirectory, fileName)
			: Path.Combine(this._storageDirectory, agentDirectory, fileName);

		// Path Traversal Guard
		if(!combinedPath.StartsWith(storageRoot, StringComparison.OrdinalIgnoreCase))
			throw new ArgumentException($"Invalid conversationId: '{conversationId}'.", nameof(conversationId));

		return combinedPath;

		String SanitizePath(String input)
			=> new String(Array.ConvertAll(input.ToCharArray(), c => Array.IndexOf(FileSystemAgentSessionStore.InvalidFileNameChars, c) >= 0 ? '_' : c));
	}
}