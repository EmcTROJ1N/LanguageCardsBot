using System.Text.Json;
using LanguageCardsBot.Contracts.Cards.V3;
using LanguageCardsBot.Presentation.Abstractions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace LanguageCardsBot.Presentation.Commands.Import;

/// <summary>
/// Handles the /import command (shows instructions) and incoming JSON document uploads (performs import).
/// </summary>
public class ImportCommandHandler(
    ITelegramBotClient botClient,
    CardsImportService.CardsImportServiceClient cardsImportService)
    : ICommandHandler<ImportCommand>, IDocumentHandler
{
    /// <inheritdoc/>
    public async Task HandleAsync(ImportCommand command, LanguageCardsBot.Contracts.Cards.V3.User user, CancellationToken cancellationToken = default)
    {
        await botClient.SendMessage(
            chatId: command.ChatId,
            text: "📥 Для импорта карточек отправьте мне JSON файл с карточками.\n\n" +
                  "Форматы:\n" +
                  "1) Экспортный формат (через /export)\n" +
                  "2) Упрощённый формат:\n" +
                  "{\n" +
                  "  \"cards\": [\n" +
                  "    {\n" +
                  "      \"term\": \"слово\",\n" +
                  "      \"translation\": \"перевод\",\n" +
                  "      \"transcription\": \"/транскрипция/\",\n" +
                  "      \"example\": \"пример\",\n" +
                  "      \"level\": 1,\n" +
                  "      \"learned\": false\n" +
                  "    }\n" +
                  "  ]\n" +
                  "}\n\n" +
                  "Также поддерживается массив карточек в корне: [ {\"term\":\"...\",\"translation\":\"...\"}, ... ]",
            cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task HandleAsync(Telegram.Bot.Types.Message message, Telegram.Bot.Types.Document document, LanguageCardsBot.Contracts.Cards.V3.User user, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(document.FileName) ||
            !document.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            await botClient.SendMessage(
                chatId: message.Chat.Id,
                text: "❌ Пожалуйста, отправьте JSON файл (*.json). Используйте /export для примера формата.",
                cancellationToken: ct);
            return;
        }

        const long maxBytes = 2 * 1024 * 1024;
        if (document.FileSize is long size && size > maxBytes)
        {
            await botClient.SendMessage(
                chatId: message.Chat.Id,
                text: $"❌ Файл слишком большой ({size / 1024} KB). Пожалуйста, отправьте файл до {maxBytes / 1024} KB.",
                cancellationToken: ct);
            return;
        }

        await botClient.SendMessage(
            chatId: message.Chat.Id,
            text: "📥 Получил файл. Импортирую карточки...",
            cancellationToken: ct);

        try
        {
            var file = await botClient.GetFile(document.FileId, ct);
            if (string.IsNullOrWhiteSpace(file.FilePath))
                throw new InvalidOperationException("Не удалось получить путь к файлу в Telegram.");

            await using var ms = new MemoryStream();
            await botClient.DownloadFile(file.FilePath, ms, ct);
            ms.Position = 0;

            using var sr = new StreamReader(ms, System.Text.Encoding.UTF8,
                detectEncodingFromByteOrderMarks: true, leaveOpen: true);
            var json = await sr.ReadToEndAsync(ct);

            if (string.IsNullOrWhiteSpace(json))
            {
                await botClient.SendMessage(
                    chatId: message.Chat.Id,
                    text: "❌ Файл пустой. Проверьте содержимое JSON.",
                    cancellationToken: ct);
                return;
            }

            var result = await cardsImportService.ImportCardsFromJsonAsync(
                new ImportCardsFromJsonRequest { Json = json, UserId = user.Id },
                cancellationToken: ct);

            if (result is { IsSuccess: true, Data: not null })
            {
                var report =
                    $"✅ Импорт завершён.\n\n" +
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

                await botClient.SendMessage(chatId: message.Chat.Id, text: report, cancellationToken: ct);
            }
            else
            {
                await botClient.SendMessage(
                    chatId: message.Chat.Id,
                    text: $"❌ {result.Errors.First().Message}",
                    cancellationToken: ct);
            }
        }
        catch (JsonException ex)
        {
            await botClient.SendMessage(
                chatId: message.Chat.Id,
                text: $"❌ Ошибка при парсинге JSON: {ex.Message}",
                cancellationToken: ct);
        }
        catch (Exception ex)
        {
            await botClient.SendMessage(
                chatId: message.Chat.Id,
                text: $"❌ Ошибка при импорте: {ex.Message}",
                cancellationToken: ct);
        }
    }
}