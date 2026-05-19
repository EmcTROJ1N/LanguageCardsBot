using System.Text.Json;
using Cards.Application.Translations;
using Cards.Presentation.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Cards.Presentation.Controllers;

[ApiController]
[Route("api/cards/v3/translation")]
public sealed class TranslationController(ITranslationApplicationService translationApplicationService)
    : ControllerBase
{
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

            return Ok(new TranslateResponseDto(
                new TranslationResultDto(
                    result.Translation,
                    result.Transcription,
                    result.Example)));
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
