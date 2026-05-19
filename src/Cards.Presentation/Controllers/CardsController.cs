using Cards.Application.Cards;
using Cards.Presentation.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Cards.Presentation.Controllers;

[ApiController]
[Route("api/cards/v3/cards")]
public sealed class CardsController(ICardApplicationService cardApplicationService) : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<ActionResult<GetCardResponseDto>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var card = await cardApplicationService.GetByIdAsync(id, cancellationToken);
        return Ok(new GetCardResponseDto(card?.ToDto()));
    }

    [HttpGet]
    public async Task<ActionResult<GetCardsResponseDto>> GetAll(CancellationToken cancellationToken)
    {
        var cards = await cardApplicationService.GetAllAsync(cancellationToken);
        return Ok(new GetCardsResponseDto(cards.Select(x => x.ToDto()).ToList()));
    }

    [HttpGet("by-user/{userId:int}")]
    public async Task<ActionResult<GetCardsResponseDto>> GetByUserId(
        int userId,
        CancellationToken cancellationToken)
    {
        var cards = await cardApplicationService.GetByUserIdAsync(userId, cancellationToken);
        return Ok(new GetCardsResponseDto(cards.Select(x => x.ToDto()).ToList()));
    }

    [HttpGet("due/{userId:int}")]
    public async Task<ActionResult<GetCardResponseDto>> GetDueCard(
        int userId,
        CancellationToken cancellationToken)
    {
        var card = await cardApplicationService.GetDueCardAsync(userId, cancellationToken);
        return Ok(new GetCardResponseDto(card?.ToDto()));
    }

    [HttpPost]
    public async Task<ActionResult<CardResponseDto>> Add(
        AddCardRequestDto request,
        CancellationToken cancellationToken)
    {
        var card = await cardApplicationService.AddAsync(
            new AddCardCommand(
                request.UserId,
                request.Term,
                request.Translation,
                request.Transcription,
                request.Example),
            cancellationToken);

        return Ok(new CardResponseDto(card.ToDto()));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<UpdateCardResponseDto>> Update(
        int id,
        UpdateCardRequestDto request,
        CancellationToken cancellationToken)
    {
        var updated = await cardApplicationService.UpdateAsync(
            new UpdateCardCommand(
                id,
                request.Term,
                request.Translation,
                request.Transcription,
                request.HasExample,
                request.Example,
                request.Learned),
            cancellationToken);

        return Ok(new UpdateCardResponseDto(updated));
    }

    [HttpPost("{cardId:int}/review")]
    public async Task<ActionResult<UpdateCardReviewResponseDto>> UpdateCardReview(
        int cardId,
        UpdateCardReviewRequestDto request,
        CancellationToken cancellationToken)
    {
        var updated = await cardApplicationService.UpdateReviewAsync(
            cardId,
            request.IsCorrect,
            cancellationToken);

        return Ok(new UpdateCardReviewResponseDto(updated));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<DeleteCardResponseDto>> DeleteById(
        int id,
        CancellationToken cancellationToken)
    {
        var deleted = await cardApplicationService.DeleteByIdAsync(id, cancellationToken);
        return Ok(new DeleteCardResponseDto(deleted));
    }

    [HttpDelete("by-user/{userId:int}")]
    public async Task<ActionResult<DeleteCardsByUserIdResponseDto>> DeleteByUserId(
        int userId,
        CancellationToken cancellationToken)
    {
        var deleted = await cardApplicationService.DeleteByUserIdAsync(userId, cancellationToken);
        return Ok(new DeleteCardsByUserIdResponseDto(deleted));
    }
}
