using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Passport.Application.Abstractions.Services;
using Passport.Application.Models;

namespace Passport.Presentation.Controllers.Generated;

/// <summary>
/// Exposes passport authentication use cases through the public REST API.
/// </summary>
public sealed class PassportController(IAuthService authService) : ControllerBaseControllerBase
{
    /// <inheritdoc/>
    public override async Task RegisterUser(UserRegistrationRequest body, CancellationToken cancellationToken = default)
    {
        await authService.RegisterAsync(
            new RegisterUserCommand(body.Email, body.Password, body.FirstName, body.LastName),
            cancellationToken);

        Response.StatusCode = 204;
    }

    /// <inheritdoc/>
    public override async Task<AuthTokenResponse> LoginUser(UserLoginRequest body, CancellationToken cancellationToken = default)
    {
        var token = await authService.LoginAsync(new LoginUserCommand(body.Email, body.Password), cancellationToken);
        return MapToResponse(token);
    }

    /// <inheritdoc/>
    public override async Task<AuthTokenResponse> RefreshToken(string body, CancellationToken cancellationToken = default)
    {
        var token = await authService.RefreshTokenAsync(body, cancellationToken);
        return MapToResponse(token);
    }

    /// <inheritdoc/>
    [Authorize]
    public override async Task<UserResponseModel> GetCurrentUser(CancellationToken cancellationToken = default)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            Response.StatusCode = 401;
            return null!;
        }

        var user = await authService.GetUserByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            Response.StatusCode = 404;
            return null!;
        }

        return MapToResponse(user);
    }

    private static AuthTokenResponse MapToResponse(AuthToken token) => new()
    {
        AccessToken = token.AccessToken,
        RefreshToken = token.RefreshToken,
        ExpiresIn = token.ExpiresIn,
        TokenType = token.TokenType,
    };

    private static UserResponseModel MapToResponse(AuthUser user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Role = user.Role,
        CreatedAt = new DateTimeOffset(user.CreatedAt, TimeSpan.Zero),
    };
}
