using Cards.Application.Abstractions.Repositories;
using Cards.Application.Cards;
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

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<CardEntity>> SearchAsync(
        int userId,
        CardSearchQuery query,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var q = query.Q?.Trim();

        var set = dbContext.Set<CardEntity>().Where(c => c.UserId == userId);

        // Text search
        if (!string.IsNullOrEmpty(q))
            set = set.Where(c => c.Term.Contains(q) || c.Translation.Contains(q));

        // Status filter
        set = query.Filter switch
        {
            // "due" tab includes both 'due' and 'new' status cards (mirrors client statusOf logic)
            "due"     => set.Where(c => !c.Learned && (c.TotalReviews == 0 || !c.NextReviewAt.HasValue || c.NextReviewAt <= now)),
            "new"     => set.Where(c => !c.Learned && (c.TotalReviews == 0 || !c.NextReviewAt.HasValue)),
            "learned" => set.Where(c => c.Learned),
            _         => set,
        };

        // Sort
        bool desc = query.SortDir == "desc";
        set = query.Sort switch
        {
            "translation" => desc ? set.OrderByDescending(c => c.Translation) : set.OrderBy(c => c.Translation),
            "level"       => desc ? set.OrderByDescending(c => c.Level)       : set.OrderBy(c => c.Level),
            "next"        => desc ? set.OrderByDescending(c => c.NextReviewAt) : set.OrderBy(c => c.NextReviewAt),
            "accuracy"    => desc
                ? set.OrderByDescending(c => c.TotalReviews == 0 ? 0.0 : (double)c.CorrectReviews / c.TotalReviews)
                : set.OrderBy(c => c.TotalReviews == 0 ? 0.0 : (double)c.CorrectReviews / c.TotalReviews),
            _ => desc ? set.OrderByDescending(c => c.Term) : set.OrderBy(c => c.Term),
        };

        return await set.ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<(int All, int Due, int New, int Learned)> CountByStatusAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var cards = dbContext.Set<CardEntity>().Where(c => c.UserId == userId);

        // EF Core DbContext is not thread-safe — run counts sequentially on the same instance
        var all     = await cards.CountAsync(cancellationToken);
        var due     = await cards.CountAsync(c => !c.Learned && (c.TotalReviews == 0 || !c.NextReviewAt.HasValue || c.NextReviewAt <= now), cancellationToken);
        var newCards = await cards.CountAsync(c => !c.Learned && (c.TotalReviews == 0 || !c.NextReviewAt.HasValue), cancellationToken);
        var learned = await cards.CountAsync(c => c.Learned, cancellationToken);

        return (all, due, newCards, learned);
    }
}
