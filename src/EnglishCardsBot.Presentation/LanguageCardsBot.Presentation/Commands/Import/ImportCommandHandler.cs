using EnglishCardsBot.Domain.Entities;
using EnglishCardsBot.Presentation.Abstractions;
using Telegram.Bot;

namespace EnglishCardsBot.Presentation.Commands.Import;

public class ImportCommandHandler(ITelegramBotClient botClient): ICommandHandler<ImportCommand>
{
    public async Task HandleAsync(ImportCommand command, User user, CancellationToken cancellationToken = default)
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
}