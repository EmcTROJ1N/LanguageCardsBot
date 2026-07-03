using Passport.Application.Models;

namespace Passport.Application.Abstractions.Clients;

/// <summary>
/// Issues and refreshes Keycloak tokens on behalf of a user.
/// </summary>
public interface IKeycloakTokenClient
{
    /// <summary>
    /// Signs in with user credentials and returns issued tokens.
    /// </summary>
    /// <param name="email">Normalized user email address.</param>
    /// <param name="password">User password.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Issued tokens, or <c>null</c> when credentials are invalid.</returns>
    Task<AuthToken?> SignInAsync(string email, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Requests a new token pair using a refresh token.
    /// </summary>
    /// <param name="refreshToken">Refresh token previously issued by Keycloak.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Newly issued tokens, or <c>null</c> when the refresh token is invalid or expired.</returns>
    Task<AuthToken?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}
