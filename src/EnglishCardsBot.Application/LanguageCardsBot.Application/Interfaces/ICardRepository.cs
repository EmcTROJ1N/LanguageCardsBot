using EnglishCardsBot.Domain.Entities;

namespace EnglishCardsBot.Application.Interfaces;

public interface ICardRepository : IRepository<Card>
{
    Task<Card?> GetDueCardAsync(int userId, CancellationToken cancellationToken = default);
    Task<Card?> GetRandomActiveCardAsync(int userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Card>> GetAllByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<int> DeleteAllByUserIdAsync(int userId, CancellationToken cancellationToken = default);
}

