using LanguageCardsBot.Presentation.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace LanguageCardsBot.Presentation.Extensions;

/// <summary>Convenience extensions on <see cref="ITelegramBotClient"/>.</summary>
public static class TelegramBotClientExtensions
{
    /// <summary>
    /// Sends a message with full MarkdownV2 escaping applied to <paramref name="text"/>.
    /// Do NOT use when <paramref name="text"/> already contains intentional Markdown tokens
    /// (*, ||, etc.) — they will be double-escaped.
    /// </summary>
    public static Task<Message> SendFormattedMessageAsync(
        this ITelegramBotClient client,
        long chatId,
        string text,
        ReplyMarkup? replyMarkup = null,
        CancellationToken ct = default)
    {
        var escaped = MarkdownV2Escaper.Escape(text);
        return client.SendMessage(
            chatId: chatId,
            text: escaped,
            parseMode: Telegram.Bot.Types.Enums.ParseMode.MarkdownV2,
            replyMarkup: replyMarkup,
            cancellationToken: ct);
    }
}
