using LanguageCardsBot.Observability.Extensions;
using LanguageCardsBot.Presentation;

DotNetEnv.Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddBotConfiguration(builder.Configuration)
    .AddCommandHandlers()
    .AddGrpcClients(builder.Configuration)
    .AddSettings(builder.Configuration)
    .AddMassTransitWithRabbitMq()
    .AddOpenTelemetryTracing(builder.Configuration, "LanguageCardsBot")
    .AddOpenTelemetryPrometheus(builder.Environment.EnvironmentName)
    .AddWorkers();

var app = builder.Build();

app.MapPrometheusScrapingEndpoint();

await app.RunAsync();
