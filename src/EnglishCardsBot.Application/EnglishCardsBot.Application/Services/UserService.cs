using EnglishCardsBot.Application.Interfaces;
using EnglishCardsBot.Domain.Entities;

namespace EnglishCardsBot.Application.Services;

public class UserService(IUserRepository users)
{
    public Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => users.GetByIdAsync(id, cancellationToken);

    public Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
        => users.GetAllAsync(cancellationToken);

    public Task<User> AddAsync(User entity, CancellationToken cancellationToken = default)
        => users.AddAsync(entity, cancellationToken);

    public Task UpdateAsync(User entity, CancellationToken cancellationToken = default)
        => users.UpdateAsync(entity, cancellationToken);

    public Task DeleteAsync(User entity, CancellationToken cancellationToken = default)
        => users.DeleteAsync(entity, cancellationToken);

    public Task<User?> GetByChatIdAsync(long chatId, CancellationToken cancellationToken = default)
        => users.GetByChatIdAsync(chatId, cancellationToken);

    public Task<User> GetOrCreateAsync(long chatId, string? username, CancellationToken cancellationToken = default)
        => users.GetOrCreateAsync(chatId, username, cancellationToken);

    // 2) Метод use-case уровня Application: GetOrCreate + sync username
    public async Task<User> GetOrCreateAndSyncUsernameAsync(
        long chatId,
        string? username,
        CancellationToken cancellationToken = default)
    {
        var user = await users.GetByChatIdAsync(chatId, cancellationToken);

        if (user is null)
        {
            // Создаём через репозиторий (он уже знает дефолты в твоей реализации)
            return await users.GetOrCreateAsync(chatId, username, cancellationToken);
        }

        // Если username пришёл и отличается — обновляем
        if (!string.IsNullOrWhiteSpace(username) &&
            !string.Equals(user.Username, username, StringComparison.Ordinal))
        {
            user.Username = username;
            await users.UpdateAsync(user, cancellationToken);
        }

        return user;
    }

    public async Task UpdateNextReminderAtUtcAsync(int userId, DateTime nextReminderAtUtc,
        CancellationToken cancellationToken = default)
    {
        var user = await GetByIdAsync(userId, cancellationToken);
        if (user is not null)
        {
            user.NextReminderAtUtc = nextReminderAtUtc;
            await UpdateAsync(user, cancellationToken);
        }
    }
}
