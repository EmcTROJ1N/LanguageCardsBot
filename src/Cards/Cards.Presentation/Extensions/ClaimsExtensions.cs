using System.Security.Claims;

namespace Cards.Presentation.Extensions;

/// <summary>Extension methods for extracting standard claims from a <see cref="ClaimsPrincipal"/>.</summary>
public static class ClaimsExtensions
{
    /// <summary>
    /// Returns the Keycloak subject claim (<c>sub</c>) or throws if the principal is not authenticated.
    /// </summary>
    /// <exception cref="UnauthorizedAccessException">Thrown when the sub claim is missing.</exception>
    public static string GetKeycloakId(this ClaimsPrincipal principal)
    {
        var sub = principal.FindFirstValue("sub")
                  ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(sub))
            throw new UnauthorizedAccessException("JWT is missing the 'sub' claim.");

        return sub;
    }
}
