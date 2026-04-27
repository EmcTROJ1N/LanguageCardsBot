using Cards.Infrastructure.Interfaces;
using Grpc.Core;
using LanguageCardsBot.Contracts.Cards.V3;

namespace Cards.Presentation.Services;

public sealed class StatsGrpcService(ICardRepository cardRepository, IReviewRepository reviewRepository) : StatsService.StatsServiceBase
{
    public override async Task<GetTodayStatsResponse> GetTodayStats(GetTodayStatsRequest request,
        ServerCallContext context)
    {
        var today = DateTime.UtcNow.Date;
        var todayStart = today;
        var todayEnd = today.AddDays(1);

        var cards = (await cardRepository.GetAllByUserIdAsync(request.UserId, context.CancellationToken)).ToList();

        var newToday = cards.Count(c => c.CreatedAt.Date == today);
        var totalCards = cards.Count;
        var learnedCards = cards.Count(c => c.Learned);

        var (totalReviewsToday, correctReviewsToday) =
            await reviewRepository.GetTodayStatsByUserIdAsync(request.UserId, context.CancellationToken);
        var (bestDay, bestCount) = await reviewRepository.GetBestDayStatsByUserIdAsync(request.UserId, context.CancellationToken);

        return new GetTodayStatsResponse
        {
            Stats = new TodayStats
            {
                NewToday = newToday,
                TotalReviewsToday = totalReviewsToday,
                CorrectReviewsToday = correctReviewsToday,
                TotalCards = totalCards,
                LearnedCards = learnedCards,
                BestDay = bestDay,
                BestCount = bestCount
            }
        };
    }
}
