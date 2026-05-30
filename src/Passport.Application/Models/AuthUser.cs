namespace Passport.Application.Models;

/// <summary>
/// Represents a user profile returned by the authentication provider.
/// </summary>
/// <param name="Id">User identifier.</param>
/// <param name="Email">User email.</param>
/// <param name="FirstName">User first name.</param>
/// <param name="LastName">User last name.</param>
/// <param name="Role">User role.</param>
/// <param name="CreatedAt">UTC timestamp when the user was created.</param>
public sealed record AuthUser(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    DateTime CreatedAt);
