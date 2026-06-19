using Passport.Application.Models;

namespace Passport.Application.Abstractions.Services;

/// <summary>
/// Coordinates passport authentication use cases.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="command">Registration command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> when the user was created; otherwise <c>false</c>.</returns>
    Task<bool> RegisterAsync(RegisterUserCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticates a user and issues tokens.
    /// </summary>
    /// <param name="command">Login command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Issued tokens, or <c>null</c> when credentials are invalid.</returns>
    Task<AuthToken?> LoginAsync(LoginUserCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes an access token using a refresh token.
    /// </summary>
    /// <param name="refreshToken">Refresh token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Newly issued tokens, or <c>null</c> when the refresh token is invalid.</returns>
    Task<AuthToken?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user profile by identifier.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>User profile, or <c>null</c> when the user does not exist.</returns>
    Task<AuthUser?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);

}
