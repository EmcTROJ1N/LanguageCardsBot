using EnglishCardsBot.Presentation.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EnglishCardsBot.Presentation;

public class Worker(IServiceProvider serviceProvider, ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);

        using var scope = serviceProvider.CreateScope();
        var botService = scope.ServiceProvider.GetRequiredService<TelegramBotService>();
        
        logger.LogInformation("Initializing Telegram Bot...");
        await botService.InitializeAsync(stoppingToken);
        logger.LogInformation("Telegram Bot initialized");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
