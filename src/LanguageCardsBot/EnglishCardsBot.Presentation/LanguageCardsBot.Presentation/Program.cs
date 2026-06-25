using EnglishCardsBot.Presentation;

DotNetEnv.Env.TraversePath().Load();

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddBotConfiguration(builder.Configuration)
    .AddCommandHandlers()
    .AddGrpcClients(builder.Configuration)
    .AddWorkers();
    // TODO: .AddMessagingConsumers(builder.Configuration)

await builder.Build().RunAsync();
