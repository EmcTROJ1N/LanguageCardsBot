using LanguageCardsBot.Presentation.Abstractions;
using Telegram.Bot.Types;

namespace LanguageCardsBot.Presentation.Dispatchers;

/// <summary>Routes a callback query to the first <see cref="ICallbackHandler"/> whose <c>CanHandle</c> returns true.</summary>
public class CallbackDispatcher(IEnumerable<ICallbackHandler> handlers) : ICallbackDispatcher
{
    /// <inheritdoc/>
    public async Task<bool> TryDispatchAsync(CallbackQuery callbackQuery, global::LanguageCardsBot.Contracts.Cards.V3.User user, CancellationToken ct)
    {
        var data = callbackQuery.Data ?? string.Empty;
        var handler = handlers.FirstOrDefault(h => h.CanHandle(data));
        if (handler is null) return false;
        await handler.HandleAsync(callbackQuery, user, ct);
        return true;
    }
}
