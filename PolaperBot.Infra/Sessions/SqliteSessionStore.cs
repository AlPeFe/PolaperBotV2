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
                UsageJson TEXT,
                UpdatedAt TEXT NOT NULL
            )
            """;
        cmd.ExecuteNonQuery();
        
        AddUsageColumnIfNotExists(conn);
        
        _logger.LogInformation("Sessions table initialized");
    }

    private static void AddUsageColumnIfNotExists(SqliteConnection conn)
    {
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "ALTER TABLE Sessions ADD COLUMN UsageJson TEXT";
            cmd.ExecuteNonQuery();
        }
        catch (SqliteException)
        {
        }
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

    public async Task SaveAsync(long userId, AgentSession session, SessionUsage? usage = null, CancellationToken cancellationToken = default)
    {
        var serialized = await _agent.SerializeSessionAsync(session);
        var sessionJson = JsonSerializer.Serialize(serialized);
        var usageJson = usage != null ? JsonSerializer.Serialize(usage) : null;

        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT OR REPLACE INTO Sessions (UserId, SessionJson, UsageJson, UpdatedAt)
            VALUES ($userId, $sessionJson, $usageJson, $updatedAt)
            """;
        cmd.Parameters.AddWithValue("$userId", userId);
        cmd.Parameters.AddWithValue("$sessionJson", sessionJson);
        cmd.Parameters.AddWithValue("$usageJson", usageJson ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$updatedAt", DateTimeOffset.UtcNow.ToString("O"));
        await cmd.ExecuteNonQueryAsync(cancellationToken);

        if (usage != null)
        {
            if (usage.InputTokens > 0 || usage.OutputTokens > 0)
            {
                _logger.LogInformation(
                    "Saved session for user {UserId} - Tokens: {Input}+{Output}={Total}, Time: {Time}ms",
                    userId, usage.InputTokens, usage.OutputTokens, usage.TotalTokens, usage.ProcessingTimeMs);
            }
            else
            {
                _logger.LogInformation(
                    "Saved session for user {UserId} - Time: {Time}ms",
                    userId, usage.ProcessingTimeMs);
            }
        }
        else
        {
            _logger.LogDebug("Saved session for user {UserId}", userId);
        }
    }
}
