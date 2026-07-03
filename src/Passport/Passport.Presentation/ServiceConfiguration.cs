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
    public static IServiceCollection AddPassportSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Info = new OpenApiInfo
                {
                    Title = "LanguageCardsBot Passport API",
                    Version = "v1",
                    Description = "Authentication and user management service for LanguageCardsBot."
                };

                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
                document.Components.SecuritySchemes[JwtBearerDefaults.AuthenticationScheme] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    In = ParameterLocation.Header,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Enter your JWT access token."
                };

                return Task.CompletedTask;
            });
        });

        return services;
    }
}
