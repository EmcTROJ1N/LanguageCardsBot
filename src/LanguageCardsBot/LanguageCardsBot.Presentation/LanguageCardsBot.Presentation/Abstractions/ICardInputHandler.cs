using LanguageCardsBot.Contracts.Cards.V3;
using Telegram.Bot.Types;

namespace LanguageCardsBot.Presentation.Abstractions;

/// <summary>Handles free-text messages that are not slash commands or menu buttons (card addition flow).</summary>
public interface ICardInputHandler
{
    Task HandleAsync(Message message, string text, global::LanguageCardsBot.Contracts.Cards.V3.User user, CancellationToken ct);
}
