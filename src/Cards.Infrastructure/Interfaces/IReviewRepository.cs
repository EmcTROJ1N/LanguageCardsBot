using Cards.Domain.Entities;
using Cards.Infrastructure.Common;
using Cards.Infrastructure.Common.Interfaces;

namespace Cards.Infrastructure.Interfaces;

public interface IReviewRepository : ICrudRepository<ReviewEntity>
{
    Task<IEnumerable<ReviewEntity>> GetTodayReviewsByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<(int Total, int Correct)> GetTodayStatsByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<(string? BestDay, int BestCount)> GetBestDayStatsByUserIdAsync(int userId, CancellationToken cancellationToken = default);
}