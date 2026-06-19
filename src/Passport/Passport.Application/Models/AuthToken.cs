namespace Passport.Application.Models;

/// <summary>
/// Represents access credentials returned by the passport authentication flow.
/// </summary>
/// <param name="AccessToken">Access token used to authenticate API requests.</param>
/// <param name="RefreshToken">Refresh token used to request a new access token.</param>
/// <param name="ExpiresIn">Access token lifetime in seconds.</param>
/// <param name="TokenType">Token type expected in the Authorization header.</param>
public sealed record AuthToken(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    string TokenType = "Bearer");
