using System.Reflection;
using LanguageCardsBot.Observability.Extensions;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Passport.Application;
using Passport.Infrastructure;
using Passport.Presentation;
using Passport.Presentation.Middleware;
using Scalar.AspNetCore;

DotNetEnv.Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddPassportInfrastructure(builder.Configuration)
    .AddPassportApplicationServices()
    .AddPassportAuthentication(builder.Configuration)
    .AddPassportAuthorization(builder.Configuration)
    .AddOpenTelemetryTracing(builder.Configuration, "Passport")
    .AddPassportOpenApiDocumentation()
    .AddControllers();

builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi(options => options.Path = "/api/passport/openapi/v1.json");
    app.MapScalarApiReference("/api/passport/scalar", options =>
    {
        options.Title = "LanguageCardsBot Passport API";
        options.AddPreferredSecuritySchemes("Bearer");
        options.OpenApiRoutePattern = "/api/passport/openapi/v1.json";
    });
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
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
