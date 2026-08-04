namespace Cards.Application.Abstractions.Metrics;

public interface IMessagingMetrics
{
    void RecordPublish(string routingKey, string outcome, double durationSeconds);
}
