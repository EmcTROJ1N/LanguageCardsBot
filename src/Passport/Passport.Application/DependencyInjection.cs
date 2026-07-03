using Microsoft.Extensions.DependencyInjection;
using Passport.Application.Abstractions.Services;
using Passport.Application.Services;

namespace Passport.Application;

/// <summary>
/// Extension methods for registering Application-layer services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers Application-layer services into the DI container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
    public static IServiceCollection AddPassportApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }
}
