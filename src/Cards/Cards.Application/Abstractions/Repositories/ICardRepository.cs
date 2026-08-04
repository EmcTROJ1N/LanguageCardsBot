using Cards.Domain.Entities;

namespace Cards.Application.Abstractions.Repositories;

/// <summary>
/// Defines persistence operations required by card use cases.
/// </summary>
public interface ICardRepository
{
    /// <summary>
    /// Gets a card by its identifier.
    /// </summary>
    Task<CardEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all cards stored in the cards backend.
    /// </summary>
    Task<IEnumerable<CardEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists a new card.
    /// </summary>
    Task<CardEntity> AddAsync(CardEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists changes to an existing card.
    /// </summary>
    Task<CardEntity> UpdateAsync(CardEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a card by its identifier when it exists.
    /// </summary>
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the next card that is due for review for a user.
    /// </summary>
    Task<CardEntity?> GetDueCardAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a random active card for a user.
    /// </summary>
    Task<CardEntity?> GetRandomActiveCardAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all cards owned by a user.
    /// </summary>
    Task<IEnumerable<CardEntity>> GetAllByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all cards owned by a user and returns the number of deleted rows.
    /// </summary>
    Task<int> DeleteAllByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts cards that are currently due for review across all users.
    /// </summary>
    Task<int> CountDueAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts non-learned cards grouped by Level across all users.
    /// </summary>
    Task<IReadOnlyDictionary<int, int>> CountActiveByLevelAsync(CancellationToken cancellationToken = default);
}
