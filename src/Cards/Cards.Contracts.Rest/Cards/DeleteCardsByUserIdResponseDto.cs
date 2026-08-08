namespace Cards.Contracts.Rest.Cards;

/// <summary>
/// Represents the REST response for deleting cards by user identifier.
/// </summary>
public sealed record DeleteCardsByUserIdResponseDto(bool Deleted);
