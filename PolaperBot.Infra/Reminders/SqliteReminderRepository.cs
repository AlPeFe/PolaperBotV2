using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using PolaperBot.Core.AI.Reminders;

namespace PolaperBot.Infra.Reminders;

public class SqliteReminderRepository : IReminderRepository
{
    private readonly string _connectionString;
    private readonly ILogger<SqliteReminderRepository> _logger;

    public SqliteReminderRepository(string dbPath, ILogger<SqliteReminderRepository> logger)
    {
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
            CREATE TABLE IF NOT EXISTS Reminders (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Description TEXT NOT NULL,
                ScheduledFor TEXT NOT NULL,
                CreatedAt TEXT NOT NULL,
                TelegramChatId INTEGER NOT NULL,
                IsNotified INTEGER NOT NULL DEFAULT 0,
                NotifiedAt TEXT
            )
            """;
        cmd.ExecuteNonQuery();

        _logger.LogInformation("Reminders table initialized");
    }

    public async Task<int> CreateAsync(Reminder reminder, CancellationToken cancellationToken = default)
    {
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO Reminders (Description, ScheduledFor, CreatedAt, TelegramChatId, IsNotified, NotifiedAt)
            VALUES ($description, $scheduledFor, $createdAt, $chatId, $isNotified, $notifiedAt);
            SELECT last_insert_rowid();
            """;
        cmd.Parameters.AddWithValue("$description", reminder.Description);
        cmd.Parameters.AddWithValue("$scheduledFor", reminder.ScheduledFor.ToString("O"));
        cmd.Parameters.AddWithValue("$createdAt", reminder.CreatedAt.ToString("O"));
        cmd.Parameters.AddWithValue("$chatId", reminder.TelegramChatId);
        cmd.Parameters.AddWithValue("$isNotified", reminder.IsNotified ? 1 : 0);
        cmd.Parameters.AddWithValue("$notifiedAt", reminder.NotifiedAt?.ToString("O") ?? (object)DBNull.Value);

        var id = Convert.ToInt32(await cmd.ExecuteScalarAsync(cancellationToken));
        
        _logger.LogInformation(
            "Created reminder {Id}: '{Description}' for {ScheduledFor} (ChatId: {ChatId})",
            id, reminder.Description, reminder.ScheduledFor, reminder.TelegramChatId);

        return id;
    }

    public async Task<Reminder?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM Reminders WHERE Id = $id";
        cmd.Parameters.AddWithValue("$id", id);

        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            return MapFromReader(reader);
        }

        return null;
    }

    public async Task<IReadOnlyList<Reminder>> GetPendingAsync(CancellationToken cancellationToken = default)
    {
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM Reminders WHERE IsNotified = 0 ORDER BY ScheduledFor ASC";

        var reminders = new List<Reminder>();
        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            reminders.Add(MapFromReader(reader));
        }

        return reminders;
    }

    public async Task<IReadOnlyList<Reminder>> GetUpcomingAsync(TimeSpan advanceNotice, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var threshold = now.Add(advanceNotice);

        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT * FROM Reminders 
            WHERE IsNotified = 0 
              AND ScheduledFor <= $threshold
              AND ScheduledFor > $now
            ORDER BY ScheduledFor ASC
            """;
        cmd.Parameters.AddWithValue("$threshold", threshold.ToString("O"));
        cmd.Parameters.AddWithValue("$now", now.ToString("O"));

        var reminders = new List<Reminder>();
        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            reminders.Add(MapFromReader(reader));
        }

        return reminders;
    }

    public async Task MarkAsNotifiedAsync(int id, CancellationToken cancellationToken = default)
    {
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            UPDATE Reminders 
            SET IsNotified = 1, NotifiedAt = $notifiedAt 
            WHERE Id = $id
            """;
        cmd.Parameters.AddWithValue("$id", id);
        cmd.Parameters.AddWithValue("$notifiedAt", DateTime.UtcNow.ToString("O"));

        await cmd.ExecuteNonQueryAsync(cancellationToken);
        _logger.LogInformation("Marked reminder {Id} as notified", id);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Reminders WHERE Id = $id";
        cmd.Parameters.AddWithValue("$id", id);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
        _logger.LogInformation("Deleted reminder {Id}", id);
    }

    private static Reminder MapFromReader(SqliteDataReader reader)
    {
        return new Reminder
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Description = reader.GetString(reader.GetOrdinal("Description")),
            ScheduledFor = DateTime.Parse(reader.GetString(reader.GetOrdinal("ScheduledFor"))),
            CreatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("CreatedAt"))),
            TelegramChatId = reader.GetInt64(reader.GetOrdinal("TelegramChatId")),
            IsNotified = reader.GetInt32(reader.GetOrdinal("IsNotified")) == 1,
            NotifiedAt = reader.IsDBNull(reader.GetOrdinal("NotifiedAt"))
                ? null
                : DateTime.Parse(reader.GetString(reader.GetOrdinal("NotifiedAt")))
        };
    }
}
