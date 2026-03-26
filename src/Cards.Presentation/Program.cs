using Cards.Infrastructure.Interfaces;
using Cards.Infrastructure.Repositories;
using Cards.Presentation.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration["Database:ConnectionString"]
    ?? Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING")
    ?? Environment.GetEnvironmentVariable("DB_PATH")
    ?? "Data Source=/data/cards.db";

if (!connectionString.Contains('='))
{
    connectionString = $"Data Source={connectionString}";
}

/*
builder.Services.AddSingleton(new ApplicationDbContext(connectionString));
await app.Services.GetRequiredService<ApplicationDbContext>().InitializeAsync();
*/

builder.Services.AddGrpc();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICardRepository, CardRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();

var app = builder.Build();

app.MapGrpcService<CardsImportGrpcService>();
app.MapGrpcService<CardGrpcService>();
app.MapGrpcService<StatsGrpcService>();
app.MapGrpcService<UserGrpcService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");

app.Run();
