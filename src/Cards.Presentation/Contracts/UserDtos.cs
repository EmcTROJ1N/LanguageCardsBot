using Cards.Domain.Entities;

namespace Cards.Presentation.Contracts;

public sealed record UserDto(
    int Id,
    long ChatId,
    string? Username,
    DateTime CreatedAt,
    int ReminderIntervalMinutes,
    DateTime? NextReminderAtUtc,
    bool HideTranslations);

public sealed record GetUserResponseDto(UserDto? User);

public sealed record GetUsersResponseDto(IReadOnlyCollection<UserDto> Users);

public sealed record UserResponseDto(UserDto User);

public sealed class UserRequestDto
{
    public long ChatId { get; init; }
    public string? Username { get; init; }
    public DateTime? CreatedAt { get; init; }
    public int ReminderIntervalMinutes { get; init; } = 1;
    public DateTime? NextReminderAtUtc { get; init; }
    public bool HideTranslations { get; init; } = true;
}

public sealed class GetOrCreateUserRequestDto
{
    public long ChatId { get; init; }
    public string? Username { get; init; }
}

public sealed class UpdateNextReminderAtUtcRequestDto
{
    public DateTime? NextReminderAtUtc { get; init; }
}

public sealed record UpdateUserResponseDto(bool Updated);

public sealed record DeleteUserResponseDto(bool Deleted);

public sealed record UpdateNextReminderAtUtcResponseDto(bool Updated);

internal static partial class ApiMappingExtensions
{
    public static UserDto ToDto(this UserEntity entity)
    {
        return new UserDto(
            entity.Id,
            entity.ChatId,
            entity.Username,
            entity.CreatedAt,
            entity.ReminderIntervalMinutes,
            entity.NextReminderAtUtc,
            entity.HideTranslations);
    }
}
