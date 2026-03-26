using Cards.Domain.Entities;
using Cards.Infrastructure.Interfaces;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using LanguageCardsBot.Contracts.Cards.V4;
using Mapster;

namespace Cards.Presentation.Services;

public sealed class UserGrpcService(IUserRepository userRepository) : UserService.UserServiceBase
{
    public override async Task<GetUserResponse> GetById(GetUserByIdRequest request, ServerCallContext context) =>
        new() { User = (await userRepository.GetByIdAsync(request.Id)).Adapt<User>() };

    public override async Task<GetAllUsersResponse> GetAll(GetAllUsersRequest request, ServerCallContext context)
    {
        var response = new GetAllUsersResponse();
        var users = await userRepository.GetAllAsync();
        response.Users.AddRange(users.Adapt<List<User>>());
        return response;
    }

    public override async Task<UserResponse> Add(AddUserRequest request, ServerCallContext context) =>
        new() { User = (await userRepository.AddAsync(request.User.Adapt<UserEntity>())).Adapt<User>() };

    public override async Task<UpdateUserResponse> Update(UpdateUserRequest request, ServerCallContext context)
    {
        var updatedUser = await userRepository.UpdateAsync(request.User.Adapt<UserEntity>());
        return new UpdateUserResponse { Updated = true };
    }

    public override async Task<DeleteUserResponse> Delete(DeleteUserRequest request, ServerCallContext context)
    {
        await userRepository.DeleteAsync(request.User.Id);
        return new DeleteUserResponse();
    }

    public override async Task<GetUserResponse> GetByChatId(GetUserByChatIdRequest request, ServerCallContext context)
    {
        var user = await userRepository.GetByChatIdAsync(request.ChatId);
        return new GetUserResponse { User = user.Adapt<User>() };
    }

    public override async Task<UserResponse> GetOrCreate(GetOrCreateUserRequest request, ServerCallContext context)
    {
        var user = await userRepository.GetOrCreateAsync(
            request.ChatId,
            request.Username);

        return new UserResponse { User = user.Adapt<User>() };
    }

    public override async Task<UserResponse> GetOrCreateAndSyncUsername(GetOrCreateAndSyncUsernameRequest request, ServerCallContext context)
    {
        var user = await userRepository.GetOrCreateAsync(
            request.ChatId,
            request.Username);

        return new UserResponse { User = user.Adapt<User>() };
    }

    public override async Task<UpdateNextReminderAtUtcResponse> UpdateNextReminderAtUtc(UpdateNextReminderAtUtcRequest request, ServerCallContext context)
    {
        throw new NotImplementedException();
        /*var nextReminderAtUtc = request.NextReminderAtUtc?.ToDateTime();

        var user = await userRepository.UpdateNextReminderAtUtcAsync(
            request.UserId,
            nextReminderAtUtc);

        return new UpdateNextReminderAtUtcResponse
        {
            User = user.Adapt<User>()
        };*/
    }
}