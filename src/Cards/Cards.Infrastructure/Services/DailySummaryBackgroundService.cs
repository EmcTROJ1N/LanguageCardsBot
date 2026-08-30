using Cards.Application.Messaging;
using Cards.Application.Stats;
using Cards.Application.Users;
using LanguageCardsBot.Contracts.Messaging.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cards.Infrastructure.Services;

/// <summary>
/// Sends a daily statistics summary to every user once per day at the configured time.
/// </summary>
public class DailySummaryBackgroundService(
    IServiceProvider serviceProvider,
    IConfiguration configuration,
    ILogger<DailySummaryBackgroundService> logger)
    : BackgroundService
{
    private DateOnly? _lastSentDate;

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var summaryTime = TimeSpan.Parse(configuration["Bot:DailySummaryTime"] ?? "21:00:00");
                var now = DateTime.UtcNow.TimeOfDay;

                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                if (Math.Abs((now - summaryTime).TotalMinutes) <= 1 && _lastSentDate != today)
                {
                    using var scope = serviceProvider.CreateScope();
                    var userService = scope.ServiceProvider.GetRequiredService<IUserApplicationService>();
                    var statsService = scope.ServiceProvider.GetRequiredService<IStatsApplicationService>();
                    var messageBus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

                    var users = await userService.GetAllAsync(stoppingToken);

                    foreach (var user in users)
                    {
                        if (!user.ChatId.HasValue)
                            continue;

                        try
                        {
                            var stats = await statsService.GetTodayStatsAsync(user.Id, stoppingToken);

                            await messageBus.PublishAsync(
                                new DailySummaryEvent(
                                    user.ChatId.Value,
                                    stats.NewToday,
                                    stats.TotalReviewsToday,
                                    stats.CorrectReviewsToday,
                                    stats.TotalCards,
                                    stats.LearnedCards,
                                    stats.BestDay,
                                    stats.BestCount),
                                routingKey: "daily-summary",
                                ct: stoppingToken);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Error sending daily summary to user {UserId}", user.Id);
                        }
                    }
                    _lastSentDate = today;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in DailySummaryBackgroundService");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
