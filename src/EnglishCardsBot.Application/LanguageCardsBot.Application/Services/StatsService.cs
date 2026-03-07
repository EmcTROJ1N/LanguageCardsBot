using EnglishCardsBot.Application.DTOs;
using EnglishCardsBot.Application.Interfaces;
using EnglishCardsBot.Domain.Entities;

namespace EnglishCardsBot.Application.Services;

public class StatsService(ICardRepository cardRepository, IReviewRepository reviewRepository)
{
    public async Task<TodayStatsDto> GetTodayStatsAsync(int userId, CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var todayStart = today;
        var todayEnd = today.AddDays(1);

        var cards = (await cardRepository.GetAllByUserIdAsync(userId, cancellationToken)).ToList();
        
        var newToday = cards.Count(c => c.CreatedAt.Date == today);
        var totalCards = cards.Count;
        var learnedCards = cards.Count(c => c.Learned);

        var (totalReviewsToday, correctReviewsToday) = await reviewRepository.GetTodayStatsByUserIdAsync(userId, cancellationToken);
        var (bestDay, bestCount) = await reviewRepository.GetBestDayStatsByUserIdAsync(userId, cancellationToken);

        return new TodayStatsDto
        {
            NewToday = newToday,
            TotalReviewsToday = totalReviewsToday,
            CorrectReviewsToday = correctReviewsToday,
            TotalCards = totalCards,
            LearnedCards = learnedCards,
            BestDay = bestDay,
            BestCount = bestCount
        };
    }
}

