using EnglishCardsBot.Presentation.Abstractions;
using LanguageCardsBot.Contracts.Cards.V3;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace EnglishCardsBot.Presentation.Commands.ReminderSettings;

public class ReminderSettingsCommandHandler(UserService.UserServiceClient userService, ITelegramBotClient botClient): ICommandHandler<ReminderSettingsCommand>
{
    public async Task HandleAsync(ReminderSettingsCommand command, User user, CancellationToken cancellationToken = default)
    {
        if (command.HideTranslations.HasValue)
        {
            user.HideTranslations = command.HideTranslations.Value;
            await userService.UpdateAsync(new UpdateUserRequest { User = user }, cancellationToken: cancellationToken);

            var status = user.HideTranslations ? "скрыты" : "показаны";
            await botClient.SendMessage(
                chatId: command.ChatId,
                text: $"✅ Настройка обновлена\\!\n\nПереводы теперь {status}",
                parseMode: ParseMode.MarkdownV2,
                cancellationToken: cancellationToken);

            return;
        }

        if (command.ReminderIntervalMinutes.HasValue)
        {
            user.ReminderIntervalMinutes = command.ReminderIntervalMinutes.Value;
            await userService.UpdateAsync(new UpdateUserRequest { User = user }, cancellationToken: cancellationToken);

            await botClient.SendMessage(
                chatId: command.ChatId,
                text: $"✅ Частота напоминаний обновлена\\!\n\nТеперь напоминания будут проверяться каждые *{command.ReminderIntervalMinutes}* минут",
                parseMode: ParseMode.MarkdownV2,
                cancellationToken: cancellationToken);
            return;
        }

        var translationsStatus = user.HideTranslations ? "скрыты" : "показаны";
        var settingsText = $"⚙️ *Настройки*\n\n" +
                           $"🔔 Частота напоминаний:\n" +
                           $"Текущая частота проверки: *{user.ReminderIntervalMinutes}* минут\n" +
                           $"Чтобы изменить: `/reminder_settings <минуты>`\n\n" +
                           $"👁️ Отображение переводов:\n" +
                           $"Переводы: *{translationsStatus}*\n" +
                           $"Чтобы изменить:\n" +
                           $"`/reminder_settings hide` — скрыть переводы\n" +
                           $"`/reminder_settings show` — показать переводы";

        await botClient.SendMessage(
            chatId: command.ChatId,
            text: settingsText,
            parseMode: ParseMode.MarkdownV2,
            cancellationToken: cancellationToken);

    }
}