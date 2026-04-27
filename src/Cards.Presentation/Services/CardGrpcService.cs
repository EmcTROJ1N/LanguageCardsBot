using Cards.Domain.Entities;
using Cards.Domain.ValueObjects;
using Cards.Infrastructure.Interfaces;
using Grpc.Core;
using LanguageCardsBot.Contracts.Cards.V3;
using Mapster;

namespace Cards.Presentation.Services;

public sealed class CardGrpcService(ICardRepository cardRepository) : CardService.CardServiceBase
{
    public override async Task<CardResponse> Add(AddCardRequest request, ServerCallContext context)
    {
        var now = DateTime.UtcNow;
        var firstInterval = ReviewIntervals.GetIntervalDays(1);
        var nextReview = now.AddDays(firstInterval);

        var existingCard = (await cardRepository.GetAllByUserIdAsync(request.UserId, context.CancellationToken))
            .FirstOrDefault(c => c.Term.Equals(request.Term, StringComparison.InvariantCultureIgnoreCase));
        if (existingCard is not null)
            return new CardResponse { Card = existingCard.Adapt<Card>() };
        
        var card = new CardEntity
        {
            UserId = request.UserId,
            Term = request.Term,
            Translation = request.Translation,
            Transcription = request.Transcription,
            Example = request.Example,
            Level = 1,
            NextReviewAt = nextReview,
            CreatedAt = now,
            Learned = false,
        };

        var created = await cardRepository.AddAsync(card, context.CancellationToken);
        return new CardResponse { Card = created.Adapt<Card>() };
    }

    public override Task<UpdateCardReviewResponse> UpdateCardReview(UpdateCardReviewRequest request, ServerCallContext context)
    {
        throw GrpcServiceBase.CreateUnimplementedException(nameof(UpdateCardReview));
    }
}
