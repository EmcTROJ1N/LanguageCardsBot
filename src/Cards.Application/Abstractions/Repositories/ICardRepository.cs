using Cards.Domain.Entities;

namespace Cards.Application.Abstractions.Repositories;

public interface ICardRepository
{
    Task<CardEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<CardEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CardEntity> AddAsync(CardEntity entity, CancellationToken cancellationToken = default);
    Task<CardEntity> UpdateAsync(CardEntity entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
    Task<CardEntity?> GetDueCardAsync(int userId, CancellationToken cancellationToken = default);
    Task<CardEntity?> GetRandomActiveCardAsync(int userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<CardEntity>> GetAllByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<int> DeleteAllByUserIdAsync(int userId, CancellationToken cancellationToken = default);
}
