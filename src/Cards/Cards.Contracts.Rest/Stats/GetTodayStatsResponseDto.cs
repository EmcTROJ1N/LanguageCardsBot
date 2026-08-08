namespace Cards.Contracts.Rest.Stats;

/// <summary>
/// Represents a REST response containing today's statistics.
/// </summary>
public sealed record GetTodayStatsResponseDto(TodayStatsDto Stats);
