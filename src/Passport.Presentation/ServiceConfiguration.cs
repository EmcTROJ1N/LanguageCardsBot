using System.Reflection;
using Microsoft.AspNetCore.Authentication;
using Passport.Application.Abstractions.Repositories;
using Passport.Application.Abstractions.Services;
using Passport.Application.Services;
using Passport.Infrastructure.Repositories;
using Passport.Presentation.Authentication;

namespace Passport.Presentation;

/// <summary>
/// Registers passport service dependencies and HTTP pipeline components.
/// </summary>
public static class ServiceConfiguration
{
    /// <summary>
    /// Registers passport application services.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddPassportApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }

    /// <summary>
    /// Registers passport infrastructure dependencies.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddPassportInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IKeycloakRepository, KeycloakRepository>();
        return services;
    }

    /// <summary>
    /// Registers local stub bearer authentication.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddPassportAuthentication(this IServiceCollection services)
    {
        services
            .AddAuthentication(StubBearerAuthenticationHandler.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, StubBearerAuthenticationHandler>(
                StubBearerAuthenticationHandler.SchemeName,
                _ => { });

        services.AddAuthorization();

        return services;
    }

    /// <summary>
    /// Registers Swagger/OpenAPI generation for the passport API.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddPassportSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        
        // TODO: was broken after migration to .net 10, fix later
        /*services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "LanguageCardsBot Passport API",
                Version = "v1",
                Description = "REST API for local passport authentication flows."
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "stub",
                In = ParameterLocation.Header,
                Description = "Use the access token returned by the login endpoint."
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            var xmlFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlFilePath = Path.Combine(AppContext.BaseDirectory, xmlFileName);
            if (File.Exists(xmlFilePath))
                options.IncludeXmlComments(xmlFilePath, includeControllerXmlComments: true);
        });*/

        return services;
    }
}
