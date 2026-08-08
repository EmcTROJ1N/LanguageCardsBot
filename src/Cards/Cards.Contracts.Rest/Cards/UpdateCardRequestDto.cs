namespace Cards.Contracts.Rest.Cards;

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
