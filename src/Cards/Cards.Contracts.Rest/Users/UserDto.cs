namespace Cards.Contracts.Rest.Users;

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
