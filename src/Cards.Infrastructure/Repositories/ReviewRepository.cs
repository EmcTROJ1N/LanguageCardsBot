using Cards.Domain.Entities;
using Cards.Infrastructure.Common.Abstractions;
using Cards.Infrastructure.Interfaces;

namespace Cards.Infrastructure.Repositories;

public class ReviewRepository: AbstractCrudRepository<ReviewEntity>, IReviewRepository
{
    public Task<IEnumerable<ReviewEntity>> GetTodayReviewsByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<(int Total, int Correct)> GetTodayStatsByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<(string? BestDay, int BestCount)> GetBestDayStatsByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}