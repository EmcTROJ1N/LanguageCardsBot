using Cards.Presentation;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddDbContext(builder.Configuration)
    .AddCardsInfrastructure()
    .AddCardsApplicationServices(builder.Configuration)
    .AddGrpcServices();

builder.Services.AddControllers();

var app = builder.Build();

app.MapGrpcServices();
app.MapControllers();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");

app.Run();
