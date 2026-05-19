namespace Cards.Application.Users;

public sealed record UserCommand(
    int Id,
    long ChatId,
    string? Username,
    DateTime? CreatedAt,
    int ReminderIntervalMinutes,
    DateTime? NextReminderAtUtc,
    bool HideTranslations);
