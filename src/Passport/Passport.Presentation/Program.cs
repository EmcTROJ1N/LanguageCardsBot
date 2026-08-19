using Passport.Application;
using Passport.Infrastructure;
using Passport.Presentation;
using Scalar.AspNetCore;

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
    app.UseOpenApi();       // отдаёт /swagger/v1/swagger.json
    app.MapScalarApiReference(options =>
    {
        options.Title = "LanguageCardsBot Passport API";
        options.AddPreferredSecuritySchemes("Bearer");
    });
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
