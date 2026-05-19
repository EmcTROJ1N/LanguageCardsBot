using Cards.Application.Cards;
using Grpc.Core;
using LanguageCardsBot.Contracts.Cards.V3;

namespace Cards.Presentation.Services;

public sealed class CardGrpcService(ICardApplicationService cardApplicationService) : CardService.CardServiceBase
{
    public override async Task<GetCardResponse> GetById(GetCardByIdRequest request, ServerCallContext context)
    {
        var card = await cardApplicationService.GetByIdAsync(request.Id, context.CancellationToken);
        return card is null
            ? new GetCardResponse()
            : new GetCardResponse { Card = card.ToGrpcCard() };
    }

    public override async Task<GetAllCardsResponse> GetAll(GetAllCardsRequest request, ServerCallContext context)
    {
        var cards = await cardApplicationService.GetAllAsync(context.CancellationToken);
        var response = new GetAllCardsResponse();
        response.Cards.AddRange(cards.Select(x => x.ToGrpcCard()));
        return response;
    }

    public override async Task<GetCardsByUserIdResponse> GetByUserId(GetCardsByUserIdRequest request, ServerCallContext context)
    {
        var cards = await cardApplicationService.GetByUserIdAsync(request.UserId, context.CancellationToken);
        var response = new GetCardsByUserIdResponse();
        response.Cards.AddRange(cards.Select(x => x.ToGrpcCard()));
        return response;
    }

    public override async Task<GetDueCardResponse> GetDueCard(GetDueCardRequest request, ServerCallContext context)
    {
        var card = await cardApplicationService.GetDueCardAsync(request.UserId, context.CancellationToken);
        return card is null
            ? new GetDueCardResponse()
            : new GetDueCardResponse { Card = card.ToGrpcCard() };
    }

    public override async Task<CardResponse> Add(AddCardRequest request, ServerCallContext context)
    {
        var created = await cardApplicationService.AddAsync(
            new AddCardCommand(
                request.UserId,
                request.Term,
                request.Translation,
                request.Transcription,
                request.HasExample ? request.Example : null),
            context.CancellationToken);

        return new CardResponse { Card = created.ToGrpcCard() };
    }

    public override async Task<UpdateCardResponse> Update(UpdateCardRequest request, ServerCallContext context)
    {
        var updated = await cardApplicationService.UpdateAsync(
            new UpdateCardCommand(
                request.Id,
                request.Term,
                request.Translation,
                request.Transcription,
                request.HasExample,
                request.HasExample ? request.Example : null,
                request.HasLearned ? request.Learned : null),
            context.CancellationToken);

        return new UpdateCardResponse { Updated = updated };
    }

    public override async Task<UpdateCardReviewResponse> UpdateCardReview(UpdateCardReviewRequest request, ServerCallContext context)
    {
        var updated = await cardApplicationService.UpdateReviewAsync(
            request.CardId,
            request.IsCorrect,
            context.CancellationToken);

        return new UpdateCardReviewResponse { Updated = updated };
    }

    public override async Task<DeleteCardResponse> DeleteById(DeleteCardByIdRequest request, ServerCallContext context)
    {
        var deleted = await cardApplicationService.DeleteByIdAsync(request.Id, context.CancellationToken);
        return new DeleteCardResponse { Deleted = deleted };
    }

    public override async Task<DeleteCardsByUserIdResponse> DeleteByUserId(DeleteCardsByUserIdRequest request, ServerCallContext context)
    {
        var deleted = await cardApplicationService.DeleteByUserIdAsync(request.UserId, context.CancellationToken);
        return new DeleteCardsByUserIdResponse { Deleted = deleted };
    }
}
