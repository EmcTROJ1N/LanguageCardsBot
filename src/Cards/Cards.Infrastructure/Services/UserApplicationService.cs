using Cards.Application.Abstractions.Repositories;
using Cards.Application.Reminders;
using Cards.Application.Users;
using Cards.Domain.Entities;

namespace Cards.Infrastructure.Services;

/// <summary>
/// Implements user use cases shared by gRPC and REST transports.
/// </summary>
public sealed class UserApplicationService(
    IUserRepository userRepository,
    ICardReminderOrchestrator reminderOrchestrator) : IUserApplicationService
{
    /// <inheritdoc />
    public Task<UserEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return userRepository.GetByIdAsync(id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<UserEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return (await userRepository.GetAllAsync(cancellationToken)).ToList();
    }

    /// <inheritdoc />
    public async Task<UserEntity> AddAsync(UserCommand command, CancellationToken cancellationToken = default)
    {
        var user = new UserEntity
        {
            Id = command.Id,
            KeycloakId = command.KeycloakId,
            ChatId = command.ChatId,
            Username = NormalizeUsername(command.Username),
            CreatedAt = ToUtc(command.CreatedAt ?? DateTime.UtcNow),
            ReminderIntervalMinutes = command.ReminderIntervalMinutes < 0 ? 1 : command.ReminderIntervalMinutes,
            NextReminderAtUtc = ToUtc(command.NextReminderAtUtc),
            HideTranslations = command.HideTranslations
        };

        var created = await userRepository.AddAsync(user, cancellationToken);
        reminderOrchestrator.RegisterUser(created);
        return created;
    }

    /// <inheritdoc />
    public async Task<bool> UpdateAsync(UserCommand command, CancellationToken cancellationToken = default)
    {
        var existingUser = await userRepository.GetByIdAsync(command.Id, cancellationToken);
        if (existingUser is null)
            return false;

        existingUser.KeycloakId = command.KeycloakId;
        existingUser.ChatId = command.ChatId;
        existingUser.Username = NormalizeUsername(command.Username);
        existingUser.ReminderIntervalMinutes = command.ReminderIntervalMinutes < 0 ? 1 : command.ReminderIntervalMinutes;
        existingUser.NextReminderAtUtc = ToUtc(command.NextReminderAtUtc);
        existingUser.HideTranslations = command.HideTranslations;

        await userRepository.UpdateAsync(existingUser, cancellationToken);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(id, cancellationToken);
        if (user is null)
            return false;

        await userRepository.DeleteAsync(id, cancellationToken);
        reminderOrchestrator.UnregisterUser(id);
        return true;
    }

    /// <inheritdoc />
    public Task<UserEntity?> GetByChatIdAsync(long chatId, CancellationToken cancellationToken = default)
    {
        return userRepository.GetByChatIdAsync(chatId, cancellationToken);
    }

    /// <inheritdoc />
    public Task<UserEntity> GetOrCreateAsync(
        long chatId,
        string? username,
        CancellationToken cancellationToken = default)
    {
        return userRepository.GetOrCreateAsync(chatId, NormalizeUsername(username), cancellationToken);
    }

    /// <inheritdoc />
    public async Task<UserEntity> GetOrCreateAndSyncUsernameAsync(
        long chatId,
        string? username,
        CancellationToken cancellationToken = default)
    {
        var normalizedUsername = NormalizeUsername(username);
        var user = await userRepository.GetOrCreateAsync(chatId, normalizedUsername, cancellationToken);

        if (!string.Equals(user.Username, normalizedUsername, StringComparison.Ordinal))
        {
            user.Username = normalizedUsername;
            await userRepository.UpdateAsync(user, cancellationToken);
        }

        return user;
    }

    /// <inheritdoc />
    public async Task<bool> UpdateNextReminderAtUtcAsync(
        int userId,
        DateTime? nextReminderAtUtc,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return false;

        user.NextReminderAtUtc = ToUtc(nextReminderAtUtc);
        await userRepository.UpdateAsync(user, cancellationToken);
        return true;
    }

    /// <inheritdoc />
    public Task<UserEntity> GetOrCreateByKeycloakIdAsync(
        string keycloakId,
        CancellationToken cancellationToken = default)
    {
        return userRepository.GetOrCreateByKeycloakIdAsync(keycloakId, cancellationToken);
    }

    private static string? NormalizeUsername(string? username)
    {
        return string.IsNullOrWhiteSpace(username) ? null : username.Trim();
    }

    private static DateTime? ToUtc(DateTime? value)
    {
        return value.HasValue ? ToUtc(value.Value) : null;
    }

    private static DateTime ToUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }
}
