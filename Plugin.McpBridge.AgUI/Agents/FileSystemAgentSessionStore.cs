using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;

namespace Plugin.McpBridge.AgUI.Agents;

public class FileSystemAgentSessionStore : AgentSessionStore
{
	private static readonly String DefaultStoragePath = Path.Combine(AppContext.BaseDirectory, "SessionStore");
	private static readonly Char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();

	private readonly String _storageDirectory;

	public FileSystemAgentSessionStore(String? storageDirectory = null)
	{
		this._storageDirectory = storageDirectory ?? FileSystemAgentSessionStore.DefaultStoragePath;
	}

	public override async ValueTask<AgentSession> GetSessionAsync(AIAgent agent, String conversationId, CancellationToken cancellationToken = default)
	{
		String path = this.GetSessionPath(conversationId);
		if(!File.Exists(path))
			return await agent.CreateSessionAsync(cancellationToken).ConfigureAwait(false);

		Byte[] bytes = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
		if(bytes.Length == 0)
			return await agent.CreateSessionAsync(cancellationToken).ConfigureAwait(false);

		using JsonDocument document = JsonDocument.Parse(bytes);
		JsonElement element = document.RootElement.Clone();
		return await agent.DeserializeSessionAsync(element, cancellationToken: cancellationToken).ConfigureAwait(false);
	}

	public override async ValueTask SaveSessionAsync(AIAgent agent, String conversationId, AgentSession session, CancellationToken cancellationToken = default)
	{
		Directory.CreateDirectory(this._storageDirectory);
		String path = this.GetSessionPath(conversationId);
		JsonElement serialized = await agent.SerializeSessionAsync(session, cancellationToken: cancellationToken).ConfigureAwait(false);
		await File.WriteAllTextAsync(path, serialized.GetRawText(), cancellationToken).ConfigureAwait(false);
	}

	private String GetSessionPath(String conversationId)
	{
		String sanitized = new String(conversationId.Select(c => Array.IndexOf(FileSystemAgentSessionStore.InvalidFileNameChars, c) >= 0 ? '_' : c).ToArray());
		String storageRoot = Path.GetFullPath(this._storageDirectory);
		if(!storageRoot.EndsWith(Path.DirectorySeparatorChar))
			storageRoot += Path.DirectorySeparatorChar;
		String fullPath = Path.GetFullPath(Path.Combine(this._storageDirectory, sanitized + ".json"));
		if(!fullPath.StartsWith(storageRoot, StringComparison.OrdinalIgnoreCase))
			throw new ArgumentException($"Invalid conversationId: '{conversationId}'.", nameof(conversationId));
		return fullPath;
	}
}