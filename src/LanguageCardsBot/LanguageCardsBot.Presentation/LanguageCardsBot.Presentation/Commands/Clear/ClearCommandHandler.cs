using LanguageCardsBot.Contracts.Cards.V3;
using LanguageCardsBot.Presentation.Abstractions;
using Telegram.Bot;

namespace LanguageCardsBot.Presentation.Commands.Clear;

public class ClearCommandHandler(ITelegramBotClient botClient,
    CardService.CardServiceClient cardService): ICommandHandler<ClearCommand>
{
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