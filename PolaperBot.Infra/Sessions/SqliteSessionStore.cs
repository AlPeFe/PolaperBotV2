using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using PolaperBot.Core.AI.Sessions;

namespace PolaperBot.Infra.Sessions;

public class SqliteSessionStore : ISessionStore
{
    private readonly string _connectionString;
    private readonly AIAgent _agent;
    private readonly ILogger<SqliteSessionStore> _logger;

    public SqliteSessionStore(AIAgent agent, string dbPath, ILogger<SqliteSessionStore> logger)
    {
        _agent = agent;
        _logger = logger;
        _connectionString = $"Data Source={dbPath}";
        InitializeDb();
    }

    private void InitializeDb()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS Sessions (
                UserId INTEGER PRIMARY KEY,
                SessionJson TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL
            )
            """;
        cmd.ExecuteNonQuery();
        _logger.LogInformation("Sessions table initialized");
    }

    public async Task<AgentSession> LoadOrCreateAsync(long userId, CancellationToken cancellationToken = default)
    {
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT SessionJson FROM Sessions WHERE UserId = $userId";
        cmd.Parameters.AddWithValue("$userId", userId);

        var json = (string?)await cmd.ExecuteScalarAsync(cancellationToken);

        if (json is null)
        {
            var newSession = await _agent.CreateSessionAsync();
            _logger.LogInformation("Created new session for user {UserId}", userId);
            return newSession;
        }

        try
        {
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);
            var session = await _agent.DeserializeSessionAsync(jsonElement);
            _logger.LogDebug("Loaded existing session for user {UserId}", userId);
            return session;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize session for user {UserId}, creating new", userId);
            return await _agent.CreateSessionAsync();
        }
    }

    public async Task SaveAsync(long userId, AgentSession session, CancellationToken cancellationToken = default)
    {
        var serialized = await _agent.SerializeSessionAsync(session);
        var json = JsonSerializer.Serialize(serialized);

        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT OR REPLACE INTO Sessions (UserId, SessionJson, UpdatedAt)
            VALUES ($userId, $json, $updatedAt)
            """;
        cmd.Parameters.AddWithValue("$userId", userId);
        cmd.Parameters.AddWithValue("$json", json);
        cmd.Parameters.AddWithValue("$updatedAt", DateTimeOffset.UtcNow.ToString("O"));
        await cmd.ExecuteNonQueryAsync(cancellationToken);
        
        _logger.LogDebug("Saved session for user {UserId}", userId);
    }
}
