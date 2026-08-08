namespace Cards.Contracts.Rest.Users;

/// <summary>
/// Represents the REST response for updating the next reminder timestamp.
/// </summary>
public sealed record UpdateNextReminderAtUtcResponseDto(bool Updated);
