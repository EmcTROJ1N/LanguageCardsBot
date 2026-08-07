namespace Cards.Application.Messaging;

public interface IMessageConsumer<in T> where T : class
{
    Task HandleAsync(T message, CancellationToken ct = default);
}
