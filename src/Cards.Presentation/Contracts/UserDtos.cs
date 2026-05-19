using Cards.Domain.Entities;

namespace Cards.Presentation.Contracts;

/// <summary>
/// Represents a user returned by the REST API.
/// </summary>
public sealed record UserDto(
    int Id,
    long ChatId,
    string? Username,
    DateTime CreatedAt,
    int ReminderIntervalMinutes,
    DateTime? NextReminderAtUtc,
    bool HideTranslations);

/// <summary>
/// Represents a REST response containing an optional user.
/// </summary>
public sealed record GetUserResponseDto(UserDto? User);

/// <summary>
/// Represents a REST response containing a user collection.
/// </summary>
public sealed record GetUsersResponseDto(IReadOnlyCollection<UserDto> Users);

/// <summary>
/// Represents a REST response containing a user.
/// </summary>
public sealed record UserResponseDto(UserDto User);

/// <summary>
/// Represents the REST request body for creating or updating a user.
/// </summary>
public sealed class UserRequestDto
{
    public long ChatId { get; init; }
    public string? Username { get; init; }
    public DateTime? CreatedAt { get; init; }
    public int ReminderIntervalMinutes { get; init; } = 1;
    public DateTime? NextReminderAtUtc { get; init; }
    public bool HideTranslations { get; init; } = true;
}

/// <summary>
/// Represents the REST request body for get-or-create user operations.
/// </summary>
public sealed class GetOrCreateUserRequestDto
{
    public long ChatId { get; init; }
    public string? Username { get; init; }
}

/// <summary>
/// Represents the REST request body for updating the next reminder timestamp.
/// </summary>
public sealed class UpdateNextReminderAtUtcRequestDto
{
    public DateTime? NextReminderAtUtc { get; init; }
}

/// <summary>
/// Represents the REST response for a user update operation.
/// </summary>
public sealed record UpdateUserResponseDto(bool Updated);

/// <summary>
/// Represents the REST response for a user delete operation.
/// </summary>
public sealed record DeleteUserResponseDto(bool Deleted);

/// <summary>
/// Represents the REST response for updating the next reminder timestamp.
/// </summary>
public sealed record UpdateNextReminderAtUtcResponseDto(bool Updated);

/// <summary>
/// Provides mapping helpers for REST user DTOs.
/// </summary>
internal static partial class ApiMappingExtensions
{
    /// <summary>
    /// Converts a user entity to a REST DTO.
    /// </summary>
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
