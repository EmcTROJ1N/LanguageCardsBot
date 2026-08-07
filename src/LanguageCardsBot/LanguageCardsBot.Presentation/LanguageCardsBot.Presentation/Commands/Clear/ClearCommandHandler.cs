using LanguageCardsBot.Contracts.Cards.V3;
using LanguageCardsBot.Presentation.Abstractions;
using Telegram.Bot;

namespace LanguageCardsBot.Presentation.Commands.Clear;

/// <summary>
/// Handles the /clear command: deletes all cards owned by the current user and reports the result.
/// </summary>
public class ClearCommandHandler(ITelegramBotClient botClient,
    CardService.CardServiceClient cardService): ICommandHandler<ClearCommand>
{
    /// <inheritdoc />
    public async Task HandleAsync(ClearCommand command, User user, CancellationToken cancellationToken = default)
    {
        var response = await cardService.DeleteByUserIdAsync(new DeleteCardsByUserIdRequest() { UserId = user.Id },
            cancellationToken: cancellationToken);
        
        // TODO: return deleted cards count
        await botClient.SendMessage(
            chatId: command.ChatId,
            text: response.Deleted
                ? $"✅ Все карточки успешно очищены."
                : "У вас нет карточек для удаления.",
            cancellationToken: cancellationToken);
    }
}
