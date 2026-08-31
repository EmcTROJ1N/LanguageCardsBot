namespace Cards.Contracts.Rest.Cards;

/// <summary>Per-status card counts for the authenticated user, unaffected by search query or active filter.</summary>
public sealed record CardCountsDto(int All, int Due, int New, int Learned);
