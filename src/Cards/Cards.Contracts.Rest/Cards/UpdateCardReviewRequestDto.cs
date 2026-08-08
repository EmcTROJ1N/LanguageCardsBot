namespace Cards.Contracts.Rest.Cards;

/// <summary>
/// Represents the REST request body for recording a card review result.
/// </summary>
public sealed class UpdateCardReviewRequestDto
{
    /// <summary>Gets a value indicating whether the user answered correctly.</summary>
    public bool IsCorrect { get; init; }
}
