using Cards.Application.Abstractions.Metrics;
using Cards.Application.Abstractions.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cards.Infrastructure.Metrics;

/// <summary>
/// Periodically polls DB-backed gauge metrics: due-review backlog and active cards by level.
/// </summary>
public sealed class CardsGaugeMetricsService(
    IServiceScopeFactory scopeFactory,
    ICardMetrics cardMetrics,
    ILogger<CardsGaugeMetricsService> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(30);
    private const int MaxLevel = 10;

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(PollInterval);
        do
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var cardRepository = scope.ServiceProvider.GetRequiredService<ICardRepository>();

                var dueCount = await cardRepository.CountDueAsync(stoppingToken);
                cardMetrics.RecordCardDueBacklog(dueCount);

                var byLevel = await cardRepository.CountActiveByLevelAsync(stoppingToken);
                for (var level = 1; level <= MaxLevel; level++)
                    cardMetrics.RecordCardsActive(level, byLevel.TryGetValue(level, out var count) ? count : 0);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to poll gauge metrics");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
