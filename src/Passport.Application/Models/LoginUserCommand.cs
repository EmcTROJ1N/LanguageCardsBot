namespace Passport.Application.Models;

/// <summary>
/// Describes a user login request handled by the passport application service.
/// </summary>
/// <param name="Email">User email.</param>
/// <param name="Password">User password.</param>
public sealed record LoginUserCommand(string Email, string Password);
