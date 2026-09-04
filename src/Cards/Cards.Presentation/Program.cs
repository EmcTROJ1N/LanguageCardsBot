using System.Reflection;
using Cards.Presentation;
using Cards.Presentation.Mapping;
using LanguageCardsBot.Observability.Extensions;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

DotNetEnv.Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

CardsMappingConfiguration.Register();

builder.Services
    .AddSettings(builder.Configuration)
    .AddRabbitMqPublisher(builder.Configuration)
    .AddDbContext(builder.Configuration)
    .AddCardsInfrastructure()
    .AddCardsApplicationServices(builder.Configuration)
    .AddPassportClient(builder.Configuration)
    .AddCardsAuthentication(builder.Configuration)
    .AddGrpcServices()
    .AddOpenTelemetryTracing(builder.Configuration, "Cards")
    .AddOpenTelemetryPrometheus(builder.Environment.EnvironmentName)
    .AddSwaggerDocumentation()
    .AddControllers();

builder.Services.AddHealthChecks();

var app = builder.Build();

await app.MigrateDatabase();

app.UseAuthentication();
app.UseAuthorization();
app.UseSwaggerDocumentation();
app.MapGrpcServices();
app.MapControllers();
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

app.Run();
