using Cards.Domain.Common;
using Cards.Infrastructure.Common.Interfaces;
using Cards.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cards.Infrastructure.Common.Abstractions;

public abstract class AbstractCrudRepository<T>(CardsMysqlDbContext dbContext): ICrudRepository<T> where T : class, IEntityWithId
{
    public async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<T>()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<T>()
            .ToListAsync(cancellationToken: cancellationToken);
    }

    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<T>().AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return entity;
    }

    public async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        dbContext.Set<T>().Attach(entity);
        dbContext.Entry(entity).State = EntityState.Modified;

        await dbContext.SaveChangesAsync(cancellationToken);

        return entity;
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Set<T>()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (entity is null)
            return;

        dbContext.Set<T>().Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}