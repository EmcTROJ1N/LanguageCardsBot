using Cards.Domain.Common;

namespace Cards.Domain.Entities;

/// <summary>
/// Represents a user in the cards system.
/// </summary>
public class UserEntity : IEntityWithId
{
    /// <summary>Gets or sets the internal integer primary key.</summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the Keycloak subject claim (UUID string).
    /// Primary identity for web users. Null for Telegram-only users created before web integration.
    /// </summary>
    public string? KeycloakId { get; set; }

    /// <summary>
    /// Gets or sets the Telegram chat identifier.
    /// Optional — set only when the user links their Telegram account.
    /// </summary>
    public long? ChatId { get; set; }

    /// <summary>Gets or sets the display username.</summary>
    public string? Username { get; set; }

    /// <summary>Gets or sets the UTC timestamp when this user was created.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Gets or sets the interval between Telegram reminders in minutes.</summary>
    public int ReminderIntervalMinutes { get; set; } = 1;

    /// <summary>Gets or sets the next scheduled reminder timestamp (UTC).</summary>
    public DateTime? NextReminderAtUtc { get; set; }

    /// <summary>Gets or sets a value indicating whether translations are hidden by default.</summary>
    public bool HideTranslations { get; set; } = true;

    /// <summary>
    /// Computes and stores the next reminder time based on the user's configured interval.
    /// </summary>
    /// <param name="sentAtUtc">UTC timestamp of the just-sent reminder.</param>
    /// <returns>The newly computed <see cref="NextReminderAtUtc"/> value.</returns>
    public DateTime? ScheduleNextReminder(DateTime sentAtUtc)
    {
        if (ReminderIntervalMinutes == 0)
        {
            NextReminderAtUtc = null;
            return null;
        }

        NextReminderAtUtc = sentAtUtc.AddMinutes(Math.Max(1, ReminderIntervalMinutes));
        return NextReminderAtUtc.Value;
    }
}
