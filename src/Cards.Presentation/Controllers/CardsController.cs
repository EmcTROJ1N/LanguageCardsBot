using Cards.Application.Cards;
using Cards.Presentation.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Cards.Presentation.Controllers;

/// <summary>
/// Exposes card use cases through the public REST API.
/// </summary>
[ApiController]
[Route("api/cards/v3/cards")]
public sealed class CardsController(ICardApplicationService cardApplicationService) : ControllerBase
{
    /// <summary>
    /// Gets a card by its identifier.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<GetCardResponseDto>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var card = await cardApplicationService.GetByIdAsync(id, cancellationToken);
        return Ok(new GetCardResponseDto(card?.ToDto()));
    }

    /// <summary>
    /// Gets all cards.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<GetCardsResponseDto>> GetAll(CancellationToken cancellationToken)
    {
        var cards = await cardApplicationService.GetAllAsync(cancellationToken);
        return Ok(new GetCardsResponseDto(cards.Select(x => x.ToDto()).ToList()));
    }

    /// <summary>
    /// Gets all cards owned by a user.
    /// </summary>
    [HttpGet("by-user/{userId:int}")]
    public async Task<ActionResult<GetCardsResponseDto>> GetByUserId(
        int userId,
        CancellationToken cancellationToken)
    {
        var cards = await cardApplicationService.GetByUserIdAsync(userId, cancellationToken);
        return Ok(new GetCardsResponseDto(cards.Select(x => x.ToDto()).ToList()));
    }

    /// <summary>
    /// Gets the next card due for review for a user.
    /// </summary>
    [HttpGet("due/{userId:int}")]
    public async Task<ActionResult<GetCardResponseDto>> GetDueCard(
        int userId,
        CancellationToken cancellationToken)
    {
        var card = await cardApplicationService.GetDueCardAsync(userId, cancellationToken);
        return Ok(new GetCardResponseDto(card?.ToDto()));
    }

    /// <summary>
    /// Adds a card for a user.
    /// </summary>
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

    /// <summary>
    /// Updates editable fields for a card.
    /// </summary>
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

    /// <summary>
    /// Records a review result for a card.
    /// </summary>
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

    /// <summary>
    /// Deletes a card by its identifier.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<DeleteCardResponseDto>> DeleteById(
        int id,
        CancellationToken cancellationToken)
    {
        var deleted = await cardApplicationService.DeleteByIdAsync(id, cancellationToken);
        return Ok(new DeleteCardResponseDto(deleted));
    }

    /// <summary>
    /// Deletes all cards owned by a user.
    /// </summary>
    [HttpDelete("by-user/{userId:int}")]
    public async Task<ActionResult<DeleteCardsByUserIdResponseDto>> DeleteByUserId(
        int userId,
        CancellationToken cancellationToken)
    {
        var deleted = await cardApplicationService.DeleteByUserIdAsync(userId, cancellationToken);
        return Ok(new DeleteCardsByUserIdResponseDto(deleted));
    }
}
