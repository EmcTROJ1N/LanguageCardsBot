using Cards.Application.Imports;
using Cards.Contracts.Rest.Imports;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace Cards.Presentation.Controllers;

/// <summary>
/// Exposes card import use cases through the public REST API.
/// </summary>
[ApiController]
[Route("import")]
public sealed class CardsImportController(ICardsImportApplicationService cardsImportApplicationService) : ControllerBase
{
    /// <summary>
    /// Imports cards for a user from a JSON document.
    /// </summary>
    [HttpPost("json")]
    public async Task<ActionResult<ImportCardsFromJsonResponseDto>> ImportCardsFromJson(
        ImportCardsFromJsonRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await cardsImportApplicationService.ImportCardsFromJsonAsync(
            request.Json,
            request.UserId,
            cancellationToken);

        return Ok(result.Adapt<ImportCardsFromJsonResponseDto>());
    }
}
