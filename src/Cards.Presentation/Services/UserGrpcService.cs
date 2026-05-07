using Cards.Domain.Entities;
using Cards.Infrastructure.Interfaces;
using Grpc.Core;
using LanguageCardsBot.Contracts.Cards.V3;

namespace Cards.Presentation.Services;

public sealed class UserGrpcService(IUserRepository userRepository) : UserService.UserServiceBase
{
    public override async Task<GetUserResponse> GetById(GetUserByIdRequest request, ServerCallContext context)
    {
        var user = await userRepository.GetByIdAsync(request.Id, context.CancellationToken);
        return user is null
            ? new GetUserResponse()
            : new GetUserResponse { User = user.ToGrpcUser() };
    }

    public override async Task<GetAllUsersResponse> GetAll(GetAllUsersRequest request, ServerCallContext context)
    {
        var response = new GetAllUsersResponse();
        var users = await userRepository.GetAllAsync(context.CancellationToken);
        response.Users.AddRange(users.Select(x => x.ToGrpcUser()));
        return response;
    }

    public override async Task<UserResponse> Add(AddUserRequest request, ServerCallContext context)
    {
        var entity = request.User.ToUserEntity();
        if (entity.CreatedAt == default)
            entity.CreatedAt = DateTime.UtcNow;

        var user = await userRepository.AddAsync(entity, context.CancellationToken);
        return new UserResponse { User = user.ToGrpcUser() };
    }

    public override async Task<UpdateUserResponse> Update(UpdateUserRequest request, ServerCallContext context)
    {
        var existingUser = await userRepository.GetByIdAsync(request.User.Id, context.CancellationToken);
        if (existingUser is null)
            return new UpdateUserResponse { Updated = false };

        existingUser.ChatId = request.User.ChatId;
        existingUser.Username = string.IsNullOrWhiteSpace(request.User.Username) ? null : request.User.Username;
        existingUser.ReminderIntervalMinutes = Math.Max(1, request.User.ReminderIntervalMinutes);
        existingUser.NextReminderAtUtc = request.User.NextReminderAtUtc?.ToDateTime();
        existingUser.HideTranslations = request.User.HideTranslations;

        await userRepository.UpdateAsync(existingUser, context.CancellationToken);
        return new UpdateUserResponse { Updated = true };
    }

    public override async Task<DeleteUserResponse> Delete(DeleteUserRequest request, ServerCallContext context)
    {
        var user = await userRepository.GetByIdAsync(request.User.Id, context.CancellationToken);
        if (user is null)
            return new DeleteUserResponse { Deleted = false };

        await userRepository.DeleteAsync(request.User.Id, context.CancellationToken);
        return new DeleteUserResponse { Deleted = true };
    }

    public override async Task<GetUserResponse> GetByChatId(GetUserByChatIdRequest request, ServerCallContext context)
    {
        var user = await userRepository.GetByChatIdAsync(request.ChatId, context.CancellationToken);
        return user is null
            ? new GetUserResponse()
            : new GetUserResponse { User = user.ToGrpcUser() };
    }

    public override async Task<UserResponse> GetOrCreate(GetOrCreateUserRequest request, ServerCallContext context)
    {
        var user = await userRepository.GetOrCreateAsync(
            request.ChatId,
            NormalizeUsername(request.Username),
            context.CancellationToken);

        return new UserResponse { User = user.ToGrpcUser() };
    }

    public override async Task<UserResponse> GetOrCreateAndSyncUsername(GetOrCreateAndSyncUsernameRequest request, ServerCallContext context)
    {
        var username = NormalizeUsername(request.Username);
        var user = await userRepository.GetOrCreateAsync(
            request.ChatId,
            username,
            context.CancellationToken);

        if (!string.Equals(user.Username, username, StringComparison.Ordinal))
        {
            user.Username = username;
            await userRepository.UpdateAsync(user, context.CancellationToken);
        }

        return new UserResponse { User = user.ToGrpcUser() };
    }

    public override async Task<UpdateNextReminderAtUtcResponse> UpdateNextReminderAtUtc(UpdateNextReminderAtUtcRequest request, ServerCallContext context)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, context.CancellationToken);
        if (user is null)
            return new UpdateNextReminderAtUtcResponse { Updated = false };

        user.NextReminderAtUtc = request.NextReminderAtUtc?.ToDateTime();
        await userRepository.UpdateAsync(user, context.CancellationToken);

        return new UpdateNextReminderAtUtcResponse { Updated = true };
    }

    private static string? NormalizeUsername(string? username)
    {
        return string.IsNullOrWhiteSpace(username) ? null : username.Trim();
    }
}
