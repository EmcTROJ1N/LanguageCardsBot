using Cards.Domain.Entities;

namespace Cards.Application.Abstractions.Repositories;

/// <summary>
/// Defines persistence operations required by user use cases.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Gets a user by its identifier.
    /// </summary>
    Task<UserEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all users stored in the cards backend.
    /// </summary>
    Task<IEnumerable<UserEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists a new user.
    /// </summary>
    Task<UserEntity> AddAsync(UserEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists changes to an existing user.
    /// </summary>
    Task<UserEntity> UpdateAsync(UserEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a user by its identifier when it exists.
    /// </summary>
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by Telegram chat identifier.
    /// </summary>
    Task<UserEntity?> GetByChatIdAsync(long chatId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an existing user by chat identifier or creates one.
    /// </summary>
    Task<UserEntity> GetOrCreateAsync(long chatId, string? username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by Keycloak subject claim.
    /// </summary>
    Task<UserEntity?> GetByKeycloakIdAsync(string keycloakId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an existing user by Keycloak subject claim or creates a new one.
    /// </summary>
    Task<UserEntity> GetOrCreateByKeycloakIdAsync(string keycloakId, CancellationToken cancellationToken = default);
}
