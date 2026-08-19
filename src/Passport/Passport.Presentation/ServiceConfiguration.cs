using Keycloak.AuthServices.Authentication;
using Keycloak.AuthServices.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi;

namespace Passport.Presentation;

/// <summary>
/// Extension methods for registering Presentation-layer services.
/// </summary>
public static class ServiceConfiguration
{
    /// <summary>
    /// Registers Keycloak JWT bearer authentication for the web API.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Application configuration used to resolve Keycloak endpoints and realm.</param>
    /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
    public static IServiceCollection AddPassportAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddKeycloakWebApiAuthentication(configuration, options =>
        {
            options.RequireHttpsMetadata = configuration.GetValue<bool?>("RequireHttpsMetadata") ?? false;
        });
        return services;
    }

    /// <summary>
    /// Registers Keycloak authorization and the built-in <c>AdminAndUser</c> policy.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Application configuration used to resolve Keycloak endpoints and realm.</param>
    /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
    public static IServiceCollection AddPassportAuthorization(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddKeycloakAuthorization();
        services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminAndUser", builder =>
                {
                    builder
                        .RequireRealmRoles("User")
                        .RequireResourceRoles("Admin");
                });
            })
            .AddKeycloakAuthorization(configuration);

        return services;
    }

    /// <summary>
    /// Registers OpenAPI documentation and the Scalar UI for the Passport API.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
    public static IServiceCollection AddPassportOpenApiDocumentation(this IServiceCollection services)
    {
        var title = "LanguageCardsBot Passport API";
        var version = "v1";
        var description = "Authentication and user management service for LanguageCardsBot.";

        services.AddOpenApiDocument(config =>
        {
            config.DocumentName = version;
            config.Title = title;
            config.Version = version;
            config.Description = description;

            config.AddSecurity("Bearer", new NSwag.OpenApiSecurityScheme
            {
                Type = NSwag.OpenApiSecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });
            config.OperationProcessors.Add(
                new NSwag.Generation.Processors.Security.AspNetCoreOperationSecurityScopeProcessor("Bearer"));
        });

        return services;
    }
}
