namespace Passport.Presentation.Models;

/// <summary>
/// HTTP response body containing user profile data.
/// </summary>
/// <param name="Id">User identifier.</param>
/// <param name="Email">User email.</param>
/// <param name="FirstName">User first name.</param>
/// <param name="LastName">User last name.</param>
/// <param name="Role">User role.</param>
/// <param name="CreatedAt">UTC timestamp when the user was created.</param>
public sealed record UserResponseModel(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    DateTime CreatedAt);
