using System.Reflection;
using LanguageCardsBot.Observability.Extensions;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Prometheus;

DotNetEnv.Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenTelemetryTracing(builder.Configuration, "ApiGateway");
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddHealthChecks();

var app = builder.Build();
app.UseHttpMetrics();
app.MapReverseProxy();
app.MapMetrics();
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
