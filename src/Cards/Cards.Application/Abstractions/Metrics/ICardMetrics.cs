namespace Cards.Application.Abstractions.Metrics;

public interface ICardMetrics
{
    void IncrementCardsCreatedTotal();
    void RecordCardDueBacklog(int cardsDueBacklogCount);
    void RecordCardTimeToLearn(DateTime createdAt, DateTime reachedLevel10At);
}