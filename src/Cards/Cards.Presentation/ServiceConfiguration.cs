using System.Reflection;
using Cards.Application.Abstractions.Metrics;
using Cards.Application.Abstractions.Repositories;
using Cards.Application.Cards;
using Cards.Application.Imports;
using Cards.Application.Messaging;
using Cards.Application.Reminders;
using Cards.Application.Stats;
using Cards.Application.Translations;
using Cards.Application.Users;
using Cards.Infrastructure.Data;
using Cards.Infrastructure.Messaging;
using Cards.Infrastructure.Metrics;
using Cards.Infrastructure.Repositories;
using Cards.Infrastructure.Services;
using LanguageCardsBot.Contracts.Messaging.Settings;
using Cards.Presentation.Interceptors;
using Cards.Presentation.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using RabbitMQ.Client;

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
    
    public static IServiceCollection AddMessaging(this IServiceCollection services)
    {
        return services.AddTransient<IMessageBus, RabbitMessageBus>();
    }

    public static IServiceCollection AddSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<RabbitMqOptions>()
            .Bind(configuration.GetSection(RabbitMqOptions.Section))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
    
    public static IServiceCollection AddRabbitMqPublisher(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(
            configuration.GetSection(RabbitMqOptions.Section)
        );
        services.AddSingleton<RabbitMqConnectionFactory>();
        services.AddHostedService<RabbitMqInitializerService>();

        // Transient — новый publisher на каждый запрос
        //services.AddTransient<IEventPublisher, RabbitMqEventPublisher>();
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
    /// Registers infrastructure repository implementations and background services.
    /// </summary>
    public static IServiceCollection AddCardsInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICardRepository, CardRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddSingleton<ICardReminderOrchestrator, CardReminderOrchestrator>();
        services.AddHostedService<CardReminderStartupService>();
        services.AddHostedService<DailySummaryBackgroundService>();
        services.AddTransient<IMessageBus, RabbitMessageBus>();
        services.AddSingleton<ICardMetrics, CardMetrics>();
        services.AddHostedService<CardsGaugeMetricsService>();
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
    /// Registers Swagger/OpenAPI generation for the public REST API.
    /// </summary>
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "LanguageCardsBot Cards API",
                Version = "v3",
                Description = "REST API for cards, users, statistics, imports, and translations."
            });

            var xmlFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlFilePath = Path.Combine(AppContext.BaseDirectory, xmlFileName);
            if (File.Exists(xmlFilePath))
                options.IncludeXmlComments(xmlFilePath, includeControllerXmlComments: true);
        });

        return services;
    }

    public static IServiceCollection AddOpenTelemetryPrometheus(this IServiceCollection services, string environmentName)
    {
        var serviceName = "cards";
        // TODO: make a better decision
        var serviceVersion = "1.0.0";


        services.AddOpenTelemetry()
            .ConfigureResource(r => r
                .AddService(serviceName: serviceName, serviceVersion: serviceVersion)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = environmentName
                }))
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation() // покрывает и gRPC-сервер
                    .AddRuntimeInstrumentation()
                    .AddMeter("LanguageCardsBot.Cards")
                    .AddPrometheusExporter();
            });

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
    /// Enables Swagger JSON and Swagger UI endpoints.
    /// </summary>
    public static WebApplication UseSwaggerDocumentation(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.RoutePrefix = "swagger";
            options.SwaggerEndpoint("./v1/swagger.json", "LanguageCardsBot Cards API");
        });

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
