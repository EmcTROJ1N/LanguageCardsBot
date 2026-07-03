namespace Passport.Application.Options;

/// <summary>
/// Configuration options for the Keycloak identity provider used by the Passport service.
/// </summary>
public sealed class KeycloakOptions
{
    /// <summary>
    /// The configuration section name that these options are bound from.
    /// </summary>
    public const string SectionName = "Keycloak";

    /// <summary>
    /// Gets the Keycloak realm name.
    /// </summary>
    public string Realm { get; init; } = string.Empty;
}
