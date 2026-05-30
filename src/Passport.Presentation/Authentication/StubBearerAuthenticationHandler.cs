using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Passport.Application.Abstractions.Services;

namespace Passport.Presentation.Authentication;

// emctroj1n
// Admin123!

/// <summary>
/// Authenticates requests using access tokens issued by the in-memory Keycloak stub.
/// </summary>
public sealed class StubBearerAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IAuthService authService)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    /// <summary>
    /// Authentication scheme name used for local stub bearer tokens.
    /// </summary>
    public const string SchemeName = "StubBearer";

    private const string BearerPrefix = "Bearer ";

    /// <inheritdoc />
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authorization = Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(authorization))
            return AuthenticateResult.NoResult();

        if (!authorization.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase))
            return AuthenticateResult.NoResult();

        var accessToken = authorization[BearerPrefix.Length..].Trim();
        if (string.IsNullOrWhiteSpace(accessToken))
            return AuthenticateResult.Fail("Bearer token is empty.");

        var user = await authService.GetUserByAccessTokenAsync(accessToken, Context.RequestAborted);
        if (user is null)
            return AuthenticateResult.Fail("Bearer token is invalid.");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}
