using EnglishCardsBot.Application.Interfaces;
using EnglishCardsBot.Application.Services;
using EnglishCardsBot.Infrastructure.Data;
using EnglishCardsBot.Infrastructure.Repositories;
using EnglishCardsBot.Infrastructure.Services;
using EnglishCardsBot.Presentation;
using EnglishCardsBot.Presentation.Services;
using EnglishCardsBot.Presentation.Workers;
using Telegram.Bot;

DotNetEnv.Env.Load();

var builder = Host.CreateApplicationBuilder(args);

// Configuration
var botToken = builder.Configuration["Bot:Token"] 
    ?? builder.Configuration["Token"] 
    ?? Environment.GetEnvironmentVariable("BOT_TOKEN") 
    ?? throw new InvalidOperationException("BOT_TOKEN is not set. Please set it in appsettings.json (Bot:Token) or environment variable BOT_TOKEN");

if (string.IsNullOrWhiteSpace(botToken))
{
    throw new InvalidOperationException("BOT_TOKEN cannot be empty. Please set a valid bot token in appsettings.json (Bot:Token) or environment variable BOT_TOKEN");
}
var dbPath = builder.Configuration["Database:ConnectionString"] ?? Environment.GetEnvironmentVariable("DB_PATH") ?? "data/bot.db";

// Normalize connection string: if it's just a path, add "Data Source=" prefix
var connectionString = dbPath.StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase) 
    ? dbPath 
    : $"Data Source={dbPath}";

// Database
builder.Services.AddSingleton(new ApplicationDbContext(connectionString));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICardRepository, CardRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();

// Services
builder.Services.AddScoped<CardService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<StatsService>();
builder.Services.AddScoped<ITranslationService, GoogleTranslationService>();

// Telegram Bot
builder.Services.AddHttpClient("telegram_bot_client")
    .AddTypedClient<ITelegramBotClient>((httpClient, sp) => 
        new TelegramBotClient(botToken, httpClient));

builder.Services.AddScoped<TelegramBotService>();

// Workers
builder.Services.AddHostedService<Worker>();
builder.Services.AddHostedService<ReminderWorker>();

var host = builder.Build();

// Initialize database
var dbContext = host.Services.GetRequiredService<ApplicationDbContext>();
await dbContext.InitializeAsync();

await host.RunAsync();
