using LanguageCardsBot.Contracts.Cards.V3;
using LanguageCardsBot.Presentation.Abstractions;
using LanguageCardsBot.Presentation.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace LanguageCardsBot.Presentation.Handlers;

/// <summary>Handles free-text messages as card input: parses lines and adds cards via gRPC.</summary>
public class CardInputHandler(
    ITelegramBotClient botClient,
    CardService.CardServiceClient cardService) : ICardInputHandler
{
    /// <inheritdoc/>
    public async Task HandleAsync(Message message, string text, global::LanguageCardsBot.Contracts.Cards.V3.User user, CancellationToken ct)
    {
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var added = new List<(string Term, string Translation)>();
        var errors = new List<string>();

        foreach (var line in lines)
        {
            try
            {
                var (term, translation, _) = ParseWordWithTranslation(line);
                if (string.IsNullOrEmpty(term)) continue;

                await cardService.AddAsync(
                    new AddCardRequest { UserId = user.Id, Term = term, Translation = translation },
                    cancellationToken: ct);
                added.Add((term, translation));
            }
            catch (Exception ex)
            {
                errors.Add($"'{line}': {ex.Message}");
            }
        }

        if (added.Count == 1)
        {
            var (term, translation) = added[0];
            var msg =
                $"Добавил карточку ✅\n\n" +
                $"*Слово*: {term}\n" +
                $"Перевод: ||{translation}||\n\n" +
                $"Я буду напоминать это слово по интервальному расписанию.";
            await botClient.SendFormattedMessageAsync(message.Chat.Id, msg, ct: ct);
        }
        else if (added.Count > 1)
        {
            var msg = $"Добавил *{added.Count}* карточек ✅\n\n";
            foreach (var (term, translation) in added)
                msg += $"• {term} — ||{translation}||\n";
            msg += "\nПереводы скрыты как спойлеры. Нажми, чтобы увидеть.";
            await botClient.SendFormattedMessageAsync(message.Chat.Id, msg, ct: ct);
        }

        if (errors.Any())
        {
            var errorMsg = "\n\nОшибки:\n" + string.Join("\n", errors.Select(e => $"• {e}"));
            await botClient.SendFormattedMessageAsync(message.Chat.Id, errorMsg, ct: ct);
        }
    }

    private static (string Term, string Translation, bool UseAuto) ParseWordWithTranslation(string line)
    {
        line = line.Trim();
        if (string.IsNullOrEmpty(line)) return ("", "", true);

        if (line.Contains(" | "))
        {
            var parts = line.Split(" | ", 2);
            if (parts.Length == 2)
            {
                var term = parts[0].Trim();
                var translation = parts[1].Trim();
                if (!string.IsNullOrEmpty(term) && !string.IsNullOrEmpty(translation))
                    return (term, translation, false);
            }
        }

        if (line.Contains(':') && !line.StartsWith("http"))
        {
            var parts = line.Split(":", 2);
            if (parts.Length == 2)
            {
                var term = parts[0].Trim();
                var translation = parts[1].Trim();
                if (!string.IsNullOrEmpty(term) && !string.IsNullOrEmpty(translation)
                    && term.Length < 100 && translation.Length < 200)
                    return (term, translation, false);
            }
        }

        if (line.Contains('–') || line.Contains('—'))
        {
            var separator = line.Contains('—') ? "—" : "–";
            var parts = line.Split(separator, 2);
            if (parts.Length == 2)
            {
                var term = parts[0].Trim();
                var translation = parts[1].Trim();
                if (!string.IsNullOrEmpty(term) && !string.IsNullOrEmpty(translation))
                    return (term, translation, false);
            }
        }

        if (line.Contains(" - "))
        {
            var parts = line.Split(" - ", 2);
            if (parts.Length == 2)
            {
                var term = parts[0].Trim();
                var translation = parts[1].Trim();
                if (!string.IsNullOrEmpty(term) && !string.IsNullOrEmpty(translation))
                    return (term, translation, false);
            }
        }

        return (line, "", true);
    }
}
