using Dapper;
using EnglishCardsBot.Application.Interfaces;
using EnglishCardsBot.Domain.Entities;
using EnglishCardsBot.Infrastructure.Data;

namespace EnglishCardsBot.Infrastructure.Repositories;

public class UserRepository(ApplicationDbContext context) : IUserRepository
{
    public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = context.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var sql = """
            SELECT
                id,
                chat_id AS ChatId,
                username AS Username,
                created_at AS CreatedAt,
                next_reminder_at_utc AS NextReminderAtUtc,
                reminder_interval_minutes AS ReminderIntervalMinutes,
                hide_translations AS HideTranslations
            FROM users
            WHERE id = @Id
        """;

        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
    }

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = context.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var sql = """
            SELECT
                id,
                chat_id AS ChatId,
                username AS Username,
                created_at AS CreatedAt,
                next_reminder_at_utc AS NextReminderAtUtc,
                reminder_interval_minutes AS ReminderIntervalMinutes,
                hide_translations AS HideTranslations
            FROM users
        """;

        return await connection.QueryAsync<User>(sql);
    }

    public async Task<User> AddAsync(User entity, CancellationToken cancellationToken = default)
    {
        using var connection = context.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var sql = """
            INSERT INTO users (
                chat_id,
                username,
                created_at,
                next_reminder_at_utc,
                reminder_interval_minutes,
                hide_translations
            )
            VALUES (
                @ChatId,
                @Username,
                @CreatedAt,
                @NextReminderAtUtc,
                @ReminderIntervalMinutes,
                @HideTranslations
            );

            SELECT last_insert_rowid();
        """;

        var id = await connection.QuerySingleAsync<int>(sql, new
        {
            entity.ChatId,
            entity.Username,
            entity.CreatedAt,
            entity.NextReminderAtUtc,
            entity.ReminderIntervalMinutes,
            HideTranslations = entity.HideTranslations ? 1 : 0
        });

        entity.Id = id;
        return entity;
    }

    public async Task UpdateAsync(User entity, CancellationToken cancellationToken = default)
    {
        using var connection = context.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var sql = """
            UPDATE users
            SET
                chat_id = @ChatId,
                username = @Username,
                created_at = @CreatedAt,
                next_reminder_at_utc = @NextReminderAtUtc,
                reminder_interval_minutes = @ReminderIntervalMinutes,
                hide_translations = @HideTranslations
            WHERE id = @Id
        """;

        await connection.ExecuteAsync(sql, new
        {
            entity.Id,
            entity.ChatId,
            entity.Username,
            entity.CreatedAt,
            entity.NextReminderAtUtc,
            entity.ReminderIntervalMinutes,
            HideTranslations = entity.HideTranslations ? 1 : 0
        });
    }

    public async Task DeleteAsync(User entity, CancellationToken cancellationToken = default)
    {
        using var connection = context.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var sql = "DELETE FROM users WHERE id = @Id";
        await connection.ExecuteAsync(sql, new { entity.Id });
    }

    public async Task<User?> GetByChatIdAsync(long chatId, CancellationToken cancellationToken = default)
    {
        using var connection = context.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var sql = """
            SELECT
                id,
                chat_id AS ChatId,
                username AS Username,
                created_at AS CreatedAt,
                next_reminder_at_utc AS NextReminderAtUtc,
                reminder_interval_minutes AS ReminderIntervalMinutes,
                hide_translations AS HideTranslations
            FROM users
            WHERE chat_id = @ChatId
        """;

        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { ChatId = chatId });
    }

    public async Task<User> GetOrCreateAsync(long chatId, string? username, CancellationToken cancellationToken = default)
    {
        var users = await GetAllAsync(cancellationToken);
        var user = await GetByChatIdAsync(chatId, cancellationToken);
        if (user != null)
            return user;

        user = new User
        {
            ChatId = chatId,
            Username = username ?? string.Empty,
        };

        return await AddAsync(user, cancellationToken);
    }
}
