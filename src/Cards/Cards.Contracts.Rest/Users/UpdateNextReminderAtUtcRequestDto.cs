namespace Cards.Contracts.Rest.Users;

/// <summary>
/// Represents the REST request body for updating the next reminder timestamp.
/// </summary>
public sealed class UpdateNextReminderAtUtcRequestDto
{
    /// <summary>Gets the new next-reminder timestamp (UTC); <c>null</c> disables the reminder.</summary>
    public DateTime? NextReminderAtUtc { get; init; }
}
