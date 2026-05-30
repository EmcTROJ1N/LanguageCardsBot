using Passport.Presentation;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddPassportInfrastructure()
    .AddPassportApplicationServices()
    .AddPassportAuthentication();

builder.Services.AddControllers();
builder.Services.AddPassportSwaggerDocumentation();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.RoutePrefix = "swagger";
        options.SwaggerEndpoint("./v1/swagger.json", "LanguageCardsBot Passport API");
    });
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
