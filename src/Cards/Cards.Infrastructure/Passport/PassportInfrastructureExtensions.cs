using Cards.Application.Abstractions.Services;
using Cards.Infrastructure.Passport.Generated;
using Microsoft.Extensions.DependencyInjection;

namespace Cards.Infrastructure.Passport;

/// <summary>
/// DI registration helpers for Passport client infrastructure.
/// </summary>
public static class PassportInfrastructureExtensions
{
    /// <summary>
    /// Registers the Passport HTTP client, Bearer token forwarding handler,
    /// and the <see cref="IPassportService"/> implementation.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="baseUrl">The base URL of the Passport HTTP API.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/> for the Passport HTTP client.</returns>
    public static IHttpClientBuilder AddPassportHttpClient(
        this IServiceCollection services,
        string baseUrl)
    {
        services.AddHttpContextAccessor();
        services.AddTransient<PassportAuthDelegatingHandler>();
        services.AddScoped<IPassportService, PassportService>();
        return services
            .AddHttpClient<IPassportClient, PassportClient>(client =>
                client.BaseAddress = new Uri(baseUrl))
            .AddHttpMessageHandler<PassportAuthDelegatingHandler>();
    }
}
