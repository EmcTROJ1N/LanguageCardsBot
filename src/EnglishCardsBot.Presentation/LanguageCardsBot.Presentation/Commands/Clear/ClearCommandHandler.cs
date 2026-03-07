using EnglishCardsBot.Application.Interfaces;
using EnglishCardsBot.Domain.Entities;
using EnglishCardsBot.Presentation.Abstractions;
using Telegram.Bot;

namespace EnglishCardsBot.Presentation.Commands.Clear;

public class ClearCommandHandler(ITelegramBotClient botClient,
    ICardRepository cardRepository): ICommandHandler<ClearCommand>
{
    public async Task HandleAsync(ClearCommand command, User user, CancellationToken cancellationToken = default)
    {
        var deletedCount = await cardRepository.DeleteAllByUserIdAsync(user.Id, cancellationToken);

        await botClient.SendMessage(
            chatId: command.ChatId,
            text: deletedCount > 0
                ? $"✅ Удалено карточек: {deletedCount}\n\nВсе карточки успешно очищены."
                : "У вас нет карточек для удаления.",
            cancellationToken: cancellationToken);

    }
}