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
using LanguageCardsBot.Contracts.Cards.V3;
using Telegram.Bot;

DotNetEnv.Env.TraversePath().Load();

var builder = Host.CreateApplicationBuilder(args);

// Configuration
var botToken = Environment.GetEnvironmentVariable("BOT_TOKEN");

if (string.IsNullOrWhiteSpace(botToken))
    botToken = builder.Configuration["Bot:Token"];
if (string.IsNullOrWhiteSpace(botToken))
    botToken = builder.Configuration["Token"];
if (string.IsNullOrWhiteSpace(botToken))
    throw new InvalidOperationException("BOT_TOKEN is not set.");

var grpcAddress = builder.Configuration["Grpc:CardsServiceUrl"];

if (string.IsNullOrWhiteSpace(grpcAddress))
    throw new InvalidOperationException("Grpc:CardsServiceUrl is not set.");

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

builder.Services.AddGrpcClient<UserService.UserServiceClient>(options =>
{
    options.Address = new Uri(grpcAddress);
});

builder.Services.AddGrpcClient<CardService.CardServiceClient>(options =>
{
    options.Address = new Uri(grpcAddress);
});

builder.Services.AddGrpcClient<StatsService.StatsServiceClient>(options =>
{
    options.Address = new Uri(grpcAddress);
});

builder.Services.AddGrpcClient<CardsImportService.CardsImportServiceClient>(options =>
{
    options.Address = new Uri(grpcAddress);
});

builder.Services.AddScoped<TelegramBotService>();

// Workers
builder.Services.AddHostedService<Worker>();
builder.Services.AddHostedService<ReminderWorker>();

var host = builder.Build();

await host.RunAsync();
