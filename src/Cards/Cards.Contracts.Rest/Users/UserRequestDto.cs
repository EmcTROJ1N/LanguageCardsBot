namespace Cards.Contracts.Rest.Users;

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
