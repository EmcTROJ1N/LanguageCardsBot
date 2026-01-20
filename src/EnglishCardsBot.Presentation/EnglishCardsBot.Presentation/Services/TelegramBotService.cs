using EnglishCardsBot.Application.Interfaces;
using EnglishCardsBot.Application.Services;
using System.Text.Json;
using EnglishCardsBot.Presentation.Commands.Clear;
using EnglishCardsBot.Presentation.Commands.Export;
using EnglishCardsBot.Presentation.Commands.Import;
using EnglishCardsBot.Presentation.Commands.List;
using EnglishCardsBot.Presentation.Commands.ReminderSettings;
using EnglishCardsBot.Presentation.Commands.Start;
using EnglishCardsBot.Presentation.Commands.Stats;
using EnglishCardsBot.Presentation.Commands.Train;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using HandleErrorSource = Telegram.Bot.Polling.HandleErrorSource;

namespace EnglishCardsBot.Presentation.Services;

public class TelegramBotService(
    ITelegramBotClient botClient,
    IUserRepository userRepository,
    ICardRepository cardRepository,
    ICardsImportService cardsImportService,
    CardService cardService,
    IServiceProvider serviceProvider,
    ITranslationService translationService)
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

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var updateHandler = new BotUpdateHandler(this);
        botClient.StartReceiving(
            updateHandler: updateHandler,
            cancellationToken: cancellationToken);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Sends a MarkdownV2 message with escaping. Supports optional reply markup.
    /// NOTE: This escapes the whole text. Use it for plain text that may contain special symbols,
    /// but do NOT use it if you expect Markdown tokens (*, ||, etc.) to render.
    /// </summary>
    public async Task<Message> SendFormattedMessageAsync(
        long chatId,
        string text,
        ReplyMarkup? replyMarkup = null,
        CancellationToken cancellationToken = default)
    {
        string escapedText = EscapeMarkdown(text);
        return await botClient.SendMessage(
            chatId: chatId,
            text: escapedText,
            parseMode: ParseMode.MarkdownV2,
            replyMarkup: replyMarkup,
            cancellationToken: cancellationToken);
    }

    public Task<Message> SendFormattedMessageAsync(long chatId, string text, CancellationToken cancellationToken = default)
        => SendFormattedMessageAsync(chatId, text, replyMarkup: null, cancellationToken);

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is { } message)
        {
            await HandleMessageAsync(message, cancellationToken);
        }
        else if (update.CallbackQuery is { } callbackQuery)
        {
            await HandleCallbackQueryAsync(callbackQuery, cancellationToken);
        }
    }

    private async Task HandleMessageAsync(Message message, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetOrCreateAsync(
            message.Chat.Id,
            message.From?.Username,
            cancellationToken);

        // Handle document upload for import
        if (message.Document is { } document)
        {
            await HandleDocumentAsync(message, document, user, cancellationToken);
            return;
        }

        if (message.Text is { } text)
        {
            if (text.StartsWith("/"))
            {
                await HandleCommandAsync(message, text, user, cancellationToken);
            }
            else
            {
                await HandleTextMessageAsync(message, text, user, cancellationToken);
            }
        }
    }

    private async Task HandleCommandAsync(Message message, string text, Domain.Entities.User user, CancellationToken cancellationToken)
    {
        var command = text.Split(' ')[0].ToLower();
        var args = text.Split(' ').Skip(1).ToArray();
        
        switch (command)
        {
            case "/start":
                await serviceProvider.GetRequiredService<StartCommandHandler>()
                    .HandleAsync(new StartCommand(message.Chat.Id), user, cancellationToken);
                break;
            case "/train":
                await serviceProvider.GetRequiredService<TrainCommandHandle>()
                    .HandleAsync(new TrainCommand(message.Chat.Id), user, cancellationToken);
                break;
            case "/stats":
                await serviceProvider.GetRequiredService<StatsCommandHandler>()
                    .HandleAsync(new StatCommand(message.Chat.Id), user, cancellationToken);
                break;
            case "/list":
            case "/cards":
                await serviceProvider.GetRequiredService<ListCommandHandler>()
                    .HandleAsync(new ListCommand(message.Chat.Id), user, cancellationToken);
                break;
            case "/reminder_settings":
                await serviceProvider.GetRequiredService<ReminderSettingsCommandHandler>()
                    .HandleAsync(new ReminderSettingsCommand(message.Chat.Id, args), user, cancellationToken);
                break;
            case "/clear":
                await serviceProvider.GetRequiredService<ClearCommandHandler>()
                    .HandleAsync(new ClearCommand(message.Chat.Id), user, cancellationToken);
                break;
            case "/export":
                await serviceProvider.GetRequiredService<ExportCommandHandler>()
                    .HandleAsync(new ExportCommand(message.Chat.Id), user, cancellationToken);
                break;
            case "/import":
                await serviceProvider.GetRequiredService<ImportCommandHandler>()
                    .HandleAsync(new ImportCommand(message.Chat.Id), user, cancellationToken);
                break;
        }
    }
    private (string Text, InlineKeyboardMarkup Keyboard) BuildCardsListPage(
        List<Domain.Entities.Card> cards,
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

    private async Task HandleCardsCallbackAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        var data = callbackQuery.Data ?? "";
        var chatId = callbackQuery.Message!.Chat.Id;
        var messageId = callbackQuery.Message.MessageId;

        // user нужен, чтобы знать userId
        var user = await userRepository.GetOrCreateAsync(
            chatId,
            callbackQuery.From?.Username,
            cancellationToken);

        // Stateless: берём актуальные карточки из репозитория (надежно при рестартах/масштабировании)
        var cards = (await cardRepository.GetAllByUserIdAsync(user.Id, cancellationToken))
            .OrderBy(c => c.Term)
            .ToList();

        // cards:page:{p} | cards:show:{cardId}:{p} | cards:del:{cardId}:{p} | cards:close
        var parts = data.Split(':', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2 || parts[0] != CardsCbPrefix)
            return;

        var action = parts[1];

        if (action == "close")
        {
            await botClient.EditMessageText(
                chatId: chatId,
                messageId: messageId,
                text: "Список карточек закрыт.",
                replyMarkup: null,
                cancellationToken: cancellationToken);
            return;
        }

        if (action == "page" && parts.Length >= 3 && int.TryParse(parts[2], out var page))
        {
            var (text, keyboard) = BuildCardsListPage(cards, page);

            await botClient.EditMessageText(
                chatId: chatId,
                messageId: messageId,
                text: text,
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);
            return;
        }

        if (action == "show" && parts.Length >= 4
            && int.TryParse(parts[2], out var cardId)
            && int.TryParse(parts[3], out var pageFrom))
        {
            var card = cards.FirstOrDefault(c => c.Id == cardId);
            if (card == null)
            {
                await botClient.SendMessage(
                    chatId: chatId,
                    text: "Карточка не найдена. Обновите список /cards.",
                    cancellationToken: cancellationToken);
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

            // NEW: delete button
            var detailsKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("⬅️ Назад к списку", $"{CardsCbPrefix}:page:{pageFrom}")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("🗑 Удалить карточку", $"{CardsCbPrefix}:del:{card.Id}:{pageFrom}")
                }
            });

            await botClient.SendMessage(
                chatId: chatId,
                text: details, // plain text
                replyMarkup: detailsKeyboard,
                cancellationToken: cancellationToken);

            return;
        }

        // NEW: delete flow
        if (action == "del" && parts.Length >= 4
            && int.TryParse(parts[2], out var deleteCardId)
            && int.TryParse(parts[3], out var pageFromDel))
        {
            // Ensure belongs to user
            var card = cards.FirstOrDefault(c => c.Id == deleteCardId);
            if (card == null)
            {
                await botClient.SendMessage(
                    chatId: chatId,
                    text: "Карточка не найдена или уже удалена.",
                    cancellationToken: cancellationToken);
                return;
            }

            // DELETE: adjust this line to your IRepository<T> contract if needed
            await cardRepository.DeleteAsync(card, cancellationToken);

            await botClient.SendMessage(
                chatId: chatId,
                text: $"🗑 Карточка удалена: {card.Term}",
                cancellationToken: cancellationToken);

            // Refresh list message (same messageId as inline list)
            var freshCards = (await cardRepository.GetAllByUserIdAsync(user.Id, cancellationToken))
                .OrderBy(c => c.Term)
                .ToList();

            if (!freshCards.Any())
            {
                await botClient.EditMessageText(
                    chatId: chatId,
                    messageId: messageId,
                    text: "У вас пока нет карточек.",
                    replyMarkup: null,
                    cancellationToken: cancellationToken);
                return;
            }

            var (text, keyboard) = BuildCardsListPage(freshCards, pageFromDel);

            await botClient.EditMessageText(
                chatId: chatId,
                messageId: messageId,
                text: text,
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);

            return;
        }
    }

    // =========================
    // IMPLEMENTED: file import (.json)
    // =========================
    private async Task HandleDocumentAsync(Message message, Document document, Domain.Entities.User user, CancellationToken cancellationToken)
    {
        // 1) Basic validation
        if (string.IsNullOrWhiteSpace(document.FileName) ||
            !document.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            await botClient.SendMessage(
                chatId: message.Chat.Id,
                text: "❌ Пожалуйста, отправьте JSON файл (*.json). Используйте /export для примера формата.",
                cancellationToken: cancellationToken);
            return;
        }

        // Guard size
        const long maxBytes = 2 * 1024 * 1024; // 2 MB
        if (document.FileSize is long size && size > maxBytes)
        {
            await botClient.SendMessage(
                chatId: message.Chat.Id,
                text: $"❌ Файл слишком большой ({size / 1024} KB). Пожалуйста, отправьте файл до {maxBytes / 1024} KB.",
                cancellationToken: cancellationToken);
            return;
        }

        await botClient.SendMessage(
            chatId: message.Chat.Id,
            text: "📥 Получил файл. Импортирую карточки...",
            cancellationToken: cancellationToken);

        try
        {
            // 2) Download file content
            var file = await botClient.GetFile(document.FileId, cancellationToken);
            if (string.IsNullOrWhiteSpace(file.FilePath))
                throw new InvalidOperationException("Не удалось получить путь к файлу в Telegram.");

            await using var ms = new MemoryStream();
            await botClient.DownloadFile(file.FilePath, ms, cancellationToken);
            ms.Position = 0;

            // 3) Read JSON
            using var sr = new StreamReader(ms, System.Text.Encoding.UTF8, detectEncodingFromByteOrderMarks: true,
                leaveOpen: true);
            var json = await sr.ReadToEndAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(json))
            {
                await botClient.SendMessage(
                    chatId: message.Chat.Id,
                    text: "❌ Файл пустой. Проверьте содержимое JSON.",
                    cancellationToken: cancellationToken);
            }

            var result = await cardsImportService.ImportCardsFromJsonAsync(json, user.Id, cancellationToken);

            // Report
            if (result is { IsSuccess: true, Data: not null })
            {
                var report = $"✅ Импорт завершён.\n\n" +
                             $"Добавлено карточек: {result.Data.Imported}\n" +
                             $"Пропущено: {result.Data.Skipped}";
                if (result.Data.Errors.Count > 0)
                {
                    const int maxErrorLines = 20;
                    var shown = result.Data.Errors.Take(maxErrorLines).ToList();
                    report += "\n\nОшибки/пропуски:\n" + string.Join("\n", shown);

                    if (result.Data.Errors.Count > maxErrorLines)
                        report += $"\n…и ещё {result.Data.Errors.Count - maxErrorLines} строк(и).";
                }

                await botClient.SendMessage(
                    chatId: message.Chat.Id,
                    text: report,
                    cancellationToken: cancellationToken);
            }
            else
                await botClient.SendMessage(
                    chatId: message.Chat.Id,
                    text: $"❌ {result.Errors.First().Message}",
                    cancellationToken: cancellationToken);
        }
        catch (JsonException ex)
        {
            await botClient.SendMessage(
                chatId: message.Chat.Id,
                text: $"❌ Ошибка при парсинге JSON: {ex.Message}",
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            await botClient.SendMessage(
                chatId: message.Chat.Id,
                text: $"❌ Ошибка при импорте: {ex.Message}",
                cancellationToken: cancellationToken);
        }
    }

    private async Task HandleTextMessageAsync(Message message, string text, Domain.Entities.User user, CancellationToken cancellationToken)
    {
        // Handle menu buttons
        if (text == "📚 Мои карточки")
        {
            await serviceProvider.GetRequiredService<ListCommandHandler>()
                .HandleAsync(new ListCommand(message.Chat.Id), user, cancellationToken);
            return;
        }
        if (text == "🎯 Тренировка")
        {
            await serviceProvider.GetRequiredService<TrainCommandHandle>()
                .HandleAsync(new TrainCommand(message.Chat.Id), user, cancellationToken);
            return;
        }
        if (text == "📊 Статистика")
        {
            await serviceProvider.GetRequiredService<StatsCommandHandler>()
                .HandleAsync(new StatCommand(message.Chat.Id), user, cancellationToken);
            return;
        }
        if (text == "⚙️ Настройки")
        {
            await serviceProvider.GetRequiredService<ReminderSettingsCommandHandler>()
                .HandleAsync(new ReminderSettingsCommand(message.Chat.Id, []), user, cancellationToken);
            return;
        }
        if (text == "📤 Экспорт")
        {
            await serviceProvider.GetRequiredService<ExportCommandHandler>()
                .HandleAsync(new ExportCommand(message.Chat.Id), user, cancellationToken);
            return;
        }
        if (text == "📥 Импорт")
        {
            await serviceProvider.GetRequiredService<ImportCommandHandler>()
                .HandleAsync(new ImportCommand(message.Chat.Id), user, cancellationToken);
            return;
        }

        // Handle word addition (as before)
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var added = new List<(string Term, string Translation)>();
        var errors = new List<string>();

        foreach (var line in lines)
        {
            try
            {
                var (term, translation, useAuto) = ParseWordWithTranslation(line);
                if (string.IsNullOrEmpty(term))
                    continue;

                if (useAuto)
                {
                    var (trans, transcription, example) = await translationService.TranslateAsync(term, cancellationToken);
                    if (string.IsNullOrEmpty(trans))
                    {
                        errors.Add($"'{term}': не удалось получить перевод");
                        continue;
                    }
                    translation = trans;
                }

                await cardService.AddCardAsync(user.Id, term, translation, $"/{term}/", null, cancellationToken);
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
            var msg = $"Добавил карточку ✅\n\n" +
                      $"*Слово*: {term}\n" +
                      $"Перевод: ||{translation}||\n\n" +
                      $"Я буду напоминать это слово по интервальному расписанию.";

            await SendFormattedMessageAsync(message.Chat.Id, msg, cancellationToken);
        }
        else if (added.Count > 1)
        {
            var msg = $"Добавил *{added.Count}* карточек ✅\n\n";
            foreach (var (term, translation) in added)
            {
                msg += $"• {term} — ||{translation}||\n";
            }
            msg += "\nПереводы скрыты как спойлеры. Нажми, чтобы увидеть.";

            await SendFormattedMessageAsync(message.Chat.Id, msg, cancellationToken);
        }

        if (errors.Any())
        {
            var errorMsg = "\n\nОшибки:\n" + string.Join("\n", errors.Select(e => $"• {e}"));
            await SendFormattedMessageAsync(message.Chat.Id, errorMsg, cancellationToken);
        }
    }

    private async Task HandleCallbackQueryAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);

        var data = callbackQuery.Data ?? "";

        // NEW: inline cards list callbacks
        if (data.StartsWith($"{CardsCbPrefix}:", StringComparison.Ordinal))
        {
            await HandleCardsCallbackAsync(callbackQuery, cancellationToken);
            return;
        }

        // Existing training callbacks
        var user = await userRepository.GetOrCreateAsync(
            callbackQuery.Message!.Chat.Id,
            callbackQuery.From?.Username,
            cancellationToken);

        if (data.StartsWith("know_"))
        {
            var cardId = int.Parse(data.Split('_')[1]);
            await cardService.UpdateCardReviewAsync(cardId, true, cancellationToken);
            await botClient.EditMessageReplyMarkup(
                chatId: callbackQuery.Message.Chat.Id,
                messageId: callbackQuery.Message.MessageId,
                replyMarkup: null,
                cancellationToken: cancellationToken);
            await botClient.SendMessage(
                chatId: callbackQuery.Message.Chat.Id,
                text: "Отлично! Повышаю уровень карточки 🚀",
                cancellationToken: cancellationToken);
        }
        else if (data.StartsWith("dontknow_"))
        {
            var cardId = int.Parse(data.Split('_')[1]);
            await cardService.UpdateCardReviewAsync(cardId, false, cancellationToken);
            await botClient.EditMessageReplyMarkup(
                chatId: callbackQuery.Message.Chat.Id,
                messageId: callbackQuery.Message.MessageId,
                replyMarkup: null,
                cancellationToken: cancellationToken);
            await botClient.SendMessage(
                chatId: callbackQuery.Message.Chat.Id,
                text: "Ничего страшного! Я покажу это слово пораньше 😉",
                cancellationToken: cancellationToken);
        }

        var nextCard = await cardRepository.GetDueCardAsync(user.Id, cancellationToken);
        if (nextCard != null)
        {
            var text = BuildTrainingMessage(nextCard, user.HideTranslations);
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Знал 😎", $"know_{nextCard.Id}"),
                    InlineKeyboardButton.WithCallbackData("Не знал 😕", $"dontknow_{nextCard.Id}")
                }
            });

            await SendFormattedMessageAsync(
                chatId: callbackQuery.Message!.Chat.Id,
                text: text,
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);
        }
        else
        {
            await botClient.SendMessage(
                chatId: callbackQuery.Message!.Chat.Id,
                text: "На сейчас всё, карточки закончились 🎉",
                cancellationToken: cancellationToken);
        }
    }

    private string BuildTrainingMessage(Domain.Entities.Card card, bool hideTranslation)
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

    private (string Term, string Translation, bool UseAuto) ParseWordWithTranslation(string line)
    {
        line = line.Trim();
        if (string.IsNullOrEmpty(line))
            return ("", "", true);

        // Format: "word | translation"
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

        // Format: "word: translation"
        if (line.Contains(":") && !line.StartsWith("http"))
        {
            var parts = line.Split(":", 2);
            if (parts.Length == 2)
            {
                var term = parts[0].Trim();
                var translation = parts[1].Trim();
                if (!string.IsNullOrEmpty(term) && !string.IsNullOrEmpty(translation) && term.Length < 100 && translation.Length < 200)
                    return (term, translation, false);
            }
        }

        // Format: "word – translation" (em dash or en dash)
        if (line.Contains("–") || line.Contains("—"))
        {
            var separator = line.Contains("—") ? "—" : "–";
            var parts = line.Split(separator, 2);
            if (parts.Length == 2)
            {
                var term = parts[0].Trim();
                var translation = parts[1].Trim();
                if (!string.IsNullOrEmpty(term) && !string.IsNullOrEmpty(translation))
                    return (term, translation, false);
            }
        }

        // Format: "word - translation"
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

    private string EscapeMarkdown(string text)
    {
        if (string.IsNullOrEmpty(text))
            return "";

        return text
            .Replace("\\", "\\\\")
            .Replace("_", "\\_")
            .Replace("*", "\\*")
            .Replace("[", "\\[")
            .Replace("]", "\\]")
            .Replace("(", "\\(")
            .Replace(")", "\\)")
            .Replace("~", "\\~")
            .Replace("`", "\\`")
            .Replace(">", "\\>")
            .Replace("#", "\\#")
            .Replace("+", "\\+")
            .Replace("-", "\\-")
            .Replace("=", "\\=")
            .Replace("|", "\\|")
            .Replace("{", "\\{")
            .Replace("}", "\\}")
            .Replace(".", "\\.")
            .Replace("!", "\\!");
    }

    internal Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Polling error: {exception.Message}");
        return Task.CompletedTask;
    }

    private class BotUpdateHandler : IUpdateHandler
    {
        private readonly TelegramBotService _botService;

        public BotUpdateHandler(TelegramBotService botService)
        {
            _botService = botService;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            await _botService.HandleUpdateAsync(botClient, update, cancellationToken);
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource errorSource, CancellationToken cancellationToken)
        {
            return _botService.HandlePollingErrorAsync(botClient, exception, cancellationToken);
        }
    }

}
