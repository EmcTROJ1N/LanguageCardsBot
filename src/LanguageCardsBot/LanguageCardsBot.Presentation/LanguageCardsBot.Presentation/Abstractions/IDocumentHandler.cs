using LanguageCardsBot.Contracts.Cards.V3;
using Telegram.Bot.Types;

namespace LanguageCardsBot.Presentation.Abstractions;

/// <summary>Handles an incoming Telegram document (file upload) message.</summary>
public interface IDocumentHandler
{
    Task HandleAsync(Message message, Document document, global::LanguageCardsBot.Contracts.Cards.V3.User user, CancellationToken ct);
}
