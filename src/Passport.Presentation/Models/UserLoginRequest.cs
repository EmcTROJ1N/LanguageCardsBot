namespace Passport.Presentation.Models;

/// <summary>
/// HTTP request body for user login.
/// </summary>
/// <param name="Email">User email.</param>
/// <param name="Password">User password.</param>
public sealed record UserLoginRequest(string Email, string Password);
