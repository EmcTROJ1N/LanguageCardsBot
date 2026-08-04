using Cards.Application.Abstractions.Repositories;
using Cards.Domain.Entities;
using Cards.Infrastructure.Common.Abstractions;
using Cards.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cards.Infrastructure.Repositories;

/// <summary>
/// Provides EF Core persistence operations for cards.
/// </summary>
public class CardRepository(CardsMysqlDbContext dbContext): AbstractCrudRepository<CardEntity>(dbContext), ICardRepository
{
    /// <inheritdoc />
    public Task<CardEntity?> GetDueCardAsync(int userId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        return dbContext.Set<CardEntity>()
            .Where(x => x.UserId == userId &&
                        !x.Learned &&
                        (x.NextReviewAt == null || x.NextReviewAt <= now))
            .OrderBy(x => x.NextReviewAt ?? DateTime.MinValue)
            .ThenBy(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CardEntity?> GetRandomActiveCardAsync(int userId, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Set<CardEntity>()
            .Where(x => x.UserId == userId && !x.Learned)
            .OrderBy(x => x.Id);

        var count = await query.CountAsync(cancellationToken);
        if (count == 0)
            return null;

        var offset = Random.Shared.Next(count);
        return await query
            .Skip(offset)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CardEntity>> GetAllByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<CardEntity>()
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> DeleteAllByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<CardEntity>()
            .Where(x => x.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<int> CountDueAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        return dbContext.Set<CardEntity>()
            .Where(x => !x.Learned && (x.NextReviewAt == null || x.NextReviewAt <= now))
            .CountAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<int, int>> CountActiveByLevelAsync(
        CancellationToken cancellationToken = default)
    {
        var groups = await dbContext.Set<CardEntity>()
            .Where(x => !x.Learned)
            .GroupBy(x => x.Level)
            .Select(g => new { Level = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return groups.ToDictionary(g => g.Level, g => g.Count);
    }
}
