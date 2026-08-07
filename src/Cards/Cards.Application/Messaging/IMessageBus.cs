namespace Cards.Application.Messaging;

/// <summary>
/// Abstraction over an asynchronous message bus for publishing domain and integration events.
/// </summary>
public interface IMessageBus
{
    /// <summary>
    /// Publishes a message with an optional delivery delay and routing key override.
    /// </summary>
    Task PublishAsync<T>(T message, TimeSpan? delay = null, string? routingKey = null, CancellationToken ct = default) where T : class;
}
