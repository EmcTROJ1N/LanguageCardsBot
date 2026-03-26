using Cards.Domain.Entities;
using Cards.Infrastructure.Common.Abstractions;
using Cards.Infrastructure.Interfaces;

namespace Cards.Infrastructure.Repositories;

public class UserRepository: AbstractCrudRepository<UserEntity>, IUserRepository
{
    public Task<UserEntity?> GetByChatIdAsync(long chatId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<UserEntity> GetOrCreateAsync(long chatId, string? username, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}