namespace Cards.Application.Stats;

public interface IStatsApplicationService
{
    Task<TodayStatsResult> GetTodayStatsAsync(int userId, CancellationToken cancellationToken = default);
}
