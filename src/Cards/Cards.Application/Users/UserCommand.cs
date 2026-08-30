namespace Cards.Application.Users;

/// <summary>
/// Represents user fields accepted by user mutation use cases.
/// </summary>
public sealed record UserCommand(
    int Id,
    string? KeycloakId,
    long? ChatId,
    string? Username,
    DateTime? CreatedAt,
    int ReminderIntervalMinutes,
    DateTime? NextReminderAtUtc,
    bool HideTranslations);
