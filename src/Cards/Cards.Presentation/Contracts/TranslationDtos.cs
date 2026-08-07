namespace Cards.Presentation.Contracts;

/// <summary>
/// Represents the REST request body for translating a term.
/// </summary>
public sealed class TranslateRequestDto
{
    /// <summary>Gets the source-language term to translate.</summary>
    public string Term { get; init; } = string.Empty;
}

/// <summary>
/// Represents translated card text returned by the REST API.
/// </summary>
public sealed record TranslationResultDto(
    string Translation,
    string Transcription,
    string Example);

/// <summary>
/// Represents the REST response for a translation request.
/// </summary>
public sealed record TranslateResponseDto(TranslationResultDto Result);
