using EnglishCardsBot.Application.Interfaces;
using EnglishCardsBot.Application.Services;
using EnglishCardsBot.Infrastructure.Data;
using EnglishCardsBot.Infrastructure.Repositories;
using EnglishCardsBot.Infrastructure.Services;
using EnglishCardsBot.Presentation;
using EnglishCardsBot.Presentation.Commands.Clear;
using EnglishCardsBot.Presentation.Commands.Export;
using EnglishCardsBot.Presentation.Commands.Import;
using EnglishCardsBot.Presentation.Commands.List;
using EnglishCardsBot.Presentation.Commands.ReminderSettings;
using EnglishCardsBot.Presentation.Commands.Start;
using EnglishCardsBot.Presentation.Commands.Stats;
using EnglishCardsBot.Presentation.Commands.Train;
using EnglishCardsBot.Presentation.Services;
using EnglishCardsBot.Presentation.Workers;
using Telegram.Bot;

DotNetEnv.Env.TraversePath().Load();

var builder = Host.CreateApplicationBuilder(args);

// Configuration
var botToken = Environment.GetEnvironmentVariable("BOT_TOKEN") 
    ?? builder.Configuration["Bot:Token"] 
    ?? builder.Configuration["Token"] 
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
builder.Services.AddScoped<ICardsImportService, CardsImportService>();


// Handlers
builder.Services.AddScoped<StartCommandHandler>();
builder.Services.AddScoped<TrainCommandHandle>();
builder.Services.AddScoped<StatsCommandHandler>();
builder.Services.AddScoped<ListCommandHandler>();
builder.Services.AddScoped<ReminderSettingsCommandHandler>();
builder.Services.AddScoped<ClearCommandHandler>();
builder.Services.AddScoped<ExportCommandHandler>();
builder.Services.AddScoped<ImportCommandHandler>();

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
