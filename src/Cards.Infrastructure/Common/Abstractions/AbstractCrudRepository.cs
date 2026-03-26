using Cards.Domain.Common;
using Cards.Infrastructure.Common.Interfaces;

namespace Cards.Infrastructure.Common.Abstractions;

public abstract class AbstractCrudRepository<T>: ICrudRepository<T> where T : IEntity
{
    public Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}