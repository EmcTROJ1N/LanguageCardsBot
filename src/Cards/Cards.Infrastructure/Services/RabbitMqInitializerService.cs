using Cards.Infrastructure.Messaging;
using LanguageCardsBot.Contracts.Messaging.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;


namespace Cards.Infrastructure.Services;

public class RabbitMqInitializerService(
    RabbitMqConnectionFactory factory,
    IOptions<RabbitMqOptions> options)
    : IHostedService
{
    public async Task StartAsync(CancellationToken ct)
    {
        var factory1 = new ConnectionFactory
        {
            HostName = options.Value.HostName,
            UserName = options.Value.UserName,
            Password = options.Value.Password
        };

        await factory.InitializeAsync(factory1, ct);
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
