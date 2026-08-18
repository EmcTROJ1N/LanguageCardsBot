using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Passport.Application.Abstractions.Services;
using Passport.Application.Models;
using Passport.Presentation.Models;

namespace Passport.Presentation.Controllers;

/// <summary>
/// Exposes passport authentication use cases through the public REST API.
/// </summary>
[ApiController]
[Route("api/passport/v1/auth")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    /// <summary>
    /// Регистрация нового пользователя.
    /// </summary>
    /// <param name="request">Запрос на регистрацию пользователя.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>No content when the user was registered.</returns>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(
        [FromBody] UserRegistrationRequest request,
        CancellationToken ct)
    {
        try
        {
            var registered = await authService.RegisterAsync(
                new RegisterUserCommand(
                    request.Email,
                    request.Password,
                    request.FirstName,
                    request.LastName),
                ct);

            return registered
                ? NoContent()
                : BadRequest(new { error = "User with the same email already exists." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Вход в систему.
    /// </summary>
    /// <param name="request">Запрос на вход.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Issued access and refresh tokens.</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthTokenResponse>> Login(
        [FromBody] UserLoginRequest request,
        CancellationToken ct)
    {
        AuthToken? token;
        try
        {
            token = await authService.LoginAsync(new LoginUserCommand(request.Email, request.Password), ct);
        }
        catch (ArgumentException)
        {
            token = null;
        }

        return token is null
            ? Unauthorized()
            : Ok(ToResponse(token));
    }

    /// <summary>
    /// Обновление токена доступа.
    /// </summary>
    /// <param name="refreshToken">Refresh токен.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Новые токены доступа.</returns>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthTokenResponse>> RefreshToken(
        [FromBody] string refreshToken,
        CancellationToken ct)
    {
        AuthToken? token;
        try
        {
            token = await authService.RefreshTokenAsync(refreshToken, ct);
        }
        catch (ArgumentException)
        {
            token = null;
        }

        return token is null
            ? Unauthorized()
            : Ok(ToResponse(token));
    }

    /// <summary>
    /// Получить информацию о текущем пользователе.
    /// </summary>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Информация о пользователе.</returns>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponseModel>> GetCurrentUser(CancellationToken ct)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { error = "Invalid user token." });

        var user = await authService.GetUserByIdAsync(userId, ct);
        return user is null
            ? NotFound()
            : Ok(ToResponse(user));
    }

    // TODO: use mapster
    /// <summary>
    /// Maps an application token DTO to the HTTP response model.
    /// </summary>
    /// <param name="token">Application token DTO.</param>
    /// <returns>HTTP response model.</returns>
    private static AuthTokenResponse ToResponse(AuthToken token)
    {
        return new AuthTokenResponse(
            token.AccessToken,
            token.RefreshToken,
            token.ExpiresIn,
            token.TokenType);
    }

    /// <summary>
    /// Maps an application user DTO to the HTTP response model.
    /// </summary>
    /// <param name="user">Application user DTO.</param>
    /// <returns>HTTP response model.</returns>
    private static UserResponseModel ToResponse(AuthUser user)
    {
        return new UserResponseModel(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role,
            user.CreatedAt);
    }
}
