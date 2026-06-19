using Keycloak.AuthServices.Authentication;
using Keycloak.AuthServices.Authorization;
using Keycloak.AuthServices.Common;
using Keycloak.AuthServices.Sdk;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Passport.Infrastructure.Authentication;
using Passport.Presentation;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddPassportInfrastructure(builder.Configuration)
    .AddPassportApplicationServices()
    .AddPassportAuthentication(builder.Configuration)
    .AddAuthorization(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddPassportSwaggerDocumentation();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
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
