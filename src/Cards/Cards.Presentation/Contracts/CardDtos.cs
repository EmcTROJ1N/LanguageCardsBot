namespace Cards.Presentation.Contracts;

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

/// <summary>
/// Represents a REST response containing an optional card.
/// </summary>
public sealed record GetCardResponseDto(CardDto? Card);

/// <summary>
/// Represents a REST response containing a card collection.
/// </summary>
public sealed record GetCardsResponseDto(IReadOnlyCollection<CardDto> Cards);

/// <summary>
/// Represents a REST response containing a created or existing card.
/// </summary>
public sealed record CardResponseDto(CardDto Card);

/// <summary>
/// Represents the REST request body for creating a card.
/// </summary>
public sealed class AddCardRequestDto
{
    public int UserId { get; init; }
    public string Term { get; init; } = string.Empty;
    public string Translation { get; init; } = string.Empty;
    public string Transcription { get; init; } = string.Empty;
    public string? Example { get; init; }
}

/// <summary>
/// Represents the REST request body for updating a card.
/// </summary>
public sealed class UpdateCardRequestDto
{
    public string Term { get; init; } = string.Empty;
    public string Translation { get; init; } = string.Empty;
    public string Transcription { get; init; } = string.Empty;
    public string? Example { get; init; }
    public bool HasExample { get; init; }
    public bool? Learned { get; init; }
}

/// <summary>
/// Represents the REST request body for recording a card review result.
/// </summary>
public sealed class UpdateCardReviewRequestDto
{
    public bool IsCorrect { get; init; }
}

/// <summary>
/// Represents the REST response for a card update operation.
/// </summary>
public sealed record UpdateCardResponseDto(bool Updated);

/// <summary>
/// Represents the REST response for a card review update operation.
/// </summary>
public sealed record UpdateCardReviewResponseDto(bool Updated);

/// <summary>
/// Represents the REST response for deleting a card.
/// </summary>
public sealed record DeleteCardResponseDto(bool Deleted);

/// <summary>
/// Represents the REST response for deleting cards by user identifier.
/// </summary>
public sealed record DeleteCardsByUserIdResponseDto(bool Deleted);
