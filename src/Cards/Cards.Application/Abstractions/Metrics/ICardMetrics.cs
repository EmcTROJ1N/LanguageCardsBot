namespace Cards.Application.Abstractions.Metrics;

public interface ICardMetrics
{
    void IncrementCardsCreatedTotal();
    void RecordCardDueBacklog(int cardsDueBacklogCount);
    void RecordCardTimeToLearn(DateTime createdAt, DateTime reachedLevel10At);

    void IncrementCardsLearnedTotal();
    void IncrementCardsDeletedTotal(int count, string scope);
    void RecordCardsActive(int level, int count);
    void IncrementCardsReviewsTotal(bool isCorrect, int levelBeforeReview);
    void IncrementCardsLevelResetTotal();
    void RecordCardReviewStreak(int streakLength);
}
