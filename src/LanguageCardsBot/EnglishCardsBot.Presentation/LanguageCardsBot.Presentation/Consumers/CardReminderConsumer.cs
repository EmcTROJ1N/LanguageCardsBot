using LanguageCardsBot.Contracts.Messaging.Events;
using MassTransit;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace EnglishCardsBot.Presentation.Consumers;

public class CardReminderConsumer(
    ILogger<CardReminderConsumer> logger,
    ITelegramBotClient botClient) : IConsumer<CardReminderEvent>
{
    public async Task Consume(ConsumeContext<CardReminderEvent> context)
    {
        var e = context.Message;

        var text = e.HideTranslation
            ? $"{e.Term} — ||{e.Translation}||"
            : $"{e.Term} — {e.Translation}";

        await botClient.SendMessage(
            chatId: e.ChatId,
            text: text,
            parseMode: ParseMode.MarkdownV2,
            cancellationToken: context.CancellationToken);

        logger.LogInformation("Reminder sent to {ChatId}: {Term}", e.ChatId, e.Term);
    }
}