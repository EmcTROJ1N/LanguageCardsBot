using Cards.Application.Users;
using Grpc.Core;
using LanguageCardsBot.Contracts.Cards.V3;
using Mapster;

namespace Cards.Presentation.Services;

/// <summary>
/// Adapts user gRPC requests to shared user application use cases.
/// </summary>
public sealed class UserGrpcService(IUserApplicationService userApplicationService) : UserService.UserServiceBase
{
    /// <summary>
    /// Handles a gRPC request to get a user by identifier.
    /// </summary>
    public override async Task<GetUserResponse> GetById(GetUserByIdRequest request, ServerCallContext context)
    {
        var user = await userApplicationService.GetByIdAsync(request.Id, context.CancellationToken);
        return user is null
            ? new GetUserResponse()
            : new GetUserResponse { User = user.Adapt<User>() };
    }

    /// <summary>
    /// Handles a gRPC request to get all users.
    /// </summary>
    public override async Task<GetAllUsersResponse> GetAll(GetAllUsersRequest request, ServerCallContext context)
    {
        var response = new GetAllUsersResponse();
        var users = await userApplicationService.GetAllAsync(context.CancellationToken);
        response.Users.AddRange(users.Adapt<List<User>>());
        return response;
    }

    /// <summary>
    /// Handles a gRPC request to add a user.
    /// </summary>
    public override async Task<UserResponse> Add(AddUserRequest request, ServerCallContext context)
    {
        var user = await userApplicationService.AddAsync(request.User.Adapt<UserCommand>(), context.CancellationToken);
        return new UserResponse { User = user.Adapt<User>() };
    }

    /// <summary>
    /// Handles a gRPC request to update a user.
    /// </summary>
    public override async Task<UpdateUserResponse> Update(UpdateUserRequest request, ServerCallContext context)
    {
        var updated = await userApplicationService.UpdateAsync(request.User.Adapt<UserCommand>(), context.CancellationToken);
        return new UpdateUserResponse { Updated = updated };
    }

    /// <summary>
    /// Handles a gRPC request to delete a user.
    /// </summary>
    public override async Task<DeleteUserResponse> Delete(DeleteUserRequest request, ServerCallContext context)
    {
        var deleted = await userApplicationService.DeleteAsync(request.User.Id, context.CancellationToken);
        return new DeleteUserResponse { Deleted = deleted };
    }

    /// <summary>
    /// Handles a gRPC request to get a user by Telegram chat identifier.
    /// </summary>
    public override async Task<GetUserResponse> GetByChatId(GetUserByChatIdRequest request, ServerCallContext context)
    {
        var user = await userApplicationService.GetByChatIdAsync(request.ChatId, context.CancellationToken);
        return user is null
            ? new GetUserResponse()
            : new GetUserResponse { User = user.Adapt<User>() };
    }

    /// <summary>
    /// Handles a gRPC request to get or create a user.
    /// </summary>
    public override async Task<UserResponse> GetOrCreate(GetOrCreateUserRequest request, ServerCallContext context)
    {
        var user = await userApplicationService.GetOrCreateAsync(
            request.ChatId,
            request.HasUsername ? request.Username : null,
            context.CancellationToken);

        return new UserResponse { User = user.Adapt<User>() };
    }

    /// <summary>
    /// Handles a gRPC request to get or create a user and synchronize username.
    /// </summary>
    public override async Task<UserResponse> GetOrCreateAndSyncUsername(GetOrCreateAndSyncUsernameRequest request, ServerCallContext context)
    {
        var user = await userApplicationService.GetOrCreateAndSyncUsernameAsync(
            request.ChatId,
            request.HasUsername ? request.Username : null,
            context.CancellationToken);

        return new UserResponse { User = user.Adapt<User>() };
    }

    /// <summary>
    /// Handles a gRPC request to update a user's next reminder timestamp.
    /// </summary>
    public override async Task<UpdateNextReminderAtUtcResponse> UpdateNextReminderAtUtc(UpdateNextReminderAtUtcRequest request, ServerCallContext context)
    {
        var updated = await userApplicationService.UpdateNextReminderAtUtcAsync(
            request.UserId,
            request.NextReminderAtUtc?.ToDateTime(),
            context.CancellationToken);

        return new UpdateNextReminderAtUtcResponse { Updated = updated };
    }
}
