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
    /// <summary>Gets the identifier of the user who owns the card.</summary>
    public int UserId { get; init; }
    /// <summary>Gets the source-language term (the word or phrase being learned).</summary>
    public string Term { get; init; } = string.Empty;
    /// <summary>Gets the translation of the term into the user's language.</summary>
    public string Translation { get; init; } = string.Empty;
    /// <summary>Gets the phonetic transcription of the term.</summary>
    public string Transcription { get; init; } = string.Empty;
    /// <summary>Gets an optional usage example for the term.</summary>
    public string? Example { get; init; }
}

/// <summary>
/// Represents the REST request body for updating a card.
/// </summary>
public sealed class UpdateCardRequestDto
{
    /// <summary>Gets the new source-language term.</summary>
    public string Term { get; init; } = string.Empty;
    /// <summary>Gets the new translation.</summary>
    public string Translation { get; init; } = string.Empty;
    /// <summary>Gets the new phonetic transcription.</summary>
    public string Transcription { get; init; } = string.Empty;
    /// <summary>Gets the new usage example (may be null when <see cref="HasExample"/> is <c>false</c>).</summary>
    public string? Example { get; init; }
    /// <summary>Gets a value indicating whether <see cref="Example"/> was provided (distinguishes "clear" from "no change").</summary>
    public bool HasExample { get; init; }
    /// <summary>Gets a value indicating whether the card is marked as learned; <c>null</c> leaves the value unchanged.</summary>
    public bool? Learned { get; init; }
}

/// <summary>
/// Represents the REST request body for recording a card review result.
/// </summary>
public sealed class UpdateCardReviewRequestDto
{
    /// <summary>Gets a value indicating whether the user answered correctly.</summary>
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
