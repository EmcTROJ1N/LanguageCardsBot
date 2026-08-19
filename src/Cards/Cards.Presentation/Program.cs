using Cards.Presentation;
using Cards.Presentation.Mapping;

DotNetEnv.Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

CardsMappingConfiguration.Register();

builder.Services
    .AddSettings(builder.Configuration)
    .AddRabbitMqPublisher(builder.Configuration)
    .AddDbContext(builder.Configuration)
    .AddCardsInfrastructure()
    .AddCardsApplicationServices(builder.Configuration)
    .AddGrpcServices()
    .AddOpenTelemetryPrometheus(builder.Environment.EnvironmentName)
    .AddSwaggerDocumentation()
    .AddControllers();

var app = builder.Build();

app.UseSwaggerDocumentation();
app.MapGrpcServices();
app.MapControllers();
app.MapPrometheusScrapingEndpoint();

app.Run();
