using Cards.Application.Stats;
using Cards.Presentation.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Cards.Presentation.Controllers;

[ApiController]
[Route("api/cards/v3/stats")]
public sealed class StatsController(IStatsApplicationService statsApplicationService) : ControllerBase
{
    [HttpGet("today/{userId:int}")]
    public async Task<ActionResult<GetTodayStatsResponseDto>> GetTodayStats(
        int userId,
        CancellationToken cancellationToken)
    {
        var stats = await statsApplicationService.GetTodayStatsAsync(userId, cancellationToken);
        return Ok(new GetTodayStatsResponseDto(stats.ToDto()));
    }
}
