using EnglishCardsBot.Application.Interfaces;
using EnglishCardsBot.Application.Services;
using EnglishCardsBot.Presentation.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace EnglishCardsBot.Presentation.Workers;

public class ReminderWorker(
    IServiceProvider serviceProvider,
    ILogger<ReminderWorker> logger,
    IConfiguration configuration)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();

                var cardRepository = scope.ServiceProvider.GetRequiredService<ICardRepository>();
                var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();
                var statsService = scope.ServiceProvider.GetRequiredService<StatsService>();
                var userService = scope.ServiceProvider.GetRequiredService<UserService>();

                var users = await userService.GetAllAsync(stoppingToken);

                foreach (var user in users)
                {
                    try
                    {
                        await ProcessRandomRemindersAsync(
                            user,
                            cardRepository,
                            botClient,
                            userService,
                            stoppingToken);

                        await ProcessDailySummaryAsync(
                            user,
                            statsService,
                            botClient,
                            stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error processing user {UserId}", user.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in ReminderWorker");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task ProcessRandomRemindersAsync(
        Domain.Entities.User user,
        ICardRepository cardRepository,
        ITelegramBotClient botClient,
        UserService userService,
        CancellationToken cancellationToken)
    {
        var nowUtc = DateTime.UtcNow;

        // Если не инициализировано — планируем первый запуск и выходим
        if (user.NextReminderAtUtc is null)
        {
            var next = nowUtc.AddMinutes(Math.Max(1, user.ReminderIntervalMinutes));
            await userService.UpdateNextReminderAtUtcAsync(user.Id, next, cancellationToken);
            var newuser = await userService.GetByIdAsync(user.Id, cancellationToken);
            
            user.NextReminderAtUtc = next;
            return;
        }

        // Рано — ничего не делаем
        if (nowUtc < user.NextReminderAtUtc.Value)
            return;

        var card = await cardRepository.GetRandomActiveCardAsync(user.Id, cancellationToken);
        if (card != null)
        {
            var text = user.HideTranslations
                ? $"{card.Term} — ||{card.Translation}||"
                : $"{card.Term} — {card.Translation}";

            await botClient.SendMessage(
                chatId: user.ChatId,
                text: text,
                ParseMode.MarkdownV2,
                cancellationToken: cancellationToken);
        }

        var nextReminder = nowUtc.AddMinutes(Math.Max(1, user.ReminderIntervalMinutes));
        await userService.UpdateNextReminderAtUtcAsync(user.Id, nextReminder, cancellationToken);
        user.NextReminderAtUtc = nextReminder;
    }

    private async Task ProcessDailySummaryAsync(
        Domain.Entities.User user,
        StatsService statsService,
        ITelegramBotClient botClient,
        CancellationToken cancellationToken)
    {
        var summaryTime = TimeSpan.Parse(configuration["Bot:DailySummaryTime"] ?? "21:00:00");
        var now = DateTime.UtcNow.TimeOfDay;

        if (Math.Abs((now - summaryTime).TotalMinutes) > 1)
            return;

        var stats = await statsService.GetTodayStatsAsync(user.Id, cancellationToken);

        var message = $"🌙 *Итоги дня*\n\n" +
                     $"Новых слов сегодня: *{stats.NewToday}*\n" +
                     $"Повторений сегодня: *{stats.TotalReviewsToday}* " +
                     $"(правильных: *{stats.CorrectReviewsToday}*)\n\n" +
                     $"Всего карточек: *{stats.TotalCards}*\n" +
                     $"Выучено: *{stats.LearnedCards}*";

        if (!string.IsNullOrEmpty(stats.BestDay))
        {
            message += $"\n\nЛучший день: *{stats.BestDay}* — *{stats.BestCount}* повторений";
        }

        await botClient.SendMessage(
            chatId: user.ChatId,
            text: message,
            parseMode: ParseMode.MarkdownV2,
            cancellationToken: cancellationToken);
    }
}
