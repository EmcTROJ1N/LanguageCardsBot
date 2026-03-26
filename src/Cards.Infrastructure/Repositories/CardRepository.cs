using Cards.Domain.Entities;
using Cards.Infrastructure.Common.Abstractions;
using Cards.Infrastructure.Interfaces;

namespace Cards.Infrastructure.Repositories;

public class CardRepository: AbstractCrudRepository<CardEntity>, ICardRepository
{
    public Task<CardEntity?> GetDueCardAsync(int userId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<CardEntity?> GetRandomActiveCardAsync(int userId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<CardEntity>> GetAllByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<int> DeleteAllByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}