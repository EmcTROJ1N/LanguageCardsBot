using Cards.Domain.Common;

namespace Cards.Domain.Entities;

public class UserEntity: IEntityWithId
{
    public int Id { get; set; }
    public long ChatId { get; set; }
    public string? Username { get; set; }
    public DateTime CreatedAt { get; set; }

    public int ReminderIntervalMinutes { get; set; } = 1;

    public DateTime? NextReminderAtUtc { get; set; }

    public bool HideTranslations { get; set; } = true;

    /// <summary>
    /// Computes and stores the next reminder time based on the user's configured interval.
    /// </summary>
    /// <param name="sentAtUtc">UTC timestamp of the just-sent reminder.</param>
    /// <returns>The newly computed <see cref="NextReminderAtUtc"/> value.</returns>
    public DateTime ScheduleNextReminder(DateTime sentAtUtc)
    {
        NextReminderAtUtc = sentAtUtc.AddMinutes(Math.Max(1, ReminderIntervalMinutes));
        return NextReminderAtUtc.Value;
    }
}
