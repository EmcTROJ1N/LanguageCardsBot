using Cards.Application.Stats;

namespace Cards.Presentation.Contracts;

public sealed record TodayStatsDto(
    int NewToday,
    int TotalReviewsToday,
    int CorrectReviewsToday,
    int TotalCards,
    int LearnedCards,
    string? BestDay,
    int BestCount);

public sealed record GetTodayStatsResponseDto(TodayStatsDto Stats);

internal static partial class ApiMappingExtensions
{
    public static TodayStatsDto ToDto(this TodayStatsResult result)
    {
        return new TodayStatsDto(
            result.NewToday,
            result.TotalReviewsToday,
            result.CorrectReviewsToday,
            result.TotalCards,
            result.LearnedCards,
            result.BestDay,
            result.BestCount);
    }
}
