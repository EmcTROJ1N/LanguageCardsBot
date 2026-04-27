using EnglishCardsBot.Presentation.Abstractions;
using LanguageCardsBot.Contracts.Cards.V3;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace EnglishCardsBot.Presentation.Commands.Train;

public class TrainCommandHandle(ITelegramBotClient botClient, CardService.CardServiceClient cardService): ICommandHandler<TrainCommand>
{
    public async Task HandleAsync(TrainCommand command, User user, CancellationToken cancellationToken = default)
    {
        var response = await cardService.GetDueCardAsync(new GetDueCardRequest { UserId = user.Id }, cancellationToken: cancellationToken);
        
        if (response == null)
        {
            await botClient.SendMessage(
                chatId: command.ChatId,
                text: "Сейчас нет карточек, которые пора повторять 🎉\n\nДобавь новые слова или подожди до следующего интервала.",
                cancellationToken: cancellationToken);
            return;
        }

        var text = BuildTrainingMessage(response.Card, user.HideTranslations);
        var keyboard = new InlineKeyboardMarkup([
            [
                InlineKeyboardButton.WithCallbackData("Знал 😎", $"know_{response.Card.Id}"),
                InlineKeyboardButton.WithCallbackData("Не знал 😕", $"dontknow_{response.Card.Id}")
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