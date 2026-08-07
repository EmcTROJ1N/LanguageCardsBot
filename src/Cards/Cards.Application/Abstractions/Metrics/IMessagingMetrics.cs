namespace Cards.Application.Abstractions.Metrics;

/// <summary>
/// Metrics for outbound message publishing to the message bus.
/// </summary>
public interface IMessagingMetrics
{
    /// <summary>Records a publish attempt tagged with routing key and outcome, plus its total duration in seconds.</summary>
    void RecordPublish(string routingKey, string outcome, double durationSeconds);
}
