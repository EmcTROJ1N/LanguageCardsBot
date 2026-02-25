
using EnglishCardsBot.Domain.Entities;
using EnglishCardsBot.Presentation.Abstractions;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace EnglishCardsBot.Presentation.Commands.Start;

public sealed class StartCommandHandler(ITelegramBotClient botClient): ICommandHandler<StartCommand>
{
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
                                   "Используй меню внизу для быстрого доступа к функциям!";

        await botClient.SendMessage(
            chatId: command.ChatId,
            text: welcomeText,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
    }
}