using System.Diagnostics.Metrics;
using Cards.Application.Abstractions.Metrics;

namespace Cards.Infrastructure.Metrics;

/// <summary>
/// Default <see cref="ICardMetrics"/> implementation backed by <see cref="System.Diagnostics.Metrics"/>
/// under the meter <c>LanguageCardsBot.Cards</c>.
/// </summary>
public class CardMetrics : ICardMetrics
{
    private readonly Counter<long> _cardsCreatedTotal;
    private readonly Gauge<long> _cardsDueBacklog;
    private readonly Histogram<double> _cardsTimeToLearnDays;
    private readonly Counter<long> _cardsLearnedTotal;
    private readonly Counter<long> _cardsDeletedTotal;
    private readonly Gauge<long> _cardsActive;
    private readonly Counter<long> _cardsReviewsTotal;
    private readonly Counter<long> _cardsLevelResetTotal;
    private readonly Histogram<long> _cardsReviewStreak;

    /// <summary>Creates the meter and registers all card instruments.</summary>
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

        _cardsLearnedTotal = meter.CreateCounter<long>(
            name: "cards.learned.total",
            unit: "cards",
            description: "Количество карточек, которые достигли Level 10");

        _cardsDeletedTotal = meter.CreateCounter<long>(
            name: "cards.deleted.total",
            unit: "cards",
            description: "Количество удалённых карточек");

        _cardsActive = meter.CreateGauge<long>(
            name: "cards.active",
            unit: "card",
            description: "Количество активных (невыученных) карточек в разбивке по уровню");

        _cardsReviewsTotal = meter.CreateCounter<long>(
            name: "cards.reviews.total",
            unit: "reviews",
            description: "Количество проведённых повторов карточек");

        _cardsLevelResetTotal = meter.CreateCounter<long>(
            name: "cards.level_reset.total",
            unit: "resets",
            description: "Количество откатов уровня карточки к 1 после неверного ответа");

        _cardsReviewStreak = meter.CreateHistogram<long>(
            name: "cards.review_streak",
            unit: "reviews",
            description: "Длина завершённой серии корректных повторов до первой ошибки");
    }

    /// <inheritdoc />
    public void IncrementCardsCreatedTotal() =>
        _cardsCreatedTotal.Add(1);

    /// <inheritdoc />
    public void RecordCardDueBacklog(int cardsDueBacklogCount) =>
        _cardsDueBacklog.Record(cardsDueBacklogCount);

    /// <inheritdoc />
    public void RecordCardTimeToLearn(DateTime createdAt, DateTime reachedLevel10At) =>
        _cardsTimeToLearnDays.Record((reachedLevel10At - createdAt).TotalDays);

    /// <inheritdoc />
    public void IncrementCardsLearnedTotal() =>
        _cardsLearnedTotal.Add(1);

    /// <inheritdoc />
    public void IncrementCardsDeletedTotal(int count, string scope) =>
        _cardsDeletedTotal.Add(count, new KeyValuePair<string, object?>("scope", scope));

    /// <inheritdoc />
    public void RecordCardsActive(int level, int count) =>
        _cardsActive.Record(count, new KeyValuePair<string, object?>("level", level));

    /// <inheritdoc />
    public void IncrementCardsReviewsTotal(bool isCorrect, int levelBeforeReview) =>
        _cardsReviewsTotal.Add(
            1,
            new KeyValuePair<string, object?>("result", isCorrect ? "correct" : "incorrect"),
            new KeyValuePair<string, object?>("level_bucket", LevelBucket(levelBeforeReview)));

    /// <inheritdoc />
    public void IncrementCardsLevelResetTotal() =>
        _cardsLevelResetTotal.Add(1);

    /// <inheritdoc />
    public void RecordCardReviewStreak(int streakLength) =>
        _cardsReviewStreak.Record(streakLength);

    private static string LevelBucket(int level) => level switch
    {
        <= 3 => "1-3",
        <= 6 => "4-6",
        _ => "7-10"
    };
}
