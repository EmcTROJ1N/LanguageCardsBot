using LanguageCardsBot.Contracts.Cards.V3;
using LanguageCardsBot.Presentation.Abstractions;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace LanguageCardsBot.Presentation.Commands.Start;

/// <summary>
/// Handles the initial bot command and sends the main reply keyboard.
/// </summary>
public sealed class StartCommandHandler(ITelegramBotClient botClient): ICommandHandler<StartCommand>
{
    /// <summary>
    /// Sends the welcome message and main keyboard.
    /// </summary>
    public async Task HandleAsync(StartCommand command, User _, CancellationToken cancellationToken = default)
    {
        var keyboard = new ReplyKeyboardMarkup([
            [new KeyboardButton("📚 Мои карточки"), new KeyboardButton("🎯 Тренировка")],
            [new KeyboardButton("📊 Статистика"), new KeyboardButton("⚙️ Настройки")],
            [new KeyboardButton("📤 Экспорт"), new KeyboardButton("📥 Импорт")]
        ])
        {
            ResizeKeyboard = true
        };

        const string welcomeText = "Привет! Я бот для интервального повторения слов 🌟\n\n" +
                                   "Просто отправь мне слово (или несколько слов построчно) — " +
                                   "я найду перевод, добавлю карточки и буду напоминать.\n\n" +
                                   "📝 Форматы добавления:\n" +
                                   "• Просто слово — автоматический перевод\n" +
                                   "• слово | перевод — с вашим переводом\n" +
                                   "• слово: перевод — альтернативный формат\n\n" +
                                   "Для Chrome-расширения: /user_id\n\n" +
                                   "Используй меню внизу для быстрого доступа к функциям!";

        await botClient.SendMessage(
            chatId: command.ChatId,
            text: welcomeText,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
    }
}
