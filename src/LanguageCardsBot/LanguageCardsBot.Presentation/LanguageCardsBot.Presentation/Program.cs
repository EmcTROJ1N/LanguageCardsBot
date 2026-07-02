using LanguageCardsBot.Presentation;

DotNetEnv.Env.TraversePath().Load();

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddBotConfiguration(builder.Configuration)
    .AddCommandHandlers()
    .AddGrpcClients(builder.Configuration) 
    .AddSettings(builder.Configuration)
    .AddMassTransitWithRabbitMq()
    .AddWorkers();

await builder.Build().RunAsync();
