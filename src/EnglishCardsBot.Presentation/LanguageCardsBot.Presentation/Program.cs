using EnglishCardsBot.Presentation.Commands.Clear;
using EnglishCardsBot.Presentation.Commands.Export;
using EnglishCardsBot.Presentation.Commands.Import;
using EnglishCardsBot.Presentation.Commands.List;
using EnglishCardsBot.Presentation.Commands.ReminderSettings;
using EnglishCardsBot.Presentation.Commands.Start;
using EnglishCardsBot.Presentation.Commands.Stats;
using EnglishCardsBot.Presentation.Commands.Train;
using Telegram.Bot;

DotNetEnv.Env.TraversePath().Load();

var builder = Host.CreateApplicationBuilder(args);

// Configuration
var botToken = Environment.GetEnvironmentVariable("BOT_TOKEN") 
    ?? builder.Configuration["Bot:Token"] 
    ?? builder.Configuration["Token"] 
    ?? throw new InvalidOperationException("BOT_TOKEN is not set. Please set it in appsettings.json (Bot:Token) or environment variable BOT_TOKEN");

if (string.IsNullOrWhiteSpace(botToken))
    throw new InvalidOperationException("BOT_TOKEN cannot be empty. Please set a valid bot token in appsettings.json (Bot:Token) or environment variable BOT_TOKEN");


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

//builder.Services.AddScoped<TelegramBotService>();

// Workers
//builder.Services.AddHostedService<Worker>();
//builder.Services.AddHostedService<ReminderWorker>();

var host = builder.Build();

await host.RunAsync();
