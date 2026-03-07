using EnglishCardsBot.Application.Services;
using EnglishCardsBot.Domain.Entities;
using EnglishCardsBot.Presentation.Abstractions;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace EnglishCardsBot.Presentation.Commands.Stats;

public class StatsCommandHandler(ITelegramBotClient botClient, StatsService statsService): ICommandHandler<StatCommand>
{
    public async Task HandleAsync(StatCommand command, User user, CancellationToken cancellationToken = default)
    {
        var stats = await statsService.GetTodayStatsAsync(user.Id, cancellationToken);

        var msg = $"📊 *Статистика*\n\n" +
                  $"Сегодня добавлено новых слов: *{stats.NewToday}*\n" +
                  $"Сегодня повторений: *{stats.TotalReviewsToday}* " +
                  $"(из них правильных: *{stats.CorrectReviewsToday}*)\n\n" +
                  $"Всего карточек: *{stats.TotalCards}*\n" +
                  $"Из них выучено: *{stats.LearnedCards}*";

        if (!string.IsNullOrEmpty(stats.BestDay))
        {
            msg += $"\n\nЛучший день по повторениям: *{stats.BestDay}* — *{stats.BestCount}* повторений";
        }

        await botClient.SendMessage(
            chatId: command.ChatId,
            text: msg,
            parseMode: ParseMode.MarkdownV2,
            cancellationToken: cancellationToken);
    }
}