namespace Cards.Application.Abstractions.Metrics;

/// <summary>
/// Business metrics for card lifecycle: creation, reviews, learning progress, deletion, and level resets.
/// </summary>
public interface ICardMetrics
{
    /// <summary>Increments the counter of successfully created cards.</summary>
    void IncrementCardsCreatedTotal();
    /// <summary>Records the current number of cards awaiting review (due backlog gauge).</summary>
    void RecordCardDueBacklog(int cardsDueBacklogCount);
    /// <summary>Records the time (in days) between card creation and reaching the "learned" state (Level 10).</summary>
    void RecordCardTimeToLearn(DateTime createdAt, DateTime reachedLevel10At);

    /// <summary>Increments the counter of cards that reached the learned state (Level 10).</summary>
    void IncrementCardsLearnedTotal();
    /// <summary>Increments the counter of deleted cards, tagged with the deletion scope (e.g. "single", "all").</summary>
    void IncrementCardsDeletedTotal(int count, string scope);
    /// <summary>Records the number of active (unlearned) cards at a specific level.</summary>
    void RecordCardsActive(int level, int count);
    /// <summary>Increments the review counter tagged with correctness and the card's level bucket before the review.</summary>
    void IncrementCardsReviewsTotal(bool isCorrect, int levelBeforeReview);
    /// <summary>Increments the counter of card-level resets caused by an incorrect answer.</summary>
    void IncrementCardsLevelResetTotal();
    /// <summary>Records the length of a completed correct-review streak (ended by the first incorrect answer).</summary>
    void RecordCardReviewStreak(int streakLength);
}
