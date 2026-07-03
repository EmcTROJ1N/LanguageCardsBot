using LanguageCardsBot.Contracts.Cards.V3;
using Telegram.Bot.Types;

namespace LanguageCardsBot.Presentation.Abstractions;

/// <summary>Routes a callback query to the appropriate <see cref="ICallbackHandler"/>.</summary>
public interface ICallbackDispatcher
{
    /// <summary>
    /// Dispatches the callback query to the first handler that can handle it.
    /// Returns false if no handler matched.
    /// </summary>
    Task<bool> TryDispatchAsync(CallbackQuery callbackQuery, global::LanguageCardsBot.Contracts.Cards.V3.User user, CancellationToken ct);
}
