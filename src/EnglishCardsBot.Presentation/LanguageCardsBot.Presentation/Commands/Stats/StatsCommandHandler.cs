using EnglishCardsBot.Presentation.Abstractions;
using LanguageCardsBot.Contracts.Cards.V3;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace EnglishCardsBot.Presentation.Commands.Stats;

public class StatsCommandHandler(ITelegramBotClient botClient, StatsService.StatsServiceClient statsService): ICommandHandler<StatCommand>
{
    public async Task HandleAsync(StatCommand command, User user, CancellationToken cancellationToken = default)
    {
        var response = await statsService.GetTodayStatsAsync(new GetTodayStatsRequest
        {
           UserId =  user.Id
        }, cancellationToken: cancellationToken);

        var msg = $"📊 *Статистика*\n\n" +
                  $"Сегодня добавлено новых слов: *{response.Stats.NewToday}*\n" +
                  $"Сегодня повторений: *{response.Stats.TotalReviewsToday}* " +
                  $"(из них правильных: *{response.Stats.CorrectReviewsToday}*)\n\n" +
                  $"Всего карточек: *{response.Stats.TotalCards}*\n" +
                  $"Из них выучено: *{response.Stats.LearnedCards}*";

        if (!string.IsNullOrEmpty(response.Stats.BestDay))
        {
            msg += $"\n\nЛучший день по повторениям: *{response.Stats.BestDay}* — *{response.Stats.BestCount}* повторений";
        }

        await botClient.SendMessage(
            chatId: command.ChatId,
            text: msg,
            parseMode: ParseMode.MarkdownV2,
            cancellationToken: cancellationToken);
    }
}