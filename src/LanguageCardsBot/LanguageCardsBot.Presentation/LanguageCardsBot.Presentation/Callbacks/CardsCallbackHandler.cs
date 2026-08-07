using LanguageCardsBot.Contracts.Cards.V3;
using LanguageCardsBot.Presentation.Abstractions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace LanguageCardsBot.Presentation.Callbacks;

/// <summary>Handles paginated card list callbacks: cards:page, cards:show, cards:del, cards:close.</summary>
public class CardsCallbackHandler(
    ITelegramBotClient botClient,
    CardService.CardServiceClient cardService) : ICallbackHandler
{
    private const int CardsPerPage = 20;
    private const string Prefix = "cards";

    /// <inheritdoc/>
    public bool CanHandle(string callbackData)
        => callbackData.StartsWith($"{Prefix}:", StringComparison.Ordinal);

    /// <inheritdoc/>
    public async Task HandleAsync(CallbackQuery callbackQuery, global::LanguageCardsBot.Contracts.Cards.V3.User user, CancellationToken ct)
    {
        var data = callbackQuery.Data ?? "";
        var chatId = callbackQuery.Message!.Chat.Id;
        var messageId = callbackQuery.Message.MessageId;

        var cards = (await cardService.GetByUserIdAsync(
                new GetCardsByUserIdRequest { UserId = user.Id }, cancellationToken: ct))
            .Cards
            .OrderBy(c => c.Term)
            .ToList();

        var parts = data.Split(':', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2) return;

        var action = parts[1];

        if (action == "close")
        {
            await botClient.EditMessageText(
                chatId: chatId, messageId: messageId,
                text: "Список карточек закрыт.", replyMarkup: null, cancellationToken: ct);
            return;
        }

        if (action == "page" && parts.Length >= 3 && int.TryParse(parts[2], out var page))
        {
            var (text, keyboard) = BuildCardsListPage(cards, page);
            await botClient.EditMessageText(
                chatId: chatId, messageId: messageId,
                text: text, replyMarkup: keyboard, cancellationToken: ct);
            return;
        }

        if (action == "show" && parts.Length >= 4
            && int.TryParse(parts[2], out var cardId)
            && int.TryParse(parts[3], out var pageFrom))
        {
            var card = cards.FirstOrDefault(c => c.Id == cardId);
            if (card is null)
            {
                await botClient.SendMessage(
                    chatId: chatId, text: "Карточка не найдена. Обновите список /cards.", cancellationToken: ct);
                return;
            }

            var status = card.Learned ? "✅ Выучено" : $"🧩 Уровень: {card.Level}";
            var transcription = string.IsNullOrWhiteSpace(card.Transcription) ? "—" : card.Transcription;
            var example = string.IsNullOrWhiteSpace(card.Example) ? "—" : card.Example;

            var details =
                $"📝 {card.Term}\n" +
                $"Перевод: {card.Translation}\n" +
                $"Транскрипция: {transcription}\n" +
                $"Пример: {example}\n" +
                $"{status}";

            var detailsKeyboard = new InlineKeyboardMarkup([
                [InlineKeyboardButton.WithCallbackData("⬅️ Назад к списку", $"{Prefix}:page:{pageFrom}")],
                [InlineKeyboardButton.WithCallbackData("🗑 Удалить карточку", $"{Prefix}:del:{card.Id}:{pageFrom}")]
            ]);

            await botClient.SendMessage(
                chatId: chatId, text: details, replyMarkup: detailsKeyboard, cancellationToken: ct);
            return;
        }

        if (action == "del" && parts.Length >= 4
            && int.TryParse(parts[2], out var deleteCardId)
            && int.TryParse(parts[3], out var pageFromDel))
        {
            var card = cards.FirstOrDefault(c => c.Id == deleteCardId);
            if (card is null)
            {
                await botClient.SendMessage(
                    chatId: chatId, text: "Карточка не найдена или уже удалена.", cancellationToken: ct);
                return;
            }

            await cardService.DeleteByIdAsync(
                new DeleteCardByIdRequest { Id = card.Id }, cancellationToken: ct);

            await botClient.SendMessage(
                chatId: chatId, text: $"🗑 Карточка удалена: {card.Term}", cancellationToken: ct);

            var freshCards = (await cardService.GetByUserIdAsync(
                    new GetCardsByUserIdRequest { UserId = user.Id }, cancellationToken: ct))
                .Cards.OrderBy(c => c.Term).ToList();

            if (!freshCards.Any())
            {
                await botClient.EditMessageText(
                    chatId: chatId, messageId: messageId,
                    text: "У вас пока нет карточек.", replyMarkup: null, cancellationToken: ct);
                return;
            }

            var (text, keyboard) = BuildCardsListPage(freshCards, pageFromDel);
            await botClient.EditMessageText(
                chatId: chatId, messageId: messageId,
                text: text, replyMarkup: keyboard, cancellationToken: ct);
        }
    }

    private static (string Text, InlineKeyboardMarkup Keyboard) BuildCardsListPage(List<Card> cards, int page)
    {
        var total = cards.Count;
        var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)CardsPerPage));
        page = Math.Clamp(page, 0, totalPages - 1);

        var pageCards = cards.Skip(page * CardsPerPage).Take(CardsPerPage).ToList();

        var header =
            $"📚 Карточки — страница {page + 1}/{totalPages}\n" +
            $"Всего: {total}\n" +
            "Нажмите на карточку, чтобы увидеть подробности.";

        var rows = new List<List<InlineKeyboardButton>>();
        for (int i = 0; i < pageCards.Count; i += 2)
        {
            var row = new List<InlineKeyboardButton>();
            var left = pageCards[i];
            row.Add(InlineKeyboardButton.WithCallbackData(
                BuildCardButtonLabel(left.Term, left.Translation),
                $"{Prefix}:show:{left.Id}:{page}"));

            if (i + 1 < pageCards.Count)
            {
                var right = pageCards[i + 1];
                row.Add(InlineKeyboardButton.WithCallbackData(
                    BuildCardButtonLabel(right.Term, right.Translation),
                    $"{Prefix}:show:{right.Id}:{page}"));
            }
            rows.Add(row);
        }

        var nav = new List<InlineKeyboardButton>();
        if (page > 0)
            nav.Add(InlineKeyboardButton.WithCallbackData("⬅️ Назад", $"{Prefix}:page:{page - 1}"));
        if (page < totalPages - 1)
            nav.Add(InlineKeyboardButton.WithCallbackData("➡️ Вперёд", $"{Prefix}:page:{page + 1}"));
        nav.Add(InlineKeyboardButton.WithCallbackData("✖️ Закрыть", $"{Prefix}:close"));
        rows.Add(nav);

        return (header, new InlineKeyboardMarkup(rows));
    }

    private static string BuildCardButtonLabel(string? term, string? translation)
    {
        var label = $"{(term ?? "").Trim()} — {(translation ?? "").Trim()}";
        const int maxLen = 55;
        return label.Length > maxLen ? label[.. (maxLen - 1)] + "…" : label;
    }
}
