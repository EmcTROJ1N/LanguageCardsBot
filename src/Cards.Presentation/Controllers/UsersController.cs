using Cards.Application.Users;
using Cards.Presentation.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Cards.Presentation.Controllers;

[ApiController]
[Route("api/cards/v3/users")]
public sealed class UsersController(IUserApplicationService userApplicationService) : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<ActionResult<GetUserResponseDto>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var user = await userApplicationService.GetByIdAsync(id, cancellationToken);
        return Ok(new GetUserResponseDto(user?.ToDto()));
    }

    [HttpGet]
    public async Task<ActionResult<GetUsersResponseDto>> GetAll(CancellationToken cancellationToken)
    {
        var users = await userApplicationService.GetAllAsync(cancellationToken);
        return Ok(new GetUsersResponseDto(users.Select(x => x.ToDto()).ToList()));
    }

    [HttpGet("by-chat/{chatId:long}")]
    public async Task<ActionResult<GetUserResponseDto>> GetByChatId(
        long chatId,
        CancellationToken cancellationToken)
    {
        var user = await userApplicationService.GetByChatIdAsync(chatId, cancellationToken);
        return Ok(new GetUserResponseDto(user?.ToDto()));
    }

    [HttpPost]
    public async Task<ActionResult<UserResponseDto>> Add(
        UserRequestDto request,
        CancellationToken cancellationToken)
    {
        var user = await userApplicationService.AddAsync(ToCommand(0, request), cancellationToken);
        return Ok(new UserResponseDto(user.ToDto()));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<UpdateUserResponseDto>> Update(
        int id,
        UserRequestDto request,
        CancellationToken cancellationToken)
    {
        var updated = await userApplicationService.UpdateAsync(ToCommand(id, request), cancellationToken);
        return Ok(new UpdateUserResponseDto(updated));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<DeleteUserResponseDto>> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var deleted = await userApplicationService.DeleteAsync(id, cancellationToken);
        return Ok(new DeleteUserResponseDto(deleted));
    }

    [HttpPost("get-or-create")]
    public async Task<ActionResult<UserResponseDto>> GetOrCreate(
        GetOrCreateUserRequestDto request,
        CancellationToken cancellationToken)
    {
        var user = await userApplicationService.GetOrCreateAsync(
            request.ChatId,
            request.Username,
            cancellationToken);

        return Ok(new UserResponseDto(user.ToDto()));
    }

    [HttpPost("get-or-create-and-sync-username")]
    public async Task<ActionResult<UserResponseDto>> GetOrCreateAndSyncUsername(
        GetOrCreateUserRequestDto request,
        CancellationToken cancellationToken)
    {
        var user = await userApplicationService.GetOrCreateAndSyncUsernameAsync(
            request.ChatId,
            request.Username,
            cancellationToken);

        return Ok(new UserResponseDto(user.ToDto()));
    }

    [HttpPatch("{userId:int}/next-reminder")]
    public async Task<ActionResult<UpdateNextReminderAtUtcResponseDto>> UpdateNextReminderAtUtc(
        int userId,
        UpdateNextReminderAtUtcRequestDto request,
        CancellationToken cancellationToken)
    {
        var updated = await userApplicationService.UpdateNextReminderAtUtcAsync(
            userId,
            request.NextReminderAtUtc,
            cancellationToken);

        return Ok(new UpdateNextReminderAtUtcResponseDto(updated));
    }

    //TODO: use mapster
    private static UserCommand ToCommand(int id, UserRequestDto request)
    {
        return new UserCommand(
            id,
            request.ChatId,
            request.Username,
            request.CreatedAt,
            request.ReminderIntervalMinutes,
            request.NextReminderAtUtc,
            request.HideTranslations);
    }
}
