using Cards.Infrastructure.Data;
using Cards.Infrastructure.Interfaces;
using Cards.Infrastructure.Repositories;
using Cards.Presentation.Services;
using Microsoft.EntityFrameworkCore;

namespace Cards.Presentation;

public static class ServiceConfiguration
{
    public static IServiceCollection AddDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = GetConnectionString(configuration);
        
        services.AddDbContext<CardsMysqlDbContext>(options =>
            options.UseMySql(
                connectionString,
                new MySqlServerVersion(new Version(8, 0, 34))
            )
        );    
        return services;
    }
    
    
    public static IServiceCollection AddGrpcServices(this IServiceCollection services)
    {
        services.AddGrpc();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICardRepository, CardRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        return services;
    }

    public static WebApplication MapGrpcServices(this WebApplication app)
    {
        app.MapGrpcService<CardsImportGrpcService>();
        app.MapGrpcService<CardGrpcService>();
        app.MapGrpcService<StatsGrpcService>();
        app.MapGrpcService<UserGrpcService>();
        return app;
    }
    
    private static string GetConnectionString(IConfiguration configuration)
    {
        var values = new[]
        {
            configuration["Database:ConnectionString"],
            Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING"),
            Environment.GetEnvironmentVariable("DB_PATH")
        };

        return values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v))
               ?? throw new InvalidOperationException("Connection string is not configured.");
    }
}