using LanguageCardsBot.Contracts.Cards.V3;
using LanguageCardsBot.Presentation.Abstractions;
using LanguageCardsBot.Presentation.Helpers;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace LanguageCardsBot.Presentation.Commands.Train;

/// <summary>Handles the /train command: shows the next due card for spaced-repetition review.</summary>
public class TrainCommandHandler(
    ITelegramBotClient botClient,
    CardService.CardServiceClient cardService) : ICommandHandler<TrainCommand>
{
    /// <inheritdoc/>
    public async Task HandleAsync(TrainCommand command, User user, CancellationToken cancellationToken = default)
    {
        var response = await cardService.GetDueCardAsync(
            new GetDueCardRequest { UserId = user.Id }, cancellationToken: cancellationToken);

        if (response.Card is null)
        {
            await botClient.SendMessage(
                chatId: command.ChatId,
                text: "Сейчас нет карточек, которые пора повторять 🎉\n\nДобавь новые слова или подожди до следующего интервала.",
                cancellationToken: cancellationToken);
            return;
        }

        var text = TrainingMessageBuilder.Build(response.Card, user.HideTranslations);
        var keyboard = new InlineKeyboardMarkup([[
            InlineKeyboardButton.WithCallbackData("Знал 😎", $"know_{response.Card.Id}"),
            InlineKeyboardButton.WithCallbackData("Не знал 😕", $"dontknow_{response.Card.Id}")
        ]]);

        await botClient.SendMessage(
            chatId: command.ChatId,
            text: text,
            parseMode: ParseMode.MarkdownV2,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
    }
}
