using Cards.Application.Abstractions.Repositories;
using Cards.Application.Reminders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cards.Infrastructure.Services;

/// <summary>
/// One-shot hosted service that starts a reminder loop for every existing user at application startup.
/// Returns immediately — the loops run in background Tasks.
/// </summary>
public class CardReminderStartupService(
    IServiceProvider serviceProvider,
    ICardReminderOrchestrator orchestrator,
    ILogger<CardReminderStartupService> logger)
    : IHostedService
{
    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var users = (await userRepo.GetAllAsync(cancellationToken)).ToList();

            foreach (var user in users)
                orchestrator.RegisterUser(user);

            logger.LogInformation(
                "CardReminderStartupService: started reminder loops for {Count} users", users.Count);
        }
        catch (Exception ex)
        {
            logger.LogCritical(
                ex,
                "CardReminderStartupService: failed to load users — reminder loops not started. gRPC/REST remain available.");
        }
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
