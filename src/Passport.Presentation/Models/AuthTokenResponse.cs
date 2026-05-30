namespace Passport.Presentation.Models;

/// <summary>
/// HTTP response body containing issued authentication tokens.
/// </summary>
/// <param name="AccessToken">Access token used for authenticated API calls.</param>
/// <param name="RefreshToken">Refresh token used to get a new access token.</param>
/// <param name="ExpiresIn">Access token lifetime in seconds.</param>
/// <param name="TokenType">Token type expected in the Authorization header.</param>
public sealed record AuthTokenResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    string TokenType = "Bearer");
