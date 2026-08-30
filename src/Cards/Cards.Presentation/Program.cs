using Cards.Presentation;
using Cards.Presentation.Mapping;
using LanguageCardsBot.Observability.Extensions;

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

var app = builder.Build();

await app.MigrateDatabase();

app.UseAuthentication();
app.UseAuthorization();
app.UseSwaggerDocumentation();
app.MapGrpcServices();
app.MapControllers();
app.MapPrometheusScrapingEndpoint();

app.Run();
