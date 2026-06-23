using Cards.Presentation;
using Cards.Presentation.Mapping;

var builder = WebApplication.CreateBuilder(args);

CardsMappingConfiguration.Register();

builder.Services
    .AddSettings(builder.Configuration)
    .AddRabbitMqPublisher(builder.Configuration)
    .AddDbContext(builder.Configuration)
    .AddCardsInfrastructure()
    .AddCardsApplicationServices(builder.Configuration)
    .AddGrpcServices();

builder.Services.AddControllers();
builder.Services.AddSwaggerDocumentation();

var app = builder.Build();

app.UseSwaggerDocumentation();
app.MapGrpcServices();
app.MapControllers();

app.Run();
