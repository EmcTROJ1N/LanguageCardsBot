using LanguageCardsBot.Contracts.Messaging.Events;
using LanguageCardsBot.Presentation.Services;
using MassTransit;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace LanguageCardsBot.Presentation.Consumers;

public class CardReminderConsumer(
    ILogger<CardReminderConsumer> logger,
    ITelegramBotClient botClient) : IConsumer<CardReminderEvent>
{
    public async Task Consume(ConsumeContext<CardReminderEvent> context)
    {
        var e = context.Message;

        var term = MarkdownV2Escaper.Escape(e.Term);
        var translation = MarkdownV2Escaper.Escape(e.Translation);

        var text = e.HideTranslation
            ? $"{term} — ||{translation}||"
            : $"{term} — {translation}";

        await botClient.SendMessage(
            chatId: e.ChatId,
            text: text,
            parseMode: ParseMode.MarkdownV2,
            cancellationToken: context.CancellationToken);

        logger.LogInformation("Reminder sent to {ChatId}: {Term}", e.ChatId, e.Term);
    }
}