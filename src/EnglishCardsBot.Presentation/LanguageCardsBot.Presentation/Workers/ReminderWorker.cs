using EnglishCardsBot.Presentation.Services;
using Google.Protobuf.WellKnownTypes;
using LanguageCardsBot.Contracts.Cards.V3;
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

                var cardService = scope.ServiceProvider.GetRequiredService<CardService.CardServiceClient>();
                var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();
                var statsService = scope.ServiceProvider.GetRequiredService<StatsService.StatsServiceClient>();
                var userService = scope.ServiceProvider.GetRequiredService<UserService.UserServiceClient>();

                var response = await userService.GetAllAsync(new GetAllUsersRequest(), cancellationToken: stoppingToken);

                foreach (var user in response.Users)
                {
                    try
                    {
                        await ProcessRandomRemindersAsync(
                            user,
                            cardService,
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
        User user,
        CardService.CardServiceClient cardService,
        ITelegramBotClient botClient,
        UserService.UserServiceClient userService,
        CancellationToken cancellationToken)
    {
        var nowUtc = DateTime.UtcNow;

        // Если не инициализировано — планируем первый запуск и выходим
        if (user.NextReminderAtUtc is null)
        {
            var next = nowUtc.AddMinutes(Math.Max(1, user.ReminderIntervalMinutes));

            await userService.UpdateNextReminderAtUtcAsync(new UpdateNextReminderAtUtcRequest
                { UserId = user.Id, NextReminderAtUtc = next.ToTimestamp() }, cancellationToken: cancellationToken);
            
            var newUser = await userService.GetByIdAsync(new GetUserByIdRequest { Id = user.Id }, cancellationToken: cancellationToken);
            
            user.NextReminderAtUtc = Timestamp.FromDateTime(next);
            return;
        }

        // Рано — ничего не делаем
        if (nowUtc < user.NextReminderAtUtc.ToDateTime())
            return;

        Card card = null;
        // TODO: GetRandomActiveCardAsync
        //var card = await cardService.GetRandomActiveCardAsync(user.Id, cancellationToken);
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
        await userService.UpdateNextReminderAtUtcAsync(new UpdateNextReminderAtUtcRequest()
        {
            UserId = user.Id, 
            NextReminderAtUtc = nextReminder.ToTimestamp()
        }, cancellationToken: cancellationToken);
        user.NextReminderAtUtc = Timestamp.FromDateTime(nextReminder);
    }

    private async Task ProcessDailySummaryAsync(
        User user,
        StatsService.StatsServiceClient statsService,
        ITelegramBotClient botClient,
        CancellationToken cancellationToken)
    {
        var summaryTime = TimeSpan.Parse(configuration["Bot:DailySummaryTime"] ?? "21:00:00");
        var now = DateTime.UtcNow.TimeOfDay;

        if (Math.Abs((now - summaryTime).TotalMinutes) > 1)
            return;

        var response = await statsService.GetTodayStatsAsync(new GetTodayStatsRequest() { UserId = user.Id }, cancellationToken: cancellationToken);

        var message = $"🌙 *Итоги дня*\n\n" +
                     $"Новых слов сегодня: *{response.Stats.NewToday}*\n" +
                     $"Повторений сегодня: *{response.Stats.TotalReviewsToday}* " +
                     $"(правильных: *{response.Stats.CorrectReviewsToday}*)\n\n" +
                     $"Всего карточек: *{response.Stats.TotalCards}*\n" +
                     $"Выучено: *{response.Stats.LearnedCards}*";

        if (!string.IsNullOrEmpty(response.Stats.BestDay))
        {
            message += $"\n\nЛучший день: *{response.Stats.BestDay}* — *{response.Stats.BestCount}* повторений";
        }

        await botClient.SendMessage(
            chatId: user.ChatId,
            text: message,
            parseMode: ParseMode.MarkdownV2,
            cancellationToken: cancellationToken);
    }
}
