namespace Cards.Contracts.Rest.Cards;

/// <summary>
/// Represents a REST response containing a created or existing card.
/// </summary>
public sealed record CardResponseDto(CardDto Card);
