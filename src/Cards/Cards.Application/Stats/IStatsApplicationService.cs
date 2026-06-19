namespace Cards.Application.Stats;

/// <summary>
/// Coordinates statistics use cases shared by gRPC and REST transports.
/// </summary>
public interface IStatsApplicationService
{
    /// <summary>
    /// Gets today's statistics for a user.
    /// </summary>
    Task<TodayStatsResult> GetTodayStatsAsync(int userId, CancellationToken cancellationToken = default);
}
