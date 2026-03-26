using Cards.Domain.Entities;
using Cards.Infrastructure.Common.Interfaces;

namespace Cards.Infrastructure.Interfaces;

public interface IUserRepository: ICrudRepository<UserEntity>
{
    Task<UserEntity?> GetByChatIdAsync(long chatId, CancellationToken cancellationToken = default);
    Task<UserEntity> GetOrCreateAsync(long chatId, string? username, CancellationToken cancellationToken = default);
}