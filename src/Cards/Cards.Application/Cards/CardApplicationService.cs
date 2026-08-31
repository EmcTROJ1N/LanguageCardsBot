using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Cards.Application.Abstractions.Metrics;
using Cards.Application.Abstractions.Repositories;
using Cards.Domain.Entities;
using Cards.Domain.ValueObjects;

namespace Cards.Application.Cards;

/// <summary>
/// Implements card use cases shared by gRPC and REST transports.
/// </summary>
public sealed class CardApplicationService(
    ICardRepository cardRepository,
    IReviewRepository reviewRepository,
    ICardMetrics cardMetrics) : ICardApplicationService
{
    /// <inheritdoc />
    public Task<CardEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return cardRepository.GetByIdAsync(id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<CardEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return (await cardRepository.GetAllAsync(cancellationToken)).ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<CardEntity>> GetByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return (await cardRepository.GetAllByUserIdAsync(userId, cancellationToken)).ToList();
    }

    /// <inheritdoc />
    public Task<CardEntity?> GetDueCardAsync(int userId, CancellationToken cancellationToken = default)
    {
        return cardRepository.GetDueCardAsync(userId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CardEntity> AddAsync(AddCardCommand command, CancellationToken cancellationToken = default)
    {
        var term = (command.Term ?? string.Empty).Trim();
        var translation = (command.Translation ?? string.Empty).Trim();
        var transcription = (command.Transcription ?? string.Empty).Trim();
        var example = string.IsNullOrWhiteSpace(command.Example) ? null : command.Example.Trim();

        var existingCard = (await cardRepository.GetAllByUserIdAsync(command.UserId, cancellationToken))
            .FirstOrDefault(c => c.Term.Equals(term, StringComparison.InvariantCultureIgnoreCase));
        if (existingCard is not null)
            return existingCard;

        var now = DateTime.UtcNow;
        var firstInterval = ReviewIntervals.GetIntervalDays(1);
        var card = new CardEntity
        {
            UserId = command.UserId,
            Term = term,
            Translation = translation,
            Transcription = transcription,
            Example = example,
            Level = 1,
            NextReviewAt = now.AddDays(firstInterval),
            CreatedAt = now,
            Learned = false
        };

        var added = await cardRepository.AddAsync(card, cancellationToken);
        cardMetrics.IncrementCardsCreatedTotal();
        return added;
    }

    /// <inheritdoc />
    public async Task<bool> UpdateAsync(UpdateCardCommand command, CancellationToken cancellationToken = default)
    {
        var card = await cardRepository.GetByIdAsync(command.Id, cancellationToken);
        if (card is null)
            return false;

        card.Term = (command.Term ?? string.Empty).Trim();
        card.Translation = (command.Translation ?? string.Empty).Trim();
        card.Transcription = (command.Transcription ?? string.Empty).Trim();

        if (command.HasExample)
            card.Example = string.IsNullOrWhiteSpace(command.Example) ? null : command.Example.Trim();

        if (command.Learned.HasValue)
        {
            card.Learned = command.Learned.Value;
            if (card.Learned)
                card.NextReviewAt = null;
            else if (card.NextReviewAt is null)
                card.NextReviewAt = DateTime.UtcNow;
        }

        await cardRepository.UpdateAsync(card, cancellationToken);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> UpdateReviewAsync(
        int cardId,
        bool isCorrect,
        CancellationToken cancellationToken = default)
    {
        var card = await cardRepository.GetByIdAsync(cardId, cancellationToken);
        if (card is null)
            return false;

        var wasLearned = card.Learned;
        var preLevel = card.Level;
        var review = card.RecordReview(isCorrect, DateTime.UtcNow);
        await reviewRepository.AddAsync(review, cancellationToken);

        cardMetrics.IncrementCardsReviewsTotal(isCorrect, preLevel);

        if (!wasLearned && card.Learned)
        {
            cardMetrics.IncrementCardsLearnedTotal();
            cardMetrics.RecordCardTimeToLearn(card.CreatedAt, review.ReviewedAt);
        }

        if (!isCorrect && preLevel > 1)
        {
            cardMetrics.IncrementCardsLevelResetTotal();
            cardMetrics.RecordCardReviewStreak(preLevel - 1);
        }

        return true;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var card = await cardRepository.GetByIdAsync(id, cancellationToken);
        if (card is null)
            return false;

        await cardRepository.DeleteAsync(id, cancellationToken);
        cardMetrics.IncrementCardsDeletedTotal(1, "single");
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var deleted = await cardRepository.DeleteAllByUserIdAsync(userId, cancellationToken);
        if (deleted > 0)
            cardMetrics.IncrementCardsDeletedTotal(deleted, "bulk_user");
        return deleted > 0;
    }

    /// <inheritdoc />
    public async Task<CardExportResult> ExportAsync(
        int userId,
        CardExportFormat format,
        CancellationToken cancellationToken = default)
    {
        var cards = (await cardRepository.GetAllByUserIdAsync(userId, cancellationToken)).ToList();

        if (format == CardExportFormat.Csv)
        {
            var bytes = Encoding.UTF8.GetBytes(BuildCsv(cards));
            return new CardExportResult(bytes, "text/csv", "cards.csv");
        }

        var json = JsonSerializer.SerializeToUtf8Bytes(
            cards.Select(c => new
            {
                c.Term, c.Translation, c.Transcription, c.Example,
                c.Level, c.Learned, createdAt = c.CreatedAt.ToString("yyyy-MM-dd"),
            }),
            new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            });
        return new CardExportResult(json, "application/json", "cards.json");
    }

    private static string BuildCsv(IReadOnlyCollection<CardEntity> cards)
    {
        var sb = new StringBuilder();
        sb.AppendLine("term,translation,transcription,example,level,learned,createdAt");
        foreach (var c in cards)
            sb.AppendLine($"{CsvField(c.Term)},{CsvField(c.Translation)},{CsvField(c.Transcription)},{CsvField(c.Example)},{c.Level},{c.Learned.ToString().ToLower()},{c.CreatedAt:yyyy-MM-dd}");
        return sb.ToString();
    }

    private static string CsvField(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value.AsSpan().ContainsAny(',', '"', '\n')
            ? $"\"{value.Replace("\"", "\"\"")}\""
            : value;
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyCollection<CardEntity> Cards, (int All, int Due, int New, int Learned) Counts)> SearchAsync(
        int userId,
        CardSearchQuery query,
        CancellationToken cancellationToken = default)
    {
        var cardsTask  = cardRepository.SearchAsync(userId, query, cancellationToken);
        var countsTask = cardRepository.CountByStatusAsync(userId, cancellationToken);
        await Task.WhenAll(cardsTask, countsTask);
        return (cardsTask.Result, countsTask.Result);
    }
}
