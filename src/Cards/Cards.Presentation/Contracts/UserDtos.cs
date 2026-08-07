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
    /// <summary>Gets the Telegram chat identifier associated with the user.</summary>
    public long ChatId { get; init; }
    /// <summary>Gets the optional Telegram username.</summary>
    public string? Username { get; init; }
    /// <summary>Gets the timestamp when the user was created (UTC).</summary>
    public DateTime? CreatedAt { get; init; }
    /// <summary>Gets the interval between reminder notifications, in minutes.</summary>
    public int ReminderIntervalMinutes { get; init; } = 1;
    /// <summary>Gets the next scheduled reminder timestamp (UTC).</summary>
    public DateTime? NextReminderAtUtc { get; init; }
    /// <summary>Gets a value indicating whether translations should be hidden in the UI by default.</summary>
    public bool HideTranslations { get; init; } = true;
}

/// <summary>
/// Represents the REST request body for get-or-create user operations.
/// </summary>
public sealed class GetOrCreateUserRequestDto
{
    /// <summary>Gets the Telegram chat identifier used to look up or create the user.</summary>
    public long ChatId { get; init; }
    /// <summary>Gets the optional Telegram username set when creating a new user.</summary>
    public string? Username { get; init; }
}

/// <summary>
/// Represents the REST request body for updating the next reminder timestamp.
/// </summary>
public sealed class UpdateNextReminderAtUtcRequestDto
{
    /// <summary>Gets the new next-reminder timestamp (UTC); <c>null</c> disables the reminder.</summary>
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
