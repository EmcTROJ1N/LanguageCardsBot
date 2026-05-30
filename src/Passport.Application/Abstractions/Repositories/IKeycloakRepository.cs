using Passport.Application.Models;

namespace Passport.Application.Abstractions.Repositories;

/// <summary>
/// Provides an abstraction over Keycloak user and token operations.
/// </summary>
public interface IKeycloakRepository
{
    /// <summary>
    /// Checks whether a user with the specified email exists.
    /// </summary>
    /// <param name="email">Normalized user email.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> when a user exists; otherwise <c>false</c>.</returns>
    Task<bool> UserExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a user in the authentication provider.
    /// </summary>
    /// <param name="email">Normalized user email.</param>
    /// <param name="password">User password.</param>
    /// <param name="firstName">User first name.</param>
    /// <param name="lastName">User last name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created user, or <c>null</c> when the email already exists.</returns>
    Task<AuthUser?> CreateUserAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticates a user and issues tokens.
    /// </summary>
    /// <param name="email">Normalized user email.</param>
    /// <param name="password">User password.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Issued tokens, or <c>null</c> when credentials are invalid.</returns>
    Task<AuthToken?> SignInAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes an access token using a refresh token.
    /// </summary>
    /// <param name="refreshToken">Refresh token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Newly issued tokens, or <c>null</c> when the refresh token is invalid.</returns>
    Task<AuthToken?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by identifier.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>User profile, or <c>null</c> when the user does not exist.</returns>
    Task<AuthUser?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by an issued access token.
    /// </summary>
    /// <param name="accessToken">Access token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>User profile, or <c>null</c> when the token is invalid.</returns>
    Task<AuthUser?> GetUserByAccessTokenAsync(string accessToken, CancellationToken cancellationToken = default);
}
