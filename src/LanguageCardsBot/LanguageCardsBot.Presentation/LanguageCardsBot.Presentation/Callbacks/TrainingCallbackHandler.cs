using LanguageCardsBot.Contracts.Cards.V3;
using LanguageCardsBot.Presentation.Abstractions;
using LanguageCardsBot.Presentation.Helpers;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace LanguageCardsBot.Presentation.Callbacks;

/// <summary>Handles training review callbacks: know_ and dontknow_, then shows the next due card.</summary>
public class TrainingCallbackHandler(
    ITelegramBotClient botClient,
    CardService.CardServiceClient cardService) : ICallbackHandler
{
    /// <inheritdoc/>
    public bool CanHandle(string callbackData)
        => callbackData.StartsWith("know_", StringComparison.Ordinal)
        || callbackData.StartsWith("dontknow_", StringComparison.Ordinal);

    /// <inheritdoc/>
    public async Task HandleAsync(CallbackQuery callbackQuery, LanguageCardsBot.Contracts.Cards.V3.User user, CancellationToken ct)
    {
        var data = callbackQuery.Data ?? "";
        var chatId = callbackQuery.Message!.Chat.Id;
        var messageId = callbackQuery.Message.MessageId;

        if (data.StartsWith("know_", StringComparison.Ordinal))
        {
            var cardId = int.Parse(data.Split('_')[1]);
            await cardService.UpdateCardReviewAsync(
                new UpdateCardReviewRequest { CardId = cardId, IsCorrect = true },
                cancellationToken: ct);
            await botClient.EditMessageReplyMarkup(
                chatId: chatId, messageId: messageId, replyMarkup: null, cancellationToken: ct);
            await botClient.SendMessage(
                chatId: chatId, text: "Отлично! Повышаю уровень карточки 🚀", cancellationToken: ct);
        }
        else if (data.StartsWith("dontknow_", StringComparison.Ordinal))
        {
            var cardId = int.Parse(data.Split('_')[1]);
            await cardService.UpdateCardReviewAsync(
                new UpdateCardReviewRequest { CardId = cardId, IsCorrect = false },
                cancellationToken: ct);
            await botClient.EditMessageReplyMarkup(
                chatId: chatId, messageId: messageId, replyMarkup: null, cancellationToken: ct);
            await botClient.SendMessage(
                chatId: chatId, text: "Ничего страшного! Я покажу это слово пораньше 😉", cancellationToken: ct);
        }

        var dueResponse = await cardService.GetDueCardAsync(
            new GetDueCardRequest { UserId = user.Id }, cancellationToken: ct);

        if (dueResponse.Card is not null)
        {
            var text = TrainingMessageBuilder.Build(dueResponse.Card, user.HideTranslations);
            var keyboard = new InlineKeyboardMarkup([[
                InlineKeyboardButton.WithCallbackData("Знал 😎", $"know_{dueResponse.Card.Id}"),
                InlineKeyboardButton.WithCallbackData("Не знал 😕", $"dontknow_{dueResponse.Card.Id}")
            ]]);
            await botClient.SendMessage(
                chatId: chatId, text: text,
                parseMode: ParseMode.MarkdownV2, replyMarkup: keyboard, cancellationToken: ct);
        }
        else
        {
            await botClient.SendMessage(
                chatId: chatId, text: "На сейчас всё, карточки закончились 🎉", cancellationToken: ct);
        }
    }
}
