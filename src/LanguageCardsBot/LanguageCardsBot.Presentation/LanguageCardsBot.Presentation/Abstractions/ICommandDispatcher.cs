using LanguageCardsBot.Contracts.Cards.V3;

namespace LanguageCardsBot.Presentation.Abstractions;

/// <summary>Routes a trigger string (slash command or menu button text) to the registered command handler.</summary>
public interface ICommandDispatcher
{
    /// <summary>
    /// Dispatches the trigger to a registered handler. Returns false if no handler is registered for the trigger.
    /// </summary>
    Task<bool> TryDispatchAsync(string trigger, long chatId, string[] args, User user, CancellationToken ct);
}
