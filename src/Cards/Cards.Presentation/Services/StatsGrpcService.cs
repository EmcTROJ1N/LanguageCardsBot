using Cards.Application.Stats;
using Grpc.Core;
using LanguageCardsBot.Contracts.Cards.V3;
using Mapster;

namespace Cards.Presentation.Services;

/// <summary>
/// Adapts statistics gRPC requests to shared statistics application use cases.
/// </summary>
public sealed class StatsGrpcService(IStatsApplicationService statsApplicationService) : StatsService.StatsServiceBase
{
    /// <summary>
    /// Handles a gRPC request to get today's statistics for a user.
    /// </summary>
    public override async Task<GetTodayStatsResponse> GetTodayStats(GetTodayStatsRequest request,
        ServerCallContext context)
    {
        var stats = await statsApplicationService.GetTodayStatsAsync(request.UserId, context.CancellationToken);

        return new GetTodayStatsResponse
        {
            Stats = stats.Adapt<TodayStats>()
        };
    }
}
