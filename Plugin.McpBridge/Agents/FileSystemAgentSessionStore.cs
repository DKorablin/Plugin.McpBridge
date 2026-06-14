using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;

namespace Plugin.McpBridge.Agents;

/// <summary>Persists agent sessions as JSON files under a root storage directory.</summary>
public class FileSystemAgentSessionStore : AgentSessionStore
{
	/// <summary>The default agent name used when no explicit agent name is provided.</summary>
	internal const String DefaultAgentName = "assistant";

	private static readonly Char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();

	private readonly String _storageDirectory;

	public FileSystemAgentSessionStore(String storageDirectory)
		=> this._storageDirectory = storageDirectory ?? throw new ArgumentNullException(nameof(storageDirectory));

	public override async ValueTask<AgentSession> GetSessionAsync(AIAgent agent, String conversationId, CancellationToken cancellationToken = default)
	{
		var json = await this.ReadSessionAsync(agent.Name, conversationId, cancellationToken);
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

	/// <summary>Reads the raw session JSON for a given agent and conversation, or <see langword="null"/> if not found.</summary>
	public async Task<JsonElement?> ReadSessionAsync(String? agentName, String conversationId, CancellationToken token = default)
	{
		String path = this.GetSessionPath(agentName, conversationId);
		if(!File.Exists(path))
			return null;

		using(FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
			return await JsonSerializer.DeserializeAsync<JsonElement>(fs, cancellationToken: token);
	}

	public IEnumerable<String> ListSessions(String? agentName)
	{
		String directory = this.GetAgentDirectory(agentName);
		if(!Directory.Exists(directory))
			return Array.Empty<String>();

		return Directory.EnumerateFiles(directory, "*.json").Select(Path.GetFileNameWithoutExtension);
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
		String? agentDirectory = agentName == null ? null : SanitizePath(agentName);

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
		String? agentDirectory = agentName == null ? null : SanitizePath(agentName);

		String storageRoot = GetAgentDirectory(agentName, createDirectory);

		String fileName = SanitizePath(conversationId) + ".json";
		String combinedPath = agentDirectory == null
			? Path.Combine(this._storageDirectory, fileName)
			: Path.Combine(this._storageDirectory, agentDirectory, fileName);

		// Path Traversal Guard
		if(!combinedPath.StartsWith(storageRoot, StringComparison.OrdinalIgnoreCase))
			throw new ArgumentException($"Invalid conversationId: '{conversationId}'.", nameof(conversationId));

		return combinedPath;
	}

	private static String SanitizePath(String input)
		=> new String(Array.ConvertAll(input.ToCharArray(), c => Array.IndexOf(FileSystemAgentSessionStore.InvalidFileNameChars, c) >= 0 ? '_' : c));
}