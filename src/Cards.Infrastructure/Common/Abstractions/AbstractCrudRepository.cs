using Cards.Domain.Common;
using Cards.Domain.Exceptions;
using Cards.Infrastructure.Common.Interfaces;
using Cards.Infrastructure.Data;
using LanguageCardsBot.Contracts.Common.Exceptions;
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
        try
        {
            await dbContext.Set<T>().AddAsync(entity, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            return entity;
        }
        catch (OperationCanceledException ex)
        {
            throw new GrpcCancelledException(innerException: ex);
        }
        catch (DbUpdateException ex)
        {
            throw new GrpcInternalException(innerException: ex);
        }
    }

    public async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        try
        {
            dbContext.Set<T>().Attach(entity);
            dbContext.Entry(entity).State = EntityState.Modified;
            await dbContext.SaveChangesAsync(cancellationToken);
            return entity;
        }
        catch (OperationCanceledException ex)
        {
            throw new GrpcCancelledException(innerException: ex);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // DbUpdateConcurrencyException extends DbUpdateException — must be caught first.
            throw new GrpcAbortedException(innerException: ex);
        }
        catch (DbUpdateException ex)
        {
            throw new GrpcInternalException(innerException: ex);
        }
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await dbContext.Set<T>()
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

            if (entity is null)
                return;

            dbContext.Set<T>().Remove(entity);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (OperationCanceledException ex)
        {
            throw new GrpcCancelledException(innerException: ex);
        }
        catch (DbUpdateException ex)
        {
            throw new GrpcInternalException(innerException: ex);
        }
    }
}