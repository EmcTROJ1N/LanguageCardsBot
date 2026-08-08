namespace Cards.Contracts.Rest.Stats;

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
