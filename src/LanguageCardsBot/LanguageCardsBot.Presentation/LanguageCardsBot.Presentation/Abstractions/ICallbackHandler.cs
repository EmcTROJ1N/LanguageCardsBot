using LanguageCardsBot.Contracts.Cards.V3;
using Telegram.Bot.Types;

namespace LanguageCardsBot.Presentation.Abstractions;

/// <summary>Handles a specific subset of Telegram callback queries identified by <see cref="CanHandle"/>.</summary>
public interface ICallbackHandler
{
    /// <summary>Returns true if this handler should process the given callback data string.</summary>
    bool CanHandle(string callbackData);

    /// <summary>Processes the callback query for an already-resolved user.</summary>
    Task HandleAsync(CallbackQuery callbackQuery, global::LanguageCardsBot.Contracts.Cards.V3.User user, CancellationToken ct);
}
