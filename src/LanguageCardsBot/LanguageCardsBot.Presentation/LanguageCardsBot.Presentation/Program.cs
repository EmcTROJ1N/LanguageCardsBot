using System.Reflection;
using LanguageCardsBot.Observability.Extensions;
using LanguageCardsBot.Presentation;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

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

builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapPrometheusScrapingEndpoint();
app.MapHealthChecks("/healthz", new HealthCheckOptions
{
    ResponseWriter = async (ctx, report) =>
    {
        ctx.Response.ContentType = "application/json";
        var version = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion ?? "unknown";
        await ctx.Response.WriteAsJsonAsync(new { status = report.Status.ToString(), version });
    }
});

await app.RunAsync();
