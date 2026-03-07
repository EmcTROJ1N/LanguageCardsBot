using Dapper;
using Microsoft.Data.Sqlite;
using System.Text.RegularExpressions;

namespace EnglishCardsBot.Infrastructure.Data;

public class ApplicationDbContext
{
    private readonly string _connectionString;

    public ApplicationDbContext(string connectionString)
    {
        _connectionString = connectionString;
        DapperTypeHandlers.Register();
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    public SqliteConnection CreateConnection()
    {
        return new SqliteConnection(_connectionString);
    }

    private void EnsureDatabaseDirectoryExists()
    {
        string? dbPath = null;

        var match = Regex.Match(_connectionString, @"Data Source\s*=\s*(.+?)(?:;|$)", RegexOptions.IgnoreCase);
        if (match.Success)
            dbPath = match.Groups[1].Value.Trim();
        else
            dbPath = _connectionString;

        if (string.IsNullOrEmpty(dbPath))
            return;

        dbPath = dbPath.Trim('"', '\'');

        var directory = Path.GetDirectoryName(dbPath);

        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        EnsureDatabaseDirectoryExists();

        using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var commands = new[]
        {
            """
            CREATE TABLE IF NOT EXISTS users (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                chat_id INTEGER UNIQUE NOT NULL,
                username TEXT,
                created_at TEXT NOT NULL,
                reminder_interval_minutes INTEGER DEFAULT 60,
                next_reminder_at_utc TEXT,
                hide_translations INTEGER DEFAULT 1
            )
            """,
            """
            CREATE TABLE IF NOT EXISTS cards (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                user_id INTEGER NOT NULL,
                term TEXT NOT NULL,
                translation TEXT NOT NULL,
                transcription TEXT NOT NULL,
                example TEXT,
                level INTEGER DEFAULT 1,
                next_review_at TEXT,
                learned INTEGER DEFAULT 0,
                created_at TEXT NOT NULL,
                last_review_at TEXT,
                total_reviews INTEGER DEFAULT 0,
                correct_reviews INTEGER DEFAULT 0,
                FOREIGN KEY (user_id) REFERENCES users(id)
            )
            """,
            """
            CREATE TABLE IF NOT EXISTS reviews (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                card_id INTEGER NOT NULL,
                is_correct INTEGER NOT NULL,
                reviewed_at TEXT NOT NULL,
                FOREIGN KEY (card_id) REFERENCES cards(id)
            )
            """,
            "CREATE INDEX IF NOT EXISTS idx_cards_user_id ON cards(user_id)",
            "CREATE INDEX IF NOT EXISTS idx_cards_next_review_at ON cards(next_review_at)",
            "CREATE INDEX IF NOT EXISTS idx_reviews_card_id ON reviews(card_id)",
            "CREATE INDEX IF NOT EXISTS idx_reviews_reviewed_at ON reviews(reviewed_at)"
        };

        foreach (var command in commands)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = command;
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        // Мягкая миграция: добавить колонку next_reminder_at_utc, если БД была создана раньше
        await EnsureUsersColumnExistsAsync(connection, "next_reminder_at_utc", "TEXT", cancellationToken);
    }

    private static async Task EnsureUsersColumnExistsAsync(
        SqliteConnection connection,
        string columnName,
        string columnType,
        CancellationToken cancellationToken)
    {
        // PRAGMA table_info(users) -> список колонок
        var columns = (await connection.QueryAsync<dynamic>(
            new CommandDefinition("PRAGMA table_info(users);", cancellationToken: cancellationToken)))
            .Select(r => (string)r.name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (columns.Contains(columnName))
            return;

        var sql = $"ALTER TABLE users ADD COLUMN {columnName} {columnType};";
        await connection.ExecuteAsync(new CommandDefinition(sql, cancellationToken: cancellationToken));
    }
}
