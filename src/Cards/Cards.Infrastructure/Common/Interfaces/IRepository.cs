using Cards.Domain.Common;

namespace Cards.Infrastructure.Common.Interfaces;

/// <summary>
/// Marker interface for repositories operating on entities that implement <see cref="IEntityWithId"/>.
/// </summary>
/// <typeparam name="T">Entity type owned by the repository.</typeparam>
public interface IRepository<T> where T : IEntityWithId;
