using Cards.Domain.Entities;

namespace Cards.Application.Abstractions.Repositories;

public interface IUserRepository
{
    Task<UserEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UserEntity> AddAsync(UserEntity entity, CancellationToken cancellationToken = default);
    Task<UserEntity> UpdateAsync(UserEntity entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
    Task<UserEntity?> GetByChatIdAsync(long chatId, CancellationToken cancellationToken = default);
    Task<UserEntity> GetOrCreateAsync(long chatId, string? username, CancellationToken cancellationToken = default);
}
