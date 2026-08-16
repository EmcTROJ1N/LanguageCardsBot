using System.Text.Json;
using Cards.Application.Translations;
using Cards.Contracts.Rest.Translations;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace Cards.Presentation.Controllers;

/// <summary>
/// Exposes translation use cases through the public REST API.
/// </summary>
[ApiController]
[Route("v3/translation")]
public sealed class TranslationController(ITranslationApplicationService translationApplicationService)
    : ControllerBase
{
    /// <summary>
    /// Translates a term into the configured target language.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TranslateResponseDto>> Translate(
        TranslateRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await translationApplicationService.TranslateAsync(
                request.Term,
                cancellationToken);

            return Ok(new TranslateResponseDto(result.Adapt<TranslationResultDto>()));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = ex.Message });
        }
        catch (JsonException ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
        }
    }
}
