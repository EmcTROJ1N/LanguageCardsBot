using Passport.Application;
using Passport.Infrastructure;
using Passport.Presentation;
using Scalar.AspNetCore;

DotNetEnv.Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddPassportInfrastructure(builder.Configuration)
    .AddPassportApplicationServices()
    .AddPassportAuthentication(builder.Configuration)
    .AddPassportAuthorization(builder.Configuration)
    .AddPassportOpenApiDocumentation()
    .AddControllers();

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

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
