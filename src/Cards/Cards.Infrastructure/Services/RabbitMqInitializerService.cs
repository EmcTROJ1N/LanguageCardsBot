using Cards.Infrastructure.Messaging;
using LanguageCardsBot.Contracts.Messaging.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Cards.Infrastructure.Services;

/// <summary>
/// Hosted service that opens the shared RabbitMQ connection on application start using
/// <see cref="RabbitMqOptions"/>.
/// </summary>
public class RabbitMqInitializerService(
    RabbitMqConnectionFactory factory,
    IOptions<RabbitMqOptions> options)
    : IHostedService
{
    /// <inheritdoc />
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

    /// <inheritdoc />
    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
