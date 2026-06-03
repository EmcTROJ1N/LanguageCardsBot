using Keycloak.AuthServices.Authentication;
using Keycloak.AuthServices.Authorization;
using Keycloak.AuthServices.Sdk;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi;
using Passport.Application.Abstractions.Clients;
using Passport.Application.Abstractions.Services;
using Passport.Application.Services;
using Passport.Infrastructure.Authentication;
using Passport.Infrastructure.Repositories;

namespace Passport.Presentation;

public static class ServiceConfiguration
{
    public static IServiceCollection AddPassportApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }

    public static IServiceCollection AddPassportInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<KeycloakAdminAuthHandler>();
        services.AddKeycloakAdminHttpClient(configuration)
            .AddHttpMessageHandler<KeycloakAdminAuthHandler>();
        services.AddHttpClient("keycloak-token");
        services.AddScoped<IKeycloakTokenClient, KeycloakTokenClient>();
        return services;
    }

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
    
    public static IServiceCollection AddAuthorization(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddKeycloakAuthorization();
        services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminAndUser", builder =>
                {
                    builder
                        .RequireRealmRoles("User") // Realm role is fetched from token
                        .RequireResourceRoles("Admin"); // Resource/Client role is fetched from token
                });
            })
            .AddKeycloakAuthorization(configuration);

        return services;
    }

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
