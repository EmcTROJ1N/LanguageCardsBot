using Cards.Application.Messaging;

namespace Cards.Infrastructure.Services;

public class MassTransitMessageBus: IMessageBus
{
    public Task PublishAsync<T>(T message, TimeSpan? delay = null, string? routingKey = null, CancellationToken ct = default) where T : class
    {
        throw new NotImplementedException();
    }
}