namespace Cards.Application.Cards;

public sealed record AddCardCommand(
    int UserId,
    string Term,
    string Translation,
    string Transcription,
    string? Example);
