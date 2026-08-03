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
        var review = card.RecordReview(isCorrect, DateTime.UtcNow);
        await reviewRepository.AddAsync(review, cancellationToken);

        if (!wasLearned && card.Learned)
            cardMetrics.RecordCardTimeToLearn(card.CreatedAt, review.ReviewedAt);

        return true;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var card = await cardRepository.GetByIdAsync(id, cancellationToken);
        if (card is null)
            return false;

        await cardRepository.DeleteAsync(id, cancellationToken);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var deleted = await cardRepository.DeleteAllByUserIdAsync(userId, cancellationToken);
        return deleted > 0;
    }
}
