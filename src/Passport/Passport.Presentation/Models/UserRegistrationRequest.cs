namespace Passport.Presentation.Models;

/// <summary>
/// HTTP request body for registering a new user.
/// </summary>
/// <param name="Email">User email.</param>
/// <param name="Password">User password.</param>
/// <param name="FirstName">User first name.</param>
/// <param name="LastName">User last name.</param>
public sealed record UserRegistrationRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName);
