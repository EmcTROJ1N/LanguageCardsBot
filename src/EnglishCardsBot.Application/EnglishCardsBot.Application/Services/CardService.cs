using EnglishCardsBot.Application.Interfaces;
using EnglishCardsBot.Domain.Entities;
using EnglishCardsBot.Domain.ValueObjects;

namespace EnglishCardsBot.Application.Services;

public class CardService
{
    private readonly ICardRepository _cardRepository;
    private readonly IReviewRepository _reviewRepository;

    public CardService(ICardRepository cardRepository, IReviewRepository reviewRepository)
    {
        _cardRepository = cardRepository;
        _reviewRepository = reviewRepository;
    }

    public async Task<Card> AddCardAsync(int userId, string term, string translation, string transcription, string? example, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var firstInterval = ReviewIntervals.GetIntervalDays(1);
        var nextReview = now.AddDays(firstInterval);

        var existingCard = (await _cardRepository.GetAllByUserIdAsync(userId, cancellationToken))
            .FirstOrDefault(c => c.Term.Equals(term, StringComparison.InvariantCultureIgnoreCase));
        if (existingCard is not null)
            return existingCard;
        
        var card = new Card
        {
            UserId = userId,
            Term = term,
            Translation = translation,
            Transcription = transcription,
            Example = example,
            Level = 1,
            NextReviewAt = nextReview,
            Learned = false,
            CreatedAt = now
        };

        return await _cardRepository.AddAsync(card, cancellationToken);
    }

    public async Task UpdateCardReviewAsync(int cardId, bool isCorrect, CancellationToken cancellationToken = default)
    {
        var card = await _cardRepository.GetByIdAsync(cardId, cancellationToken);
        if (card == null)
            return;

        var now = DateTime.UtcNow;
        card.TotalReviews++;
        if (isCorrect)
            card.CorrectReviews++;

        if (isCorrect)
        {
            card.Level++;
            if (card.Level > 10)
            {
                card.Learned = true;
                card.NextReviewAt = null;
            }
            else
            {
                var intervalDays = ReviewIntervals.GetIntervalDays(card.Level);
                card.NextReviewAt = now.AddDays(intervalDays);
            }
        }
        else
        {
            card.Level = Math.Max(1, card.Level - 1);
            var intervalDays = ReviewIntervals.GetIntervalDays(card.Level);
            card.NextReviewAt = now.AddDays(intervalDays);
            card.Learned = false;
        }

        card.LastReviewAt = now;

        await _cardRepository.UpdateAsync(card, cancellationToken);

        var review = new Review
        {
            CardId = cardId,
            IsCorrect = isCorrect,
            ReviewedAt = now
        };

        await _reviewRepository.AddAsync(review, cancellationToken);
    }
}

