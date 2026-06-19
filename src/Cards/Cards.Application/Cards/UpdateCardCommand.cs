namespace Cards.Application.Cards;

/// <summary>
/// Represents the editable card fields for a card update use case.
/// </summary>
public sealed record UpdateCardCommand(
    int Id,
    string Term,
    string Translation,
    string Transcription,
    bool HasExample,
    string? Example,
    bool? Learned);
