using Cards.Application.Imports;
using Cards.Presentation.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Cards.Presentation.Controllers;

[ApiController]
[Route("api/cards/v3/import")]
public sealed class CardsImportController(ICardsImportApplicationService cardsImportApplicationService) : ControllerBase
{
    [HttpPost("json")]
    public async Task<ActionResult<ImportCardsFromJsonResponseDto>> ImportCardsFromJson(
        ImportCardsFromJsonRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await cardsImportApplicationService.ImportCardsFromJsonAsync(
            request.Json,
            request.UserId,
            cancellationToken);

        return Ok(result.ToDto());
    }
}
