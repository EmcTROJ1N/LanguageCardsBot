namespace Cards.Application.Messaging;

public interface IMessageBus
{
    Task PublishAsync<T>(T message, string routingKey, CancellationToken ct = default) where T : class;
}