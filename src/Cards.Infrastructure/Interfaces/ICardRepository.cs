using Cards.Domain.Entities;
using Cards.Infrastructure.Common.Interfaces;

namespace Cards.Infrastructure.Interfaces;

public interface ICardRepository : ICrudRepository<CardEntity>
{
    Task<CardEntity?> GetDueCardAsync(int userId, CancellationToken cancellationToken = default);
    Task<CardEntity?> GetRandomActiveCardAsync(int userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<CardEntity>> GetAllByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<int> DeleteAllByUserIdAsync(int userId, CancellationToken cancellationToken = default);
}
