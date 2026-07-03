using Keycloak.AuthServices.Sdk;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Passport.Application.Abstractions.Clients;
using Passport.Application.Options;
using Passport.Infrastructure.Authentication;
using Passport.Infrastructure.Repositories;

namespace Passport.Infrastructure;

/// <summary>
/// Extension methods for registering Infrastructure-layer services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers Infrastructure-layer services into the DI container.
    /// Binds <see cref="KeycloakOptions"/> from the <c>Keycloak</c> configuration section,
    /// configures the Keycloak admin HTTP client with its auth handler, and registers
    /// the token client used for user sign-in and token refresh.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
    public static IServiceCollection AddPassportInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<KeycloakOptions>(configuration.GetSection(KeycloakOptions.SectionName));
        services.AddSingleton<KeycloakAdminAuthHandler>();
        services.AddKeycloakAdminHttpClient(configuration)
            .AddHttpMessageHandler<KeycloakAdminAuthHandler>();
        services.AddHttpClient("keycloak-token");
        services.AddScoped<IKeycloakTokenClient, KeycloakTokenClient>();
        return services;
    }
}
