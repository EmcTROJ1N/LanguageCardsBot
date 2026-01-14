namespace EnglishCardsBot.Application.DTOs;

public class TodayStatsDto
{
    public int NewToday { get; set; }
    public int TotalReviewsToday { get; set; }
    public int CorrectReviewsToday { get; set; }
    public int TotalCards { get; set; }
    public int LearnedCards { get; set; }
    public string? BestDay { get; set; }
    public int BestCount { get; set; }
}

