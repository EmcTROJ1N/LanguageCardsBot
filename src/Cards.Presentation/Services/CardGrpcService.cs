using Cards.Domain.Entities;
using Cards.Domain.ValueObjects;
using Cards.Infrastructure.Interfaces;
using Grpc.Core;
using LanguageCardsBot.Contracts.Cards.V3;

namespace Cards.Presentation.Services;

public sealed class CardGrpcService(ICardRepository cardRepository, IReviewRepository reviewRepository) : CardService.CardServiceBase
{
    public override async Task<GetCardResponse> GetById(GetCardByIdRequest request, ServerCallContext context)
    {
        var card = await cardRepository.GetByIdAsync(request.Id, context.CancellationToken);
        return card is null
            ? new GetCardResponse()
            : new GetCardResponse { Card = card.ToGrpcCard() };
    }

    public override async Task<GetAllCardsResponse> GetAll(GetAllCardsRequest request, ServerCallContext context)
    {
        var cards = await cardRepository.GetAllAsync(context.CancellationToken);
        var response = new GetAllCardsResponse();
        response.Cards.AddRange(cards.Select(x => x.ToGrpcCard()));
        return response;
    }

    public override async Task<GetCardsByUserIdResponse> GetByUserId(GetCardsByUserIdRequest request, ServerCallContext context)
    {
        var cards = await cardRepository.GetAllByUserIdAsync(request.UserId, context.CancellationToken);
        var response = new GetCardsByUserIdResponse();
        response.Cards.AddRange(cards.Select(x => x.ToGrpcCard()));
        return response;
    }

    public override async Task<GetDueCardResponse> GetDueCard(GetDueCardRequest request, ServerCallContext context)
    {
        var card = await cardRepository.GetDueCardAsync(request.UserId, context.CancellationToken);
        return card is null
            ? new GetDueCardResponse()
            : new GetDueCardResponse { Card = card.ToGrpcCard() };
    }

    public override async Task<CardResponse> Add(AddCardRequest request, ServerCallContext context)
    {
        var now = DateTime.UtcNow;
        var firstInterval = ReviewIntervals.GetIntervalDays(1);
        var nextReview = now.AddDays(firstInterval);

        var existingCard = (await cardRepository.GetAllByUserIdAsync(request.UserId, context.CancellationToken))
            .FirstOrDefault(c => c.Term.Equals(request.Term, StringComparison.InvariantCultureIgnoreCase));
        if (existingCard is not null)
            return new CardResponse { Card = existingCard.ToGrpcCard() };
        
        var card = new CardEntity
        {
            UserId = request.UserId,
            Term = request.Term.Trim(),
            Translation = request.Translation.Trim(),
            Transcription = request.Transcription.Trim(),
            Example = string.IsNullOrWhiteSpace(request.Example) ? null : request.Example.Trim(),
            Level = 1,
            NextReviewAt = nextReview,
            CreatedAt = now,
            Learned = false,
        };

        var created = await cardRepository.AddAsync(card, context.CancellationToken);
        return new CardResponse { Card = created.ToGrpcCard() };
    }

    public override async Task<UpdateCardResponse> Update(UpdateCardRequest request, ServerCallContext context)
    {
        var card = await cardRepository.GetByIdAsync(request.Id, context.CancellationToken);
        if (card is null)
            return new UpdateCardResponse { Updated = false };

        card.Term = request.Term.Trim();
        card.Translation = request.Translation.Trim();
        card.Transcription = request.Transcription.Trim();

        if (request.HasExample)
            card.Example = string.IsNullOrWhiteSpace(request.Example) ? null : request.Example.Trim();

        if (request.HasLearned)
        {
            card.Learned = request.Learned;
            if (request.Learned)
                card.NextReviewAt = null;
            else if (card.NextReviewAt is null)
                card.NextReviewAt = DateTime.UtcNow;
        }

        await cardRepository.UpdateAsync(card, context.CancellationToken);
        return new UpdateCardResponse { Updated = true };
    }

    public override async Task<UpdateCardReviewResponse> UpdateCardReview(UpdateCardReviewRequest request, ServerCallContext context)
    {
        var card = await cardRepository.GetByIdAsync(request.CardId, context.CancellationToken);
        if (card is null)
            return new UpdateCardReviewResponse { Updated = false };

        var review = card.RecordReview(request.IsCorrect, DateTime.UtcNow);
        await reviewRepository.AddAsync(review, context.CancellationToken);

        return new UpdateCardReviewResponse { Updated = true };
    }

    public override async Task<DeleteCardResponse> DeleteById(DeleteCardByIdRequest request, ServerCallContext context)
    {
        var card = await cardRepository.GetByIdAsync(request.Id, context.CancellationToken);
        if (card is null)
            return new DeleteCardResponse { Deleted = false };

        await cardRepository.DeleteAsync(request.Id, context.CancellationToken);
        return new DeleteCardResponse { Deleted = true };
    }

    public override async Task<DeleteCardsByUserIdResponse> DeleteByUserId(DeleteCardsByUserIdRequest request, ServerCallContext context)
    {
        var deleted = await cardRepository.DeleteAllByUserIdAsync(request.UserId, context.CancellationToken);
        return new DeleteCardsByUserIdResponse { Deleted = deleted > 0 };
    }
}
