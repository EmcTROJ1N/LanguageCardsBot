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
}
