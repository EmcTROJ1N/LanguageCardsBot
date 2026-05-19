using Cards.Application.Abstractions.Repositories;
using Cards.Application.Cards;
using Cards.Application.Imports;
using Cards.Application.Stats;
using Cards.Application.Translations;
using Cards.Application.Users;
using Cards.Infrastructure.Data;
using Cards.Infrastructure.Repositories;
using Cards.Presentation.Interceptors;
using Cards.Presentation.Services;
using Microsoft.EntityFrameworkCore;

namespace Cards.Presentation;

/// <summary>
/// Registers cards service dependencies and transport endpoints.
/// </summary>
public static class ServiceConfiguration
{
    /// <summary>
    /// Registers the MySQL EF Core database context.
    /// </summary>
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
    
    
    /// <summary>
    /// Registers gRPC services and interceptors.
    /// </summary>
    public static IServiceCollection AddGrpcServices(this IServiceCollection services)
    {
        services.AddGrpc(options => options.Interceptors.Add<GrpcExceptionInterceptor>());
        return services;
    }

    /// <summary>
    /// Registers infrastructure repository implementations.
    /// </summary>
    public static IServiceCollection AddCardsInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICardRepository, CardRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        return services;
    }

    /// <summary>
    /// Registers application services shared by REST and gRPC transports.
    /// </summary>
    public static IServiceCollection AddCardsApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<ICardApplicationService, CardApplicationService>();
        services.AddScoped<IUserApplicationService, UserApplicationService>();
        services.AddScoped<IStatsApplicationService, StatsApplicationService>();
        services.AddScoped<ICardsImportApplicationService, CardsImportApplicationService>();
        services.AddSingleton(CreateTranslationOptions(configuration));
        services.AddHttpClient<ITranslationApplicationService, GoogleTranslationApplicationService>();

        return services;
    }

    /// <summary>
    /// Maps all cards gRPC services.
    /// </summary>
    public static WebApplication MapGrpcServices(this WebApplication app)
    {
        app.MapGrpcService<CardsImportGrpcService>();
        app.MapGrpcService<CardGrpcService>();
        app.MapGrpcService<StatsGrpcService>();
        app.MapGrpcService<UserGrpcService>();
        app.MapGrpcService<GoogleTranslationService>();
        return app;
    }
    
    /// <summary>
    /// Resolves the cards database connection string from configuration or environment variables.
    /// </summary>
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

    /// <summary>
    /// Creates translation options from configuration or environment variables.
    /// </summary>
    private static TranslationOptions CreateTranslationOptions(IConfiguration configuration)
    {
        var targetLanguage = configuration["Translation:TargetLanguage"]
                             ?? Environment.GetEnvironmentVariable("TRANSLATION_TARGET_LANGUAGE")
                             ?? "ru";
        var sourceLanguage = configuration["Translation:SourceLanguage"]
                             ?? Environment.GetEnvironmentVariable("TRANSLATION_SOURCE_LANGUAGE")
                             ?? "auto";

        return new TranslationOptions(sourceLanguage, targetLanguage);
    }
}
