using Cards.Domain.Entities;

namespace Cards.Application.Cards;

/// <summary>
/// Coordinates card use cases shared by gRPC and REST transports.
/// </summary>
public interface ICardApplicationService
{
    /// <summary>
    /// Gets a card by its identifier.
    /// </summary>
    Task<CardEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all cards.
    /// </summary>
    Task<IReadOnlyCollection<CardEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all cards owned by a user.
    /// </summary>
    Task<IReadOnlyCollection<CardEntity>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the next card due for review for a user.
    /// </summary>
    Task<CardEntity?> GetDueCardAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a card or returns an existing user card with the same term.
    /// </summary>
    Task<CardEntity> AddAsync(AddCardCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates editable card fields.
    /// </summary>
    Task<bool> UpdateAsync(UpdateCardCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a card review result and updates spaced-repetition state.
    /// </summary>
    Task<bool> UpdateReviewAsync(int cardId, bool isCorrect, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a card by its identifier.
    /// </summary>
    Task<bool> DeleteByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all cards owned by a user.
    /// </summary>
    Task<bool> DeleteByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports all cards owned by a user as a downloadable file.
    /// </summary>
    Task<CardExportResult> ExportAsync(int userId, CardExportFormat format, CancellationToken cancellationToken = default);
}
