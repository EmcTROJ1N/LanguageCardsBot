namespace Cards.Contracts.Rest.Cards;

/// <summary>Response payload for the card search endpoint.</summary>
public sealed record SearchCardsResponseDto(List<CardDto> Cards, CardCountsDto Counts);
