using Cards.Application.Abstractions.Repositories;

namespace Cards.Application.Stats;

public sealed class StatsApplicationService(
    ICardRepository cardRepository,
    IReviewRepository reviewRepository) : IStatsApplicationService
{
    public async Task<TodayStatsResult> GetTodayStatsAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var cards = (await cardRepository.GetAllByUserIdAsync(userId, cancellationToken)).ToList();

        var newToday = cards.Count(c => c.CreatedAt.Date == today);
        var totalCards = cards.Count;
        var learnedCards = cards.Count(c => c.Learned);
        var (totalReviewsToday, correctReviewsToday) =
            await reviewRepository.GetTodayStatsByUserIdAsync(userId, cancellationToken);
        var (bestDay, bestCount) =
            await reviewRepository.GetBestDayStatsByUserIdAsync(userId, cancellationToken);

        return new TodayStatsResult(
            newToday,
            totalReviewsToday,
            correctReviewsToday,
            totalCards,
            learnedCards,
            bestDay,
            bestCount);
    }
}
