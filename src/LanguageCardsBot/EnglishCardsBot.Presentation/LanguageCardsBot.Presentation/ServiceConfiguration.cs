using EnglishCardsBot.Presentation.Commands.Clear;
using EnglishCardsBot.Presentation.Commands.Export;
using EnglishCardsBot.Presentation.Commands.Import;
using EnglishCardsBot.Presentation.Commands.List;
using EnglishCardsBot.Presentation.Commands.ReminderSettings;
using EnglishCardsBot.Presentation.Commands.Start;
using EnglishCardsBot.Presentation.Commands.Stats;
using EnglishCardsBot.Presentation.Commands.Train;
using EnglishCardsBot.Presentation.Commands.UserId;
using EnglishCardsBot.Presentation.Consumers;
using EnglishCardsBot.Presentation.Services;
using LanguageCardsBot.Contracts.Cards.V3;
using LanguageCardsBot.Contracts.Messaging.Events;
using LanguageCardsBot.Contracts.Messaging.Settings;
using MassTransit;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace EnglishCardsBot.Presentation;

/// <summary>
/// Registers LanguageCardsBot worker service dependencies.
/// </summary>
public static class ServiceConfiguration
{
    /// <summary>
    /// Registers the Telegram bot client using the token resolved from environment or configuration.
    /// Also registers <see cref="TelegramBotService"/>.
    /// </summary>
    public static IServiceCollection AddBotConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var botToken = ResolveBotToken(configuration);

        services.AddHttpClient("telegram_bot_client")
            .AddTypedClient<ITelegramBotClient>((httpClient, _) =>
                new TelegramBotClient(botToken, httpClient));

        services.AddScoped<TelegramBotService>();

        return services;
    }

    /// <summary>
    /// Registers all Telegram command handlers.
    /// </summary>
    public static IServiceCollection AddCommandHandlers(this IServiceCollection services)
    {
        services.AddScoped<StartCommandHandler>();
        services.AddScoped<TrainCommandHandle>();
        services.AddScoped<StatsCommandHandler>();
        services.AddScoped<ListCommandHandler>();
        services.AddScoped<ReminderSettingsCommandHandler>();
        services.AddScoped<ClearCommandHandler>();
        services.AddScoped<ExportCommandHandler>();
        services.AddScoped<ImportCommandHandler>();
        services.AddScoped<UserIdCommandHandler>();

        return services;
    }

    /// <summary>
    /// Registers typed gRPC clients pointing at the Cards microservice.
    /// </summary>
    public static IServiceCollection AddGrpcClients(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var grpcAddress = configuration["Grpc:CardsServiceUrl"]
            ?? throw new InvalidOperationException("Grpc:CardsServiceUrl is not set.");

        var uri = new Uri(grpcAddress);

        services.AddGrpcClient<UserService.UserServiceClient>(o => o.Address = uri);
        services.AddGrpcClient<CardService.CardServiceClient>(o => o.Address = uri);
        services.AddGrpcClient<StatsService.StatsServiceClient>(o => o.Address = uri);
        services.AddGrpcClient<CardsImportService.CardsImportServiceClient>(o => o.Address = uri);

        return services;
    }

    /// <summary>
    /// Registers background workers: Telegram polling and reminder scheduler.
    /// </summary>
    public static IServiceCollection AddWorkers(this IServiceCollection services)
    {
        services.AddHostedService<Worker>();
        return services;
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
    
    public static IServiceCollection AddMassTransitWithRabbitMq(this IServiceCollection services)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<CardReminderConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var options = context.GetRequiredService<IOptions<RabbitMqOptions>>().Value;

                cfg.Host(options.HostName, h =>
                {
                    h.Username(options.UserName);
                    h.Password(options.Password);
                });

                cfg.Message<CardReminderEvent>(m =>
                    m.SetEntityName(options.ExchangeName));

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }

    private static string ResolveBotToken(IConfiguration configuration)
    {
        var token = Environment.GetEnvironmentVariable("BOT_TOKEN");

        if (string.IsNullOrWhiteSpace(token))
            token = configuration["Bot:Token"];
        if (string.IsNullOrWhiteSpace(token))
            token = configuration["Token"];
        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("BOT_TOKEN is not set.");

        return token;
    }
}
