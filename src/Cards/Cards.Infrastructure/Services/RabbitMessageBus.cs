using System.Text.Json;
using Cards.Application.Messaging;
using Cards.Infrastructure.Messaging;
using LanguageCardsBot.Contracts.Messaging.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Cards.Infrastructure.Services;

public class RabbitMessageBus(RabbitMqConnectionFactory connectionFactory,
    IOptions<RabbitMqOptions> options,
    ILogger<RabbitMessageBus> logger): IMessageBus
{
    public async Task PublishAsync<T>(T message, string? routingKey = null, CancellationToken ct = default) where T : class
    {
        await using var channel = await connectionFactory.Connection
            .CreateChannelAsync(cancellationToken: ct);
     
        routingKey ??= typeof(T).Name;

        await channel.ExchangeDeclareAsync(
            exchange: options.Value.ExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            cancellationToken: ct
        );

        var body = JsonSerializer.SerializeToUtf8Bytes(message);
        var props = new BasicProperties { Persistent = true };

        await channel.BasicPublishAsync(
            exchange: options.Value.ExchangeName,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: props,
            body: body,
            cancellationToken: ct
        );

        logger.LogInformation("Published {EventType}", typeof(T).Name);
    }
}