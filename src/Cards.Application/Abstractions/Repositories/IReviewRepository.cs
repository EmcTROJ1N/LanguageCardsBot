using Cards.Domain.Entities;

namespace Cards.Application.Abstractions.Repositories;

public interface IReviewRepository
{
    Task<ReviewEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ReviewEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ReviewEntity> AddAsync(ReviewEntity entity, CancellationToken cancellationToken = default);
    Task<ReviewEntity> UpdateAsync(ReviewEntity entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ReviewEntity>> GetTodayReviewsByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<(int Total, int Correct)> GetTodayStatsByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<(string? BestDay, int BestCount)> GetBestDayStatsByUserIdAsync(int userId, CancellationToken cancellationToken = default);
}
