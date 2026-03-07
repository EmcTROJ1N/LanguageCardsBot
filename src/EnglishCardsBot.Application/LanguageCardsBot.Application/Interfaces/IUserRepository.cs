using EnglishCardsBot.Domain.Entities;

namespace EnglishCardsBot.Application.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByChatIdAsync(long chatId, CancellationToken cancellationToken = default);
    Task<User> GetOrCreateAsync(long chatId, string? username, CancellationToken cancellationToken = default);
}

