namespace EnglishCardsBot.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public long ChatId { get; set; }
    public string? Username { get; set; }
    public DateTime CreatedAt { get; set; }

    public int ReminderIntervalMinutes { get; set; } = 1;

    // NEW: когда следующее напоминание (UTC)
    public DateTime? NextReminderAtUtc { get; set; }

    public bool HideTranslations { get; set; } = true;
}
