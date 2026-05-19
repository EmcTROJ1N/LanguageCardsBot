using Cards.Application.Users;
using Grpc.Core;
using LanguageCardsBot.Contracts.Cards.V3;

namespace Cards.Presentation.Services;

public sealed class UserGrpcService(IUserApplicationService userApplicationService) : UserService.UserServiceBase
{
    public override async Task<GetUserResponse> GetById(GetUserByIdRequest request, ServerCallContext context)
    {
        var user = await userApplicationService.GetByIdAsync(request.Id, context.CancellationToken);
        return user is null
            ? new GetUserResponse()
            : new GetUserResponse { User = user.ToGrpcUser() };
    }

    public override async Task<GetAllUsersResponse> GetAll(GetAllUsersRequest request, ServerCallContext context)
    {
        var response = new GetAllUsersResponse();
        var users = await userApplicationService.GetAllAsync(context.CancellationToken);
        response.Users.AddRange(users.Select(x => x.ToGrpcUser()));
        return response;
    }

    public override async Task<UserResponse> Add(AddUserRequest request, ServerCallContext context)
    {
        var user = await userApplicationService.AddAsync(ToCommand(request.User), context.CancellationToken);
        return new UserResponse { User = user.ToGrpcUser() };
    }

    public override async Task<UpdateUserResponse> Update(UpdateUserRequest request, ServerCallContext context)
    {
        var updated = await userApplicationService.UpdateAsync(ToCommand(request.User), context.CancellationToken);
        return new UpdateUserResponse { Updated = updated };
    }

    public override async Task<DeleteUserResponse> Delete(DeleteUserRequest request, ServerCallContext context)
    {
        var deleted = await userApplicationService.DeleteAsync(request.User.Id, context.CancellationToken);
        return new DeleteUserResponse { Deleted = deleted };
    }

    public override async Task<GetUserResponse> GetByChatId(GetUserByChatIdRequest request, ServerCallContext context)
    {
        var user = await userApplicationService.GetByChatIdAsync(request.ChatId, context.CancellationToken);
        return user is null
            ? new GetUserResponse()
            : new GetUserResponse { User = user.ToGrpcUser() };
    }

    public override async Task<UserResponse> GetOrCreate(GetOrCreateUserRequest request, ServerCallContext context)
    {
        var user = await userApplicationService.GetOrCreateAsync(
            request.ChatId,
            request.HasUsername ? request.Username : null,
            context.CancellationToken);

        return new UserResponse { User = user.ToGrpcUser() };
    }

    public override async Task<UserResponse> GetOrCreateAndSyncUsername(GetOrCreateAndSyncUsernameRequest request, ServerCallContext context)
    {
        var user = await userApplicationService.GetOrCreateAndSyncUsernameAsync(
            request.ChatId,
            request.HasUsername ? request.Username : null,
            context.CancellationToken);

        return new UserResponse { User = user.ToGrpcUser() };
    }

    public override async Task<UpdateNextReminderAtUtcResponse> UpdateNextReminderAtUtc(UpdateNextReminderAtUtcRequest request, ServerCallContext context)
    {
        var updated = await userApplicationService.UpdateNextReminderAtUtcAsync(
            request.UserId,
            request.NextReminderAtUtc?.ToDateTime(),
            context.CancellationToken);

        return new UpdateNextReminderAtUtcResponse { Updated = updated };
    }

    private static UserCommand ToCommand(User user)
    {
        return new UserCommand(
            user.Id,
            user.ChatId,
            user.HasUsername ? user.Username : null,
            user.CreatedAt?.ToDateTime(),
            user.ReminderIntervalMinutes,
            user.NextReminderAtUtc?.ToDateTime(),
            user.HideTranslations);
    }
}
