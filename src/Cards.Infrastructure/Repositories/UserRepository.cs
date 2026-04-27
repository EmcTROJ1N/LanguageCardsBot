using Cards.Domain.Entities;
using Cards.Infrastructure.Common.Abstractions;
using Cards.Infrastructure.Data;
using Cards.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Cards.Infrastructure.Repositories;

public class UserRepository(CardsMysqlDbContext dbContext): AbstractCrudRepository<UserEntity>(dbContext), IUserRepository
{
    public Task<UserEntity?> GetByChatIdAsync(long chatId, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<UserEntity>()
            .FirstOrDefaultAsync(x => x.ChatId == chatId, cancellationToken);
    }

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
            Username = username
        };

        await dbContext.Set<UserEntity>()
            .AddAsync(user, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return user;
    }
}