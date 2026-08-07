using System.Reflection;
using LanguageCardsBot.Contracts.Cards.V3;
using LanguageCardsBot.Contracts.Messaging.Events;
using LanguageCardsBot.Contracts.Messaging.Settings;
using LanguageCardsBot.Presentation.Abstractions;
using LanguageCardsBot.Presentation.Callbacks;
using LanguageCardsBot.Presentation.Commands.Clear;
using LanguageCardsBot.Presentation.Commands.Export;
using LanguageCardsBot.Presentation.Commands.Import;
using LanguageCardsBot.Presentation.Commands.List;
using LanguageCardsBot.Presentation.Commands.ReminderSettings;
using LanguageCardsBot.Presentation.Commands.Start;
using LanguageCardsBot.Presentation.Commands.Stats;
using LanguageCardsBot.Presentation.Commands.Train;
using LanguageCardsBot.Presentation.Commands.UserId;
using LanguageCardsBot.Presentation.Consumers;
using LanguageCardsBot.Presentation.Dispatchers;
using LanguageCardsBot.Presentation.Handlers;
using LanguageCardsBot.Presentation.Services;
using MassTransit;
using Microsoft.Extensions.Options;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Telegram.Bot;

namespace LanguageCardsBot.Presentation;

/// <summary>
/// Registers LanguageCardsBot worker service dependencies.
/// </summary>
public static class ServiceConfiguration
{
    /// <summary>
    /// Registers the Telegram bot client, <see cref="TelegramBotService"/>, dispatchers,
    /// and all command/callback handlers.
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
        services.AddScoped<IDocumentHandler, ImportCommandHandler>();
        services.AddScoped<ICardInputHandler, CardInputHandler>();

        services.AddScoped<ICallbackHandler, TrainingCallbackHandler>();
        services.AddScoped<ICallbackHandler, CardsCallbackHandler>();
        services.AddScoped<ICallbackDispatcher, CallbackDispatcher>();

        return services;
    }

    /// <summary>
    /// Builds the <see cref="CommandRegistry"/> and registers all command handlers with their triggers.
    /// Both slash commands and menu button texts are registered here — one place per command.
    /// </summary>
    public static IServiceCollection AddCommandHandlers(this IServiceCollection services)
    {
        var registry = new CommandRegistry();
        services.AddSingleton(registry);
        services.AddScoped<ICommandDispatcher, CommandDispatcher>();

        services
            .AddCommand<StartCommandHandler, StartCommand>(
                registry, ["/start"],
                (chatId, _) => new StartCommand(chatId))
            .AddCommand<TrainCommandHandle, TrainCommand>(
                registry, ["/train", "🎯 Тренировка"],
                (chatId, _) => new TrainCommand(chatId))
            .AddCommand<StatsCommandHandler, StatCommand>(
                registry, ["/stats", "📊 Статистика"],
                (chatId, _) => new StatCommand(chatId))
            .AddCommand<ListCommandHandler, ListCommand>(
                registry, ["/list", "/cards", "📚 Мои карточки"],
                (chatId, _) => new ListCommand(chatId))
            .AddCommand<ReminderSettingsCommandHandler, ReminderSettingsCommand>(
                registry, ["/reminder_settings", "⚙️ Настройки"],
                (chatId, args) => new ReminderSettingsCommand(chatId, args))
            .AddCommand<ClearCommandHandler, ClearCommand>(
                registry, ["/clear"],
                (chatId, _) => new ClearCommand(chatId))
            .AddCommand<ExportCommandHandler, ExportCommand>(
                registry, ["/export", "📤 Экспорт"],
                (chatId, _) => new ExportCommand(chatId))
            .AddCommand<ImportCommandHandler, ImportCommand>(
                registry, ["/import", "📥 Импорт"],
                (chatId, _) => new ImportCommand(chatId))
            .AddCommand<UserIdCommandHandler, UserIdCommand>(
                registry, ["/user_id", "/id"],
                (chatId, _) => new UserIdCommand(chatId));

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

    public static IServiceCollection AddOpenTelemetryPrometheus(
        this IServiceCollection services,
        string environmentName)
    {
        var serviceName = "language-cards-bot";
        var serviceVersion = Assembly.GetEntryAssembly()
            ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? "unknown";

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
                    .AddAspNetCoreInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddMeter("MassTransit")
                    .AddPrometheusExporter();
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
