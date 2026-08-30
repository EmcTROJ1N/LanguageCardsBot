using System.Collections.Concurrent;
using Cards.Application.Abstractions.Repositories;
using Cards.Application.Messaging;
using Cards.Application.Reminders;
using Cards.Domain.Entities;
using LanguageCardsBot.Contracts.Messaging.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cards.Infrastructure.Services;

/// <summary>
/// Manages one async reminder loop per user. Registered as a singleton.
/// Each loop sleeps via Task.Delay — no thread is held while waiting.
/// </summary>
public sealed class CardReminderOrchestrator(
    IServiceProvider serviceProvider,
    ILogger<CardReminderOrchestrator> logger)
    : ICardReminderOrchestrator, IDisposable
{
    private readonly ConcurrentDictionary<int, CancellationTokenSource> _loops = new();

    /// <inheritdoc />
    public void RegisterUser(UserEntity user)
    {
        if (_loops.ContainsKey(user.Id))
            return;

        var cts = new CancellationTokenSource();

        if (!_loops.TryAdd(user.Id, cts))
        {
            cts.Dispose();
            return;
        }

        var initialNextReminder = user.NextReminderAtUtc;
        var initialInterval     = user.ReminderIntervalMinutes;

        _ = Task.Run(() => UserLoopAsync(user.Id, initialNextReminder, initialInterval, cts.Token));
    }

    /// <inheritdoc />
    public void UnregisterUser(int userId)
    {
        if (_loops.TryRemove(userId, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        foreach (var cts in _loops.Values)
        {
            cts.Cancel();
            cts.Dispose();
        }
        _loops.Clear();
    }

    private async Task UserLoopAsync(
        int userId,
        DateTime? initialNextReminderAt,
        int initialIntervalMinutes,
        CancellationToken ct)
    {
        var delay = initialNextReminderAt.HasValue
            ? TimeSpan.FromTicks(Math.Max(0, (initialNextReminderAt.Value - DateTime.UtcNow).Ticks))
            : TimeSpan.FromMinutes(Math.Max(1, initialIntervalMinutes));

        while (!ct.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(delay, ct);

                using var scope = serviceProvider.CreateScope();
                var userRepo   = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                var cardRepo   = scope.ServiceProvider.GetRequiredService<ICardRepository>();
                var messageBus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

                var user = await userRepo.GetByIdAsync(userId, ct);
                if (user is null)
                {
                    UnregisterUser(userId);
                    return;
                }

                var card = await cardRepo.GetRandomActiveCardAsync(user.Id, ct);
                if (card is not null && user.ChatId.HasValue)
                {
                    await messageBus.PublishAsync(
                        new CardReminderEvent(user.ChatId.Value, card.Id, card.Term, card.Translation, user.HideTranslations),
                        routingKey: "reminder",
                        ct: ct);
                }

                var nextTime = user.ScheduleNextReminder(DateTime.UtcNow);
                await userRepo.UpdateAsync(user, ct);

                delay = TimeSpan.FromMinutes(Math.Max(1, user.ReminderIntervalMinutes));

                logger.LogDebug(
                    "Scheduled next reminder for user {UserId} at {NextTime}",
                    userId,
                    nextTime);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Reminder loop error for user {UserId} — retrying in 1 minute", userId);
                delay = TimeSpan.FromMinutes(1);
            }
        }
    }
}
