using Cards.Application.Cards;
using Cards.Application.Users;
using Cards.Contracts.Rest.Cards;
using Cards.Presentation.Extensions;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cards.Presentation.Controllers;

/// <summary>
/// Exposes card use cases through the public REST API.
/// </summary>
[ApiController]
[Route("cards")]
public sealed class CardsController(
    ICardApplicationService cardApplicationService,
    IUserApplicationService userApplicationService) : ControllerBase
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
        return Ok(new GetCardResponseDto(card?.Adapt<CardDto>()));
    }

    /// <summary>
    /// Gets all cards owned by the authenticated user.
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<GetCardsResponseDto>> GetAll(CancellationToken cancellationToken)
    {
        var keycloakId = User.GetKeycloakId();
        var user = await userApplicationService.GetOrCreateByKeycloakIdAsync(keycloakId, cancellationToken);
        var cards = await cardApplicationService.GetByUserIdAsync(user.Id, cancellationToken);
        return Ok(new GetCardsResponseDto(cards.Adapt<List<CardDto>>()));
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
        return Ok(new GetCardsResponseDto(cards.Adapt<List<CardDto>>()));
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
        return Ok(new GetCardResponseDto(card?.Adapt<CardDto>()));
    }

    /// <summary>
    /// Adds a card for the authenticated user. UserId is resolved from the JWT bearer token.
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<CardResponseDto>> Add(
        AddCardRequestDto request,
        CancellationToken cancellationToken)
    {
        var keycloakId = User.GetKeycloakId();
        var user = await userApplicationService.GetOrCreateByKeycloakIdAsync(keycloakId, cancellationToken);

        var card = await cardApplicationService.AddAsync(
            new AddCardCommand(
                UserId: user.Id,
                Term: request.Term,
                Translation: request.Translation,
                Transcription: request.Transcription,
                Example: request.Example),
            cancellationToken);

        return Ok(new CardResponseDto(card.Adapt<CardDto>()));
    }

    /// <summary>
    /// Exports all cards owned by the authenticated user as a downloadable file.
    /// </summary>
    /// <param name="format">Output format: <c>json</c> or <c>csv</c>.</param>
    [HttpGet("export")]
    [Authorize]
    public async Task<IActionResult> Export(
        [FromQuery] string format,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<CardExportFormat>(format, ignoreCase: true, out var exportFormat))
            return BadRequest(new { error = $"Unsupported format '{format}'. Use 'json' or 'csv'." });

        var keycloakId = User.GetKeycloakId();
        var user = await userApplicationService.GetOrCreateByKeycloakIdAsync(keycloakId, cancellationToken);
        var result = await cardApplicationService.ExportAsync(user.Id, exportFormat, cancellationToken);
        return File(result.Content, result.ContentType, result.FileName);
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
            request.Adapt<UpdateCardCommand>() with { Id = id },
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
