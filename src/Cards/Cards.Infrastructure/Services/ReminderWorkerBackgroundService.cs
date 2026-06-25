using Cards.Application.Cards;
using Cards.Application.Messaging;
using Cards.Application.Stats;
using Cards.Application.Users;
using Cards.Domain.Entities;
using LanguageCardsBot.Contracts.Messaging.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cards.Infrastructure.Services;

/// <summary>
/// Background worker that sends card reminders and daily summaries via the message bus.
/// </summary>
public class ReminderWorkerBackgroundService(
    IServiceProvider serviceProvider,
    ILogger<ReminderWorkerBackgroundService> logger,
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

                var cardService = scope.ServiceProvider.GetRequiredService<ICardApplicationService>();
                var statsService = scope.ServiceProvider.GetRequiredService<IStatsApplicationService>();
                var userService = scope.ServiceProvider.GetRequiredService<IUserApplicationService>();
                var messageBus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

                var users = await userService.GetAllAsync(stoppingToken);

                foreach (var user in users)
                {
                    try
                    {
                        await ProcessRandomRemindersAsync(user, cardService, userService, messageBus, stoppingToken);
                        await ProcessDailySummaryAsync(user, statsService, messageBus, stoppingToken);
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
        UserEntity user,
        ICardApplicationService cardService,
        IUserApplicationService userService,
        IMessageBus messageBus,
        CancellationToken cancellationToken)
    {
        var nowUtc = DateTime.UtcNow;

        if (user.NextReminderAtUtc is null)
        {
            var next = nowUtc.AddMinutes(Math.Max(1, user.ReminderIntervalMinutes));
            await userService.UpdateNextReminderAtUtcAsync(user.Id, next, cancellationToken);
            user.NextReminderAtUtc = next;
            return;
        }

        if (nowUtc < user.NextReminderAtUtc.Value)
            return;

        var card = await cardService.GetDueCardAsync(user.Id, cancellationToken);
        if (card != null)
        {
            await messageBus.PublishAsync(
                new CardReminderEvent(user.ChatId, card.Id, card.Term, card.Translation, user.HideTranslations),
                "reminder",
                cancellationToken);
        }

        var nextReminder = nowUtc.AddMinutes(Math.Max(1, user.ReminderIntervalMinutes));
        await userService.UpdateNextReminderAtUtcAsync(user.Id, nextReminder, cancellationToken);
        user.NextReminderAtUtc = nextReminder;
    }

    private async Task ProcessDailySummaryAsync(
        UserEntity user,
        IStatsApplicationService statsService,
        IMessageBus messageBus,
        CancellationToken cancellationToken)
    {
        var summaryTime = TimeSpan.Parse(configuration["Bot:DailySummaryTime"] ?? "21:00:00");
        var now = DateTime.UtcNow.TimeOfDay;

        if (Math.Abs((now - summaryTime).TotalMinutes) > 1)
            return;

        var stats = await statsService.GetTodayStatsAsync(user.Id, cancellationToken);

        await messageBus.PublishAsync(
            new DailySummaryEvent(
                user.ChatId,
                stats.NewToday,
                stats.TotalReviewsToday,
                stats.CorrectReviewsToday,
                stats.TotalCards,
                stats.LearnedCards,
                stats.BestDay,
                stats.BestCount),
            "daily-summary",
            cancellationToken);
    }
}