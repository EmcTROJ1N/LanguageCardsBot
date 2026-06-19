namespace Cards.Application.Cards;

/// <summary>
/// Represents the data required to create a card.
/// </summary>
public sealed record AddCardCommand(
    int UserId,
    string Term,
    string Translation,
    string Transcription,
    string? Example);
