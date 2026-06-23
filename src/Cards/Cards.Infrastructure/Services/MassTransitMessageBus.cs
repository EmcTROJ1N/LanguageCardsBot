using Cards.Application.Messaging;

namespace Cards.Infrastructure.Services;

public class MassTransitMessageBus: IMessageBus
{
    public Task PublishAsync<T>(T message, string routingKey, CancellationToken ct = default) where T : class
    {
        throw new NotImplementedException();
    }
}