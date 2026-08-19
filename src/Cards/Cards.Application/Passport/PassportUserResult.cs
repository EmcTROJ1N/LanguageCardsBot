namespace Cards.Application.Passport;

/// <summary>
/// User profile returned by the Passport identity service.
/// </summary>
public sealed record PassportUserResult(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string Role);
