using Cards.Application.Cards;
using Grpc.Core;
using LanguageCardsBot.Contracts.Cards.V3;

namespace Cards.Presentation.Services;

/// <summary>
/// Adapts card gRPC requests to shared card application use cases.
/// </summary>
public sealed class CardGrpcService(ICardApplicationService cardApplicationService) : CardService.CardServiceBase
{
    /// <summary>
    /// Handles a gRPC request to get a card by identifier.
    /// </summary>
    public override async Task<GetCardResponse> GetById(GetCardByIdRequest request, ServerCallContext context)
    {
        var card = await cardApplicationService.GetByIdAsync(request.Id, context.CancellationToken);
        return card is null
            ? new GetCardResponse()
            : new GetCardResponse { Card = card.ToGrpcCard() };
    }

    /// <summary>
    /// Handles a gRPC request to get all cards.
    /// </summary>
    public override async Task<GetAllCardsResponse> GetAll(GetAllCardsRequest request, ServerCallContext context)
    {
        var cards = await cardApplicationService.GetAllAsync(context.CancellationToken);
        var response = new GetAllCardsResponse();
        response.Cards.AddRange(cards.Select(x => x.ToGrpcCard()));
        return response;
    }

    /// <summary>
    /// Handles a gRPC request to get cards by user identifier.
    /// </summary>
    public override async Task<GetCardsByUserIdResponse> GetByUserId(GetCardsByUserIdRequest request, ServerCallContext context)
    {
        var cards = await cardApplicationService.GetByUserIdAsync(request.UserId, context.CancellationToken);
        var response = new GetCardsByUserIdResponse();
        response.Cards.AddRange(cards.Select(x => x.ToGrpcCard()));
        return response;
    }

    /// <summary>
    /// Handles a gRPC request to get the next due card for a user.
    /// </summary>
    public override async Task<GetDueCardResponse> GetDueCard(GetDueCardRequest request, ServerCallContext context)
    {
        var card = await cardApplicationService.GetDueCardAsync(request.UserId, context.CancellationToken);
        return card is null
            ? new GetDueCardResponse()
            : new GetDueCardResponse { Card = card.ToGrpcCard() };
    }

    /// <summary>
    /// Handles a gRPC request to add a card.
    /// </summary>
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

    /// <summary>
    /// Handles a gRPC request to update a card.
    /// </summary>
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

    /// <summary>
    /// Handles a gRPC request to record a card review result.
    /// </summary>
    public override async Task<UpdateCardReviewResponse> UpdateCardReview(UpdateCardReviewRequest request, ServerCallContext context)
    {
        var updated = await cardApplicationService.UpdateReviewAsync(
            request.CardId,
            request.IsCorrect,
            context.CancellationToken);

        return new UpdateCardReviewResponse { Updated = updated };
    }

    /// <summary>
    /// Handles a gRPC request to delete a card by identifier.
    /// </summary>
    public override async Task<DeleteCardResponse> DeleteById(DeleteCardByIdRequest request, ServerCallContext context)
    {
        var deleted = await cardApplicationService.DeleteByIdAsync(request.Id, context.CancellationToken);
        return new DeleteCardResponse { Deleted = deleted };
    }

    /// <summary>
    /// Handles a gRPC request to delete cards by user identifier.
    /// </summary>
    public override async Task<DeleteCardsByUserIdResponse> DeleteByUserId(DeleteCardsByUserIdRequest request, ServerCallContext context)
    {
        var deleted = await cardApplicationService.DeleteByUserIdAsync(request.UserId, context.CancellationToken);
        return new DeleteCardsByUserIdResponse { Deleted = deleted };
    }
}
