using Cards.Application.Abstractions.Metrics;
using Cards.Application.Abstractions.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cards.Infrastructure.Metrics;

/// <summary>
/// Periodically polls the count of due-for-review cards and publishes it as a gauge metric.
/// </summary>
public sealed class DueBacklogMetricsService(
    IServiceScopeFactory scopeFactory,
    ICardMetrics cardMetrics,
    ILogger<DueBacklogMetricsService> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(PollInterval);
        do
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var cardRepository = scope.ServiceProvider.GetRequiredService<ICardRepository>();
                var count = await cardRepository.CountDueAsync(stoppingToken);
                cardMetrics.RecordCardDueBacklog(count);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to record due-backlog metric");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
