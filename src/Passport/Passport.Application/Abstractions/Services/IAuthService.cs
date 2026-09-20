using Passport.Application.Models;
using Passport.Domain.Exceptions;

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
    /// <exception cref="UserAlreadyExistsException">A user with the same email is already registered.</exception>
    Task RegisterAsync(RegisterUserCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticates a user and issues tokens.
    /// </summary>
    /// <param name="command">Login command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Issued tokens.</returns>
    /// <exception cref="InvalidCredentialsException">The provided credentials are invalid.</exception>
    Task<AuthToken> LoginAsync(LoginUserCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes an access token using a refresh token.
    /// </summary>
    /// <param name="refreshToken">Refresh token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Newly issued tokens.</returns>
    /// <exception cref="InvalidRefreshTokenException">The refresh token is expired or invalid.</exception>
    Task<AuthToken> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user profile by identifier.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>User profile, or <c>null</c> when the user does not exist.</returns>
    Task<AuthUser?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
