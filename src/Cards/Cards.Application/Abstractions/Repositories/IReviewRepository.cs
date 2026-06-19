using Cards.Domain.Entities;

namespace Cards.Application.Abstractions.Repositories;

/// <summary>
/// Defines persistence operations required by review and statistics use cases.
/// </summary>
public interface IReviewRepository
{
    /// <summary>
    /// Gets a review by its identifier.
    /// </summary>
    Task<ReviewEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all stored reviews.
    /// </summary>
    Task<IEnumerable<ReviewEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists a new review.
    /// </summary>
    Task<ReviewEntity> AddAsync(ReviewEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists changes to an existing review.
    /// </summary>
    Task<ReviewEntity> UpdateAsync(ReviewEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a review by its identifier when it exists.
    /// </summary>
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets reviews recorded today for a user.
    /// </summary>
    Task<IEnumerable<ReviewEntity>> GetTodayReviewsByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets today's total and correct review counts for a user.
    /// </summary>
    Task<(int Total, int Correct)> GetTodayStatsByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the user's best review day and the review count for that day.
    /// </summary>
    Task<(string? BestDay, int BestCount)> GetBestDayStatsByUserIdAsync(int userId, CancellationToken cancellationToken = default);
}
