using Cards.Domain.Common;

namespace Cards.Infrastructure.Common.Interfaces;

// TODO: move into Contracts.Common

/// <summary>
/// Generic CRUD repository contract for entities identified by <see cref="IEntityWithId"/>.
/// </summary>
/// <typeparam name="T">Entity type owned by the repository.</typeparam>
public interface ICrudRepository<T>: IRepository<T> where T : IEntityWithId
{
    /// <summary>Retrieves a single entity by identifier, or <c>null</c> if not found.</summary>
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    /// <summary>Retrieves all entities of type <typeparamref name="T"/>.</summary>
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    /// <summary>Inserts a new entity and persists changes; returns the tracked entity.</summary>
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    /// <summary>Attaches and marks the entity as modified, then persists changes.</summary>
    Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);
    /// <summary>Deletes the entity by identifier if it exists; otherwise a no-op.</summary>
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
}
