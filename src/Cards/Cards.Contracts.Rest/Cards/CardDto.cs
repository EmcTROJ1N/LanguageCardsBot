namespace Cards.Contracts.Rest.Cards;

/// <summary>
/// Represents a card returned by the REST API.
/// </summary>
public sealed record CardDto(
    int Id,
    int UserId,
    string Term,
    string Translation,
    string Transcription,
    string? Example,
    int Level,
    DateTime? NextReviewAt,
    bool Learned,
    DateTime CreatedAt,
    DateTime? LastReviewAt,
    int TotalReviews,
    int CorrectReviews);
