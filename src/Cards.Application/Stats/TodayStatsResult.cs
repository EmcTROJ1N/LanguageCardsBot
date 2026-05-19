namespace Cards.Application.Stats;

public sealed record TodayStatsResult(
    int NewToday,
    int TotalReviewsToday,
    int CorrectReviewsToday,
    int TotalCards,
    int LearnedCards,
    string? BestDay,
    int BestCount);
