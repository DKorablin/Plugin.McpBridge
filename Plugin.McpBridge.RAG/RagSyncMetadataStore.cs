using Microsoft.Data.Sqlite;

namespace Plugin.McpBridge.RAGHost;

internal sealed class SyncMetadataEntry
{
	public String SourceId { get; set; } = String.Empty;
	public String SourceLink { get; set; } = String.Empty;
	public Int64 LastWriteUtcTicks { get; set; }
	public Int64 Length { get; set; }
	public String? SourceHash { get; set; }
}

internal sealed class RagSyncMetadataStore
{
	private const String MetadataTableName = "rag_sync_metadata";
	private readonly String _connectionString;

	public RagSyncMetadataStore(String connectionString)
		=> this._connectionString = connectionString;

	public async Task EnsureSchemaAsync(CancellationToken cancellationToken)
	{
		await using SqliteConnection connection = new SqliteConnection(this._connectionString);
		await connection.OpenAsync(cancellationToken);

		await using SqliteCommand cmd = connection.CreateCommand();
		cmd.CommandText = $@"
CREATE TABLE IF NOT EXISTS {MetadataTableName} (
	SourceId TEXT NOT NULL PRIMARY KEY,
	SourceLink TEXT NOT NULL,
	LastWriteUtcTicks INTEGER NOT NULL,
	Length INTEGER NOT NULL,
	SourceHash TEXT NULL
);";
		await cmd.ExecuteNonQueryAsync(cancellationToken);

		await using SqliteCommand migrationCmd = connection.CreateCommand();
		migrationCmd.CommandText = $"ALTER TABLE {MetadataTableName} ADD COLUMN SourceHash TEXT NULL;";
		try
		{
			await migrationCmd.ExecuteNonQueryAsync(cancellationToken);
		}
		catch(SqliteException ex) when(ex.SqliteErrorCode == 1 && ex.Message.Contains("duplicate column name", StringComparison.OrdinalIgnoreCase))
		{
			// Column already exists.
		}
	}

	public async Task<Dictionary<String, SyncMetadataEntry>> LoadAsync(CancellationToken cancellationToken)
	{
		Dictionary<String, SyncMetadataEntry> result = new Dictionary<String, SyncMetadataEntry>(StringComparer.OrdinalIgnoreCase);

		await using SqliteConnection connection = new SqliteConnection(this._connectionString);
		await connection.OpenAsync(cancellationToken);

		await using SqliteCommand cmd = connection.CreateCommand();
		cmd.CommandText = $"SELECT SourceId, SourceLink, LastWriteUtcTicks, Length, SourceHash FROM {MetadataTableName};";

		await using SqliteDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);
		while(await reader.ReadAsync(cancellationToken))
		{
			SyncMetadataEntry entry = new SyncMetadataEntry
			{
				SourceId = reader.GetString(0),
				SourceLink = reader.GetString(1),
				LastWriteUtcTicks = reader.GetInt64(2),
				Length = reader.GetInt64(3),
				SourceHash = reader.IsDBNull(4) ? null : reader.GetString(4),
			};
			result[entry.SourceId] = entry;
		}

		return result;
	}

	public async Task UpsertAsync(IEnumerable<SyncMetadataEntry> entries, CancellationToken cancellationToken)
	{
		await using SqliteConnection connection = new SqliteConnection(this._connectionString);
		await connection.OpenAsync(cancellationToken);

		await using SqliteTransaction transaction = connection.BeginTransaction();
		foreach(SyncMetadataEntry entry in entries)
		{
			await using SqliteCommand cmd = connection.CreateCommand();
			cmd.Transaction = transaction;
			cmd.CommandText = $@"
INSERT INTO {MetadataTableName}(SourceId, SourceLink, LastWriteUtcTicks, Length, SourceHash)
VALUES($sourceId, $sourceLink, $lastWriteUtcTicks, $length, $sourceHash)
ON CONFLICT(SourceId) DO UPDATE SET
	SourceLink = excluded.SourceLink,
	LastWriteUtcTicks = excluded.LastWriteUtcTicks,
	Length = excluded.Length,
	SourceHash = excluded.SourceHash;";
			cmd.Parameters.AddWithValue("$sourceId", entry.SourceId);
			cmd.Parameters.AddWithValue("$sourceLink", entry.SourceLink);
			cmd.Parameters.AddWithValue("$lastWriteUtcTicks", entry.LastWriteUtcTicks);
			cmd.Parameters.AddWithValue("$length", entry.Length);
			cmd.Parameters.AddWithValue("$sourceHash", (Object?)entry.SourceHash ?? DBNull.Value);
			await cmd.ExecuteNonQueryAsync(cancellationToken);
		}

		await transaction.CommitAsync(cancellationToken);
	}

	public async Task DeleteAsync(IEnumerable<String> sourceIds, CancellationToken cancellationToken)
	{
		await using SqliteConnection connection = new SqliteConnection(this._connectionString);
		await connection.OpenAsync(cancellationToken);

		await using SqliteTransaction transaction = connection.BeginTransaction();
		foreach(String sourceId in sourceIds)
		{
			await using SqliteCommand cmd = connection.CreateCommand();
			cmd.Transaction = transaction;
			cmd.CommandText = $"DELETE FROM {MetadataTableName} WHERE SourceId = $sourceId;";
			cmd.Parameters.AddWithValue("$sourceId", sourceId);
			await cmd.ExecuteNonQueryAsync(cancellationToken);
		}

		await transaction.CommitAsync(cancellationToken);
	}
}