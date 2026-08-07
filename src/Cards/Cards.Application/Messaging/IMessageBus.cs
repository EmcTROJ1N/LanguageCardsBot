namespace Cards.Application.Messaging;

public interface IMessageBus
{
    Task PublishAsync<T>(T message, TimeSpan? delay = null, string? routingKey = null, CancellationToken ct = default) where T : class;
}
