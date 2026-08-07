using LanguageCardsBot.Contracts.Cards.V3;

namespace LanguageCardsBot.Presentation.Abstractions;

/// <summary>
/// Contract for classes that handle a specific bot command represented by <typeparamref name="TCommand"/>.
/// One handler per command type — dispatched by <see cref="Dispatchers.ICommandDispatcher"/>.
/// </summary>
/// <typeparam name="TCommand">The concrete command message type this handler processes.</typeparam>
public interface ICommandHandler<in TCommand>
{
    /// <summary>
    /// Processes the command in the context of the resolved bot user.
    /// </summary>
    /// <param name="command">Parsed command payload (chat id, arguments, etc.).</param>
    /// <param name="user">The Cards-service user associated with the Telegram chat.</param>
    /// <param name="cancellationToken">Cancellation token propagated from the polling loop.</param>
    Task HandleAsync(TCommand command, User user, CancellationToken cancellationToken = default);
}
