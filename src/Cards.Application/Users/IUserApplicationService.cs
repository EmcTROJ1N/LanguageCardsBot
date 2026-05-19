using Cards.Domain.Entities;

namespace Cards.Application.Users;

public interface IUserApplicationService
{
    Task<UserEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<UserEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UserEntity> AddAsync(UserCommand command, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(UserCommand command, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<UserEntity?> GetByChatIdAsync(long chatId, CancellationToken cancellationToken = default);
    Task<UserEntity> GetOrCreateAsync(long chatId, string? username, CancellationToken cancellationToken = default);
    Task<UserEntity> GetOrCreateAndSyncUsernameAsync(long chatId, string? username, CancellationToken cancellationToken = default);
    Task<bool> UpdateNextReminderAtUtcAsync(int userId, DateTime? nextReminderAtUtc, CancellationToken cancellationToken = default);
}
