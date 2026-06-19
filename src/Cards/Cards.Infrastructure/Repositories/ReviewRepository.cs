using Cards.Application.Abstractions.Repositories;
using Cards.Domain.Entities;
using Cards.Infrastructure.Common.Abstractions;
using Cards.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cards.Infrastructure.Repositories;

/// <summary>
/// Provides EF Core persistence operations for reviews.
/// </summary>
public class ReviewRepository(CardsMysqlDbContext context): AbstractCrudRepository<ReviewEntity>(context), IReviewRepository
{
    /// <inheritdoc />
    public async Task<IEnumerable<ReviewEntity>> GetTodayReviewsByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var todayStart = DateTime.UtcNow.Date;
        var todayEnd = todayStart.AddDays(1);

        return await context.Set<ReviewEntity>()
            .Where(x => x.CardEntity.UserId == userId &&
                        x.ReviewedAt >= todayStart &&
                        x.ReviewedAt < todayEnd)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<(int Total, int Correct)> GetTodayStatsByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var todayStart = DateTime.UtcNow.Date;
        var todayEnd = todayStart.AddDays(1);

        var total = await context.Set<ReviewEntity>()
            .CountAsync(x => x.CardEntity.UserId == userId &&
                             x.ReviewedAt >= todayStart &&
                             x.ReviewedAt < todayEnd,
                cancellationToken);

        var correct = await context.Set<ReviewEntity>()
            .CountAsync(x => x.CardEntity.UserId == userId &&
                             x.IsCorrect &&
                             x.ReviewedAt >= todayStart &&
                             x.ReviewedAt < todayEnd,
                cancellationToken);

        return (total, correct);
    }

    /// <inheritdoc />
    public async Task<(string? BestDay, int BestCount)> GetBestDayStatsByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var best = await context.Set<ReviewEntity>()
            .Where(x => x.CardEntity.UserId == userId)
            .GroupBy(x => x.ReviewedAt.Date)
            .Select(x => new { Day = x.Key, Count = x.Count() })
            .OrderByDescending(x => x.Count)
            .ThenByDescending(x => x.Day)
            .FirstOrDefaultAsync(cancellationToken);

        return best is null
            ? (null, 0)
            : (best.Day.ToString("yyyy-MM-dd"), best.Count);
    }
}
