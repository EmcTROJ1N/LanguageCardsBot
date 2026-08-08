namespace Cards.Contracts.Rest.Cards;

/// <summary>
/// Represents a REST response containing a card collection.
/// </summary>
public sealed record GetCardsResponseDto(IReadOnlyCollection<CardDto> Cards);
