namespace Cards.Application.Stats;

/// <summary>
/// Represents the review and card statistics calculated for the current UTC day.
/// </summary>
public sealed record TodayStatsResult(
    int NewToday,
    int TotalReviewsToday,
    int CorrectReviewsToday,
    int TotalCards,
    int LearnedCards,
    string? BestDay,
    int BestCount);
