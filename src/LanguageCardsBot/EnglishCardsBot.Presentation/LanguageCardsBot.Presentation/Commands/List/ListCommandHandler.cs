using EnglishCardsBot.Presentation.Abstractions;
using LanguageCardsBot.Contracts.Cards.V3;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace EnglishCardsBot.Presentation.Commands.List;

public class ListCommandHandler(ITelegramBotClient botClient,
    CardService.CardServiceClient cardRepository): ICommandHandler<ListCommand>
{
    
    // =========================
    // Cards list (InlineKeyboard)
    // =========================
    private const int CardsPerPage = 20;

    // callback formats (keep <= 64 bytes):
    // cards:page:{p}
    // cards:show:{cardId}:{p}
    // cards:del:{cardId}:{p}
    // cards:close
    private const string CardsCbPrefix = "cards";
 
    
    public async Task HandleAsync(ListCommand command, User user, CancellationToken cancellationToken = default)
    {
        var cards = (await cardRepository.GetByUserIdAsync(new GetCardsByUserIdRequest() { UserId = user.Id }, 
                cancellationToken: cancellationToken))
            .Cards
            .OrderBy(c => c.Term)
            .ToList();

        if (!cards.Any())
        {
            await botClient.SendMessage(
                chatId: command.ChatId,
                text: "У вас пока нет карточек.\n\nОтправьте мне слова, и я добавлю их в ваш список!",
                cancellationToken: cancellationToken);
            return;
        }

        var page = 0;
        var (text, keyboard) = BuildCardsListPage(cards, page);

        await botClient.SendMessage(
            chatId: command.ChatId,
            text: text,                 // plain text, no markdown
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
    }
    
    private (string Text, InlineKeyboardMarkup Keyboard) BuildCardsListPage(
        List<Card> cards,
        int page)
    {
        var total = cards.Count;
        var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)CardsPerPage));
        page = Math.Clamp(page, 0, totalPages - 1);

        var skip = page * CardsPerPage;
        var pageCards = cards.Skip(skip).Take(CardsPerPage).ToList();

        var header =
            $"📚 Карточки — страница {page + 1}/{totalPages}\n" +
            $"Всего: {total}\n" +
            "Нажмите на карточку, чтобы увидеть подробности.";

        // 2 колонки: term — translation
        var rows = new List<List<InlineKeyboardButton>>();
        for (int i = 0; i < pageCards.Count; i += 2)
        {
            var row = new List<InlineKeyboardButton>();

            var left = pageCards[i];
            row.Add(InlineKeyboardButton.WithCallbackData(
                BuildCardButtonLabel(left.Term, left.Translation),
                $"{CardsCbPrefix}:show:{left.Id}:{page}"
            ));

            if (i + 1 < pageCards.Count)
            {
                var right = pageCards[i + 1];
                row.Add(InlineKeyboardButton.WithCallbackData(
                    BuildCardButtonLabel(right.Term, right.Translation),
                    $"{CardsCbPrefix}:show:{right.Id}:{page}"
                ));
            }

            rows.Add(row);
        }

        // navigation row
        var nav = new List<InlineKeyboardButton>();
        if (page > 0)
            nav.Add(InlineKeyboardButton.WithCallbackData("⬅️ Назад", $"{CardsCbPrefix}:page:{page - 1}"));
        if (page < totalPages - 1)
            nav.Add(InlineKeyboardButton.WithCallbackData("➡️ Вперёд", $"{CardsCbPrefix}:page:{page + 1}"));

        nav.Add(InlineKeyboardButton.WithCallbackData("✖️ Закрыть", $"{CardsCbPrefix}:close"));
        rows.Add(nav);

        return (header, new InlineKeyboardMarkup(rows));
    }
    
    private static string BuildCardButtonLabel(string? term, string? translation)
    {
        var t = (term ?? "").Trim();
        var tr = (translation ?? "").Trim();

        var label = $"{t} — {tr}";
        const int maxLen = 55;
        if (label.Length > maxLen)
            label = label[..(maxLen - 1)] + "…";

        return label;
    }

}