using EnglishCardsBot.Application.Interfaces;
using EnglishCardsBot.Domain.Entities;
using EnglishCardsBot.Presentation.Abstractions;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace EnglishCardsBot.Presentation.Commands.Train;

public class TrainCommandHandle(ITelegramBotClient botClient, ICardRepository cardRepository): ICommandHandler<TrainCommand>
{
    public async Task HandleAsync(TrainCommand command, User user, CancellationToken cancellationToken = default)
    {
        var card = await cardRepository.GetDueCardAsync(user.Id, cancellationToken);
        if (card == null)
        {
            await botClient.SendMessage(
                chatId: command.ChatId,
                text: "Сейчас нет карточек, которые пора повторять 🎉\n\nДобавь новые слова или подожди до следующего интервала.",
                cancellationToken: cancellationToken);
            return;
        }

        var text = BuildTrainingMessage(card, user.HideTranslations);
        var keyboard = new InlineKeyboardMarkup([
            [
                InlineKeyboardButton.WithCallbackData("Знал 😎", $"know_{card.Id}"),
                InlineKeyboardButton.WithCallbackData("Не знал 😕", $"dontknow_{card.Id}")
            ]
        ]);

        await botClient.SendMessage(
            chatId: command.ChatId,
            text: text,
            parseMode: ParseMode.MarkdownV2,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
    }
    
    private string BuildTrainingMessage(Card card, bool hideTranslation)
    {
        var translation = hideTranslation ? $"||{card.Translation}||" : card.Translation;
        var example = string.IsNullOrEmpty(card.Example)
            ? ""
            : hideTranslation ? $"||{card.Example}||" : card.Example;

        var text = $"💡 *Слово*: {card.Term}\nПеревод: {translation}";
        if (!string.IsNullOrEmpty(example))
        {
            text += $"\nПример: {example}";
        }

        return text;
    }
}