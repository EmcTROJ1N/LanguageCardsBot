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
        throw new NotImplementedException();
    }

    public Task<CardEntity?> GetRandomActiveCardAsync(int userId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
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