using Cards.Presentation;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddDbContext(builder.Configuration)
    .AddGrpcServices();

var app = builder.Build();

app.MapGrpcServices();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");

app.Run();
