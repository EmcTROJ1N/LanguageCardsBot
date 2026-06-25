namespace LanguageCardsBot.Contracts.Messaging.Events;

/// <summary>
/// Published by the Cards microservice at the configured daily summary time.
/// The consumer is responsible for formatting and delivering the Telegram message.
/// </summary>
public sealed record DailySummaryEvent(
    long ChatId,
    int NewToday,
    int TotalReviewsToday,
    int CorrectReviewsToday,
    int TotalCards,
    int LearnedCards,
    string? BestDay,
    int BestCount);
