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
