using Cards.Application.Messaging;

namespace Cards.Infrastructure.Services;

/// <summary>
/// MassTransit-backed <see cref="IMessageBus"/> implementation. Not yet wired up.
/// </summary>
public class MassTransitMessageBus: IMessageBus
{
    /// <inheritdoc />
    public Task PublishAsync<T>(T message, TimeSpan? delay = null, string? routingKey = null, CancellationToken ct = default) where T : class
    {
        throw new NotImplementedException();
    }
}
