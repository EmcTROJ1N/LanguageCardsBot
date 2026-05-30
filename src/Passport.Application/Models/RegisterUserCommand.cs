namespace Passport.Application.Models;

/// <summary>
/// Describes a user registration request handled by the passport application service.
/// </summary>
/// <param name="Email">User email.</param>
/// <param name="Password">User password.</param>
/// <param name="FirstName">User first name.</param>
/// <param name="LastName">User last name.</param>
public sealed record RegisterUserCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName);
