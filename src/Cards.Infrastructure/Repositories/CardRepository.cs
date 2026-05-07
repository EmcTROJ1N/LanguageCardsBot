using Cards.Domain.Entities;
using Cards.Infrastructure.Common.Abstractions;
using Cards.Infrastructure.Data;
using Cards.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Cards.Infrastructure.Repositories;

public class CardRepository(CardsMysqlDbContext dbContext): AbstractCrudRepository<CardEntity>(dbContext), ICardRepository
{
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

    public async Task<IEnumerable<CardEntity>> GetAllByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<CardEntity>()
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> DeleteAllByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<CardEntity>()
            .Where(x => x.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
