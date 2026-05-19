using Cards.Application.Stats;

namespace Cards.Presentation.Contracts;

/// <summary>
/// Represents today's user statistics returned by the REST API.
/// </summary>
public sealed record TodayStatsDto(
    int NewToday,
    int TotalReviewsToday,
    int CorrectReviewsToday,
    int TotalCards,
    int LearnedCards,
    string? BestDay,
    int BestCount);

/// <summary>
/// Represents a REST response containing today's statistics.
/// </summary>
public sealed record GetTodayStatsResponseDto(TodayStatsDto Stats);

/// <summary>
/// Provides mapping helpers for REST statistics DTOs.
/// </summary>
internal static partial class ApiMappingExtensions
{
    /// <summary>
    /// Converts an application statistics result to a REST DTO.
    /// </summary>
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
