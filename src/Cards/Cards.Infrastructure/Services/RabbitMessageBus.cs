using System.Diagnostics;
using System.Text.Json;
using Cards.Application.Abstractions.Metrics;
using Cards.Application.Messaging;
using Cards.Infrastructure.Messaging;
using LanguageCardsBot.Contracts.Messaging.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Cards.Infrastructure.Services;

/// <summary>
/// RabbitMQ implementation of <see cref="IMessageBus"/> that publishes messages with
/// publisher confirms enabled. <see cref="PublishAsync{T}"/> throws if the broker does
/// not confirm the message within 5 seconds.
/// </summary>
public class RabbitMessageBus(
    RabbitMqConnectionFactory connectionFactory,
    IOptions<RabbitMqOptions> options,
    ILogger<RabbitMessageBus> logger,
    IMessagingMetrics metrics) : IMessageBus
{
    private readonly HashSet<int> _declaredDelayQueues = new();

    /// <inheritdoc/>
    public async Task PublishAsync<T>(
        T message,
        TimeSpan? delay = null,
        string? routingKey = null,
        CancellationToken ct = default) where T : class
    {
        var effectiveRoutingKey = routingKey ?? typeof(T).Name;
        var stopwatch = Stopwatch.StartNew();
        var outcome = "failed";

        using var confirmCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        confirmCts.CancelAfter(TimeSpan.FromSeconds(5));

        try
        {
            var channelOptions = new CreateChannelOptions(
                publisherConfirmationsEnabled: true,
                publisherConfirmationTrackingEnabled: true);

            await using var channel = await connectionFactory.Connection
                .CreateChannelAsync(channelOptions, confirmCts.Token);

            await channel.ExchangeDeclareAsync(
                exchange: options.Value.ExchangeName,
                type: ExchangeType.Fanout,
                durable: true,
                cancellationToken: confirmCts.Token
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
                var dlxExchange = $"{options.Value.ExchangeName}.delayed";

                await channel.ExchangeDeclareAsync(
                    exchange: dlxExchange,
                    type: ExchangeType.Direct,
                    durable: true,
                    cancellationToken: confirmCts.Token
                );

                var waitingQueue = await EnsureDelayQueueAsync(channel, dlxExchange, ttlMs, confirmCts.Token);

                publishExchange   = string.Empty;
                publishRoutingKey = waitingQueue;
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
                cancellationToken: confirmCts.Token
            );

            logger.LogInformation(
                "Published {EventType}{Delay}",
                typeof(T).Name,
                delay.HasValue ? $" with delay {delay.Value}" : " immediately"
            );

            outcome = "ok";
        }
        catch (OperationCanceledException) when (confirmCts.IsCancellationRequested && !ct.IsCancellationRequested)
        {
            outcome = "timeout";
            throw;
        }
        finally
        {
            stopwatch.Stop();
            metrics.RecordPublish(effectiveRoutingKey, outcome, stopwatch.Elapsed.TotalSeconds);
        }
    }

    /// <summary>
    /// Declares the waiting queue for the given TTL (idempotent) and returns its name.
    /// </summary>
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

        await channel.ExchangeBindAsync(
            destination: options.Value.ExchangeName,
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
