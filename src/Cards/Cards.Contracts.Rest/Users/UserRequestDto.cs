namespace Cards.Contracts.Rest.Users;

/// <summary>Represents the REST request body for creating or updating a user.</summary>
public sealed class UserRequestDto
{
    /// <summary>Gets the Keycloak subject claim. Required for web users.</summary>
    public string? KeycloakId { get; init; }
    /// <summary>Gets the optional Telegram chat identifier.</summary>
    public long? ChatId { get; init; }
    /// <summary>Gets the optional display username.</summary>
    public string? Username { get; init; }
    /// <summary>Gets the UTC timestamp when the user was created.</summary>
    public DateTime? CreatedAt { get; init; }
    /// <summary>Gets the interval between reminder notifications, in minutes.</summary>
    public int ReminderIntervalMinutes { get; init; } = 1;
    /// <summary>Gets the next scheduled reminder timestamp (UTC).</summary>
    public DateTime? NextReminderAtUtc { get; init; }
    /// <summary>Gets a value indicating whether translations should be hidden in the UI by default.</summary>
    public bool HideTranslations { get; init; } = true;
}
