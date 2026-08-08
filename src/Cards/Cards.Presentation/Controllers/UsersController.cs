using Cards.Application.Users;
using Cards.Contracts.Rest.Users;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace Cards.Presentation.Controllers;

/// <summary>
/// Exposes user use cases through the public REST API.
/// </summary>
[ApiController]
[Route("api/cards/v3/users")]
public sealed class UsersController(IUserApplicationService userApplicationService) : ControllerBase
{
    /// <summary>
    /// Gets a user by its identifier.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<GetUserResponseDto>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var user = await userApplicationService.GetByIdAsync(id, cancellationToken);
        return Ok(new GetUserResponseDto(user?.Adapt<UserDto>()));
    }

    /// <summary>
    /// Gets all users.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<GetUsersResponseDto>> GetAll(CancellationToken cancellationToken)
    {
        var users = await userApplicationService.GetAllAsync(cancellationToken);
        return Ok(new GetUsersResponseDto(users.Adapt<List<UserDto>>()));
    }

    /// <summary>
    /// Gets a user by Telegram chat identifier.
    /// </summary>
    [HttpGet("by-chat/{chatId:long}")]
    public async Task<ActionResult<GetUserResponseDto>> GetByChatId(
        long chatId,
        CancellationToken cancellationToken)
    {
        var user = await userApplicationService.GetByChatIdAsync(chatId, cancellationToken);
        return Ok(new GetUserResponseDto(user?.Adapt<UserDto>()));
    }

    /// <summary>
    /// Adds a user.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<UserResponseDto>> Add(
        UserRequestDto request,
        CancellationToken cancellationToken)
    {
        var user = await userApplicationService.AddAsync(request.Adapt<UserCommand>(), cancellationToken);
        return Ok(new UserResponseDto(user.Adapt<UserDto>()));
    }

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<UpdateUserResponseDto>> Update(
        int id,
        UserRequestDto request,
        CancellationToken cancellationToken)
    {
        var updated = await userApplicationService.UpdateAsync(
            request.Adapt<UserCommand>() with { Id = id },
            cancellationToken);

        return Ok(new UpdateUserResponseDto(updated));
    }

    /// <summary>
    /// Deletes a user by its identifier.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<DeleteUserResponseDto>> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var deleted = await userApplicationService.DeleteAsync(id, cancellationToken);
        return Ok(new DeleteUserResponseDto(deleted));
    }

    /// <summary>
    /// Gets an existing user by chat identifier or creates one.
    /// </summary>
    [HttpPost("get-or-create")]
    public async Task<ActionResult<UserResponseDto>> GetOrCreate(
        GetOrCreateUserRequestDto request,
        CancellationToken cancellationToken)
    {
        var user = await userApplicationService.GetOrCreateAsync(
            request.ChatId,
            request.Username,
            cancellationToken);

        return Ok(new UserResponseDto(user.Adapt<UserDto>()));
    }

    /// <summary>
    /// Gets or creates a user and synchronizes the stored username.
    /// </summary>
    [HttpPost("get-or-create-and-sync-username")]
    public async Task<ActionResult<UserResponseDto>> GetOrCreateAndSyncUsername(
        GetOrCreateUserRequestDto request,
        CancellationToken cancellationToken)
    {
        var user = await userApplicationService.GetOrCreateAndSyncUsernameAsync(
            request.ChatId,
            request.Username,
            cancellationToken);

        return Ok(new UserResponseDto(user.Adapt<UserDto>()));
    }

    /// <summary>
    /// Updates the next reminder timestamp for a user.
    /// </summary>
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
}
