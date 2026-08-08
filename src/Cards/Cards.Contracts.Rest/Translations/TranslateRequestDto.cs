namespace Cards.Contracts.Rest.Translations;

/// <summary>
/// Represents the REST request body for translating a term.
/// </summary>
public sealed class TranslateRequestDto
{
    /// <summary>Gets the source-language term to translate.</summary>
    public string Term { get; init; } = string.Empty;
}
