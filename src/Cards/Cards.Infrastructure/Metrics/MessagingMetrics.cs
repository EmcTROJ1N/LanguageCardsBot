using System.Diagnostics.Metrics;
using Cards.Application.Abstractions.Metrics;

namespace Cards.Infrastructure.Metrics;

public class MessagingMetrics : IMessagingMetrics
{
    private readonly Counter<long> _publishTotal;
    private readonly Histogram<double> _publishDuration;

    public MessagingMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("LanguageCardsBot.Messaging", "1.0.0");

        _publishTotal = meter.CreateCounter<long>(
            name: "messaging.publish.total",
            unit: "operations",
            description: "Количество попыток публикации сообщений в RabbitMQ");

        _publishDuration = meter.CreateHistogram<double>(
            name: "messaging.publish.duration",
            unit: "s",
            description: "Длительность публикации (включая publisher confirm), сек");
    }

    public void RecordPublish(string routingKey, string outcome, double durationSeconds)
    {
        var routingKeyTag = new KeyValuePair<string, object?>("routing_key", routingKey);
        var outcomeTag = new KeyValuePair<string, object?>("outcome", outcome);

        _publishTotal.Add(1, routingKeyTag, outcomeTag);
        _publishDuration.Record(durationSeconds, routingKeyTag);
    }
}
