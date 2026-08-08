namespace Cards.Contracts.Rest.Users;

/// <summary>
/// Represents the REST request body for get-or-create user operations.
/// </summary>
public sealed class GetOrCreateUserRequestDto
{
    /// <summary>Gets the Telegram chat identifier used to look up or create the user.</summary>
    public long ChatId { get; init; }
    /// <summary>Gets the optional Telegram username set when creating a new user.</summary>
    public string? Username { get; init; }
}
