using System.Diagnostics.Metrics;
using Cards.Application.Abstractions.Metrics;

namespace Cards.Infrastructure.Metrics;

public class CardMetrics : ICardMetrics
{
    private readonly Counter<long> _cardsCreatedTotal;
    private readonly Gauge<long> _cardsDueBacklog;
    private readonly Histogram<double> _cardsTimeToLearnDays;

    public CardMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("LanguageCardsBot.Cards", "1.0.0");

        _cardsCreatedTotal = meter.CreateCounter<long>(
            name: "cards.created.total",
            unit: "cards",
            description: "Количество успешно созданных карточек");

        _cardsDueBacklog = meter.CreateGauge<long>(
            name: "cards.due.backlog",
            unit: "card",
            description: "Количество карточек, ожидающих повторения (backlog)");

        _cardsTimeToLearnDays = meter.CreateHistogram<double>(
            name: "cards.time_to_learn.days",
            unit: "day",
            description: "Время от CreatedAt до достижения Level 10, в днях");
    }

    public void IncrementCardsCreatedTotal() =>
        _cardsCreatedTotal.Add(1);

    public void RecordCardDueBacklog(int cardsDueBacklogCount) =>
        _cardsDueBacklog.Record(cardsDueBacklogCount);

    public void RecordCardTimeToLearn(DateTime createdAt, DateTime reachedLevel10At) =>
        _cardsTimeToLearnDays.Record((reachedLevel10At - createdAt).TotalDays);
}