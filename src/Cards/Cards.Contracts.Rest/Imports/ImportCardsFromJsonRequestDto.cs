namespace Cards.Contracts.Rest.Imports;

/// <summary>
/// Represents the REST request body for importing cards from JSON.
/// </summary>
public sealed class ImportCardsFromJsonRequestDto
{
    /// <summary>Gets the raw JSON payload with cards to import.</summary>
    public string Json { get; init; } = string.Empty;

    /// <summary>Gets the identifier of the user who will own the imported cards.</summary>
    public int UserId { get; init; }
}
