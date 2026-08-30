namespace Cards.Contracts.Rest.Cards;

/// <summary>
/// Represents the REST request body for creating a card.
/// UserId is resolved server-side from the JWT bearer token.
/// </summary>
public sealed class AddCardRequestDto
{
    /// <summary>Gets the source-language term (the word or phrase being learned).</summary>
    public string Term { get; init; } = string.Empty;
    /// <summary>Gets the translation of the term into the user's language.</summary>
    public string Translation { get; init; } = string.Empty;
    /// <summary>Gets the phonetic transcription of the term.</summary>
    public string Transcription { get; init; } = string.Empty;
    /// <summary>Gets an optional usage example for the term.</summary>
    public string? Example { get; init; }
}
