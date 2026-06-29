using System.Text.Json;
using Cards.Application.Messaging;
using Cards.Infrastructure.Messaging;
using LanguageCardsBot.Contracts.Messaging.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Cards.Infrastructure.Services;

public class RabbitMessageBus(
    RabbitMqConnectionFactory connectionFactory,
    IOptions<RabbitMqOptions> options,
    ILogger<RabbitMessageBus> logger) : IMessageBus
{
    // Кэш: чтобы не объявлять одни и те же очереди при каждом вызове
    private readonly HashSet<int> _declaredDelayQueues = new();

    public async Task PublishAsync<T>(
        T message,
        TimeSpan? delay = null,
        string? routingKey = null,
        CancellationToken ct = default) where T : class
    {
        await using var channel = await connectionFactory.Connection
            .CreateChannelAsync(cancellationToken: ct);

        routingKey ??= typeof(T).Name;

        // Основной fanout exchange — куда в итоге попадёт сообщение
        await channel.ExchangeDeclareAsync(
            exchange: options.Value.ExchangeName,
            type: ExchangeType.Fanout,
            durable: true,
            cancellationToken: ct
        );

        BasicProperties props;
        string publishExchange;
        string publishRoutingKey;

        if (delay is null or { TotalMilliseconds: <= 0 })
        {
            publishExchange   = options.Value.ExchangeName;
            publishRoutingKey = string.Empty;
            props = BuildProperties();
        }
        else
        {
            var ttlMs = (int)delay.Value.TotalMilliseconds;

            // DLX exchange: direct, чтобы routing key работал
            var dlxExchange = $"{options.Value.ExchangeName}.delayed";

            await channel.ExchangeDeclareAsync(
                exchange: dlxExchange,
                type: ExchangeType.Direct,
                durable: true,
                cancellationToken: ct
            );

            // Waiting queue на конкретный TTL — создаём один раз, потом берём из кэша
            var waitingQueue = await EnsureDelayQueueAsync(channel, dlxExchange, ttlMs, ct);

            // Публикуем в waiting queue через default exchange
            publishExchange   = string.Empty;  // default exchange
            publishRoutingKey = waitingQueue;   // routing key = имя очереди
            props = BuildProperties(ttlMs);
        }

        var envelope = new
        {
            messageId   = Guid.NewGuid(),
            messageType = new[] { $"urn:message:{typeof(T).Namespace}:{typeof(T).Name}" },
            message
        };

        var body = JsonSerializer.SerializeToUtf8Bytes(envelope);

        await channel.BasicPublishAsync(
            exchange:         publishExchange,
            routingKey:       publishRoutingKey,
            mandatory:        false,
            basicProperties:  props,
            body:             body,
            cancellationToken: ct
        );

        logger.LogInformation(
            "Published {EventType}{Delay}",
            typeof(T).Name,
            delay.HasValue ? $" with delay {delay.Value}" : " immediately"
        );
    }

    // Объявляет waiting queue для заданного TTL и возвращает её имя.
    // Называем очередь детерминированно: повторный вызов с тем же ttlMs
    // просто убеждается, что очередь существует (declare idempotent).
    private async Task<string> EnsureDelayQueueAsync(
        IChannel channel,
        string dlxExchange,
        int ttlMs,
        CancellationToken ct)
    {
        if (_declaredDelayQueues.Contains(ttlMs))
            return DelayQueueName(ttlMs);

        var queueName = DelayQueueName(ttlMs);

        await channel.QueueDeclareAsync(
            queue:      queueName,
            durable:    true,
            exclusive:  false,
            autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                ["x-message-ttl"] = ttlMs,
                ["x-dead-letter-exchange"] = dlxExchange,
                ["x-dead-letter-routing-key"] = options.Value.ExchangeName,
            },
            cancellationToken: ct
        );

        // Связываем DLX exchange с основным fanout exchange по routing key
        await channel.ExchangeBindAsync(
            destination: options.Value.ExchangeName,  // fanout — финальный получатель
            source:      dlxExchange,
            routingKey:  options.Value.ExchangeName,
            cancellationToken: ct
        );

        _declaredDelayQueues.Add(ttlMs);
        return queueName;
    }

    private static string DelayQueueName(int ttlMs) =>
        $"delay.{ttlMs}ms";

    private static BasicProperties BuildProperties(int? ttlMs = null)
    {
        var props = new BasicProperties
        {
            Persistent  = true,
            ContentType = "application/vnd.masstransit+json",
        };

        if (ttlMs.HasValue)
            props.Expiration = ttlMs.Value.ToString();

        return props;
    }
}