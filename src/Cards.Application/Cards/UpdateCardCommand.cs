namespace Cards.Application.Cards;

public sealed record UpdateCardCommand(
    int Id,
    string Term,
    string Translation,
    string Transcription,
    bool HasExample,
    string? Example,
    bool? Learned);
