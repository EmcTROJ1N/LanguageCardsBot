namespace Cards.Application.Messaging;

/// <summary>
/// Consumer contract for messages of type <typeparamref name="T"/> delivered from the message bus.
/// </summary>
/// <typeparam name="T">Concrete message contract type.</typeparam>
public interface IMessageConsumer<in T> where T : class
{
    /// <summary>Handles a single message of type <typeparamref name="T"/>.</summary>
    Task HandleAsync(T message, CancellationToken ct = default);
}
