namespace Cards.Contracts.Rest.Cards;

/// <summary>
/// Represents a REST response containing an optional card.
/// </summary>
public sealed record GetCardResponseDto(CardDto? Card);
