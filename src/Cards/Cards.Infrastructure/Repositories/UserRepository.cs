using Cards.Application.Abstractions.Repositories;
using Cards.Domain.Entities;
using Cards.Infrastructure.Common.Abstractions;
using Cards.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cards.Infrastructure.Repositories;

/// <summary>
/// Provides EF Core persistence operations for users.
/// </summary>
public class UserRepository(CardsMysqlDbContext dbContext): AbstractCrudRepository<UserEntity>(dbContext), IUserRepository
{
    /// <inheritdoc />
    public Task<UserEntity?> GetByChatIdAsync(long chatId, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<UserEntity>()
            .FirstOrDefaultAsync(x => x.ChatId == chatId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<UserEntity> GetOrCreateAsync(
        long chatId,
        string? username,
        CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Set<UserEntity>()
            .FirstOrDefaultAsync(x => x.ChatId == chatId, cancellationToken);
        if (user != null)
            return user;

        user = new UserEntity
        {
            ChatId = chatId,
            CreatedAt = DateTime.UtcNow,
            Username = username
        };

        await dbContext.Set<UserEntity>()
            .AddAsync(user, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return user;
    }

    /// <inheritdoc />
    public Task<UserEntity?> GetByKeycloakIdAsync(string keycloakId, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<UserEntity>()
            .FirstOrDefaultAsync(x => x.KeycloakId == keycloakId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<UserEntity> GetOrCreateByKeycloakIdAsync(
        string keycloakId,
        CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Set<UserEntity>()
            .FirstOrDefaultAsync(x => x.KeycloakId == keycloakId, cancellationToken);
        if (user != null)
            return user;

        user = new UserEntity
        {
            KeycloakId = keycloakId,
            CreatedAt = DateTime.UtcNow
        };

        await dbContext.Set<UserEntity>().AddAsync(user, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return user;
    }
}
