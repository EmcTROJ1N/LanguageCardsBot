using Cards.Domain.Entities;

namespace Cards.Application.Users;

/// <summary>
/// Coordinates user use cases shared by gRPC and REST transports.
/// </summary>
public interface IUserApplicationService
{
    /// <summary>
    /// Gets a user by its identifier.
    /// </summary>
    Task<UserEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all users.
    /// </summary>
    Task<IReadOnlyCollection<UserEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new user.
    /// </summary>
    Task<UserEntity> AddAsync(UserCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    Task<bool> UpdateAsync(UserCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a user by its identifier.
    /// </summary>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by Telegram chat identifier.
    /// </summary>
    Task<UserEntity?> GetByChatIdAsync(long chatId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an existing user by chat identifier or creates one.
    /// </summary>
    Task<UserEntity> GetOrCreateAsync(long chatId, string? username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets or creates a user and synchronizes the stored username.
    /// </summary>
    Task<UserEntity> GetOrCreateAndSyncUsernameAsync(long chatId, string? username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the next reminder timestamp for a user.
    /// </summary>
    Task<bool> UpdateNextReminderAtUtcAsync(int userId, DateTime? nextReminderAtUtc, CancellationToken cancellationToken = default);
}
