namespace Cards.Contracts.Rest.Translations;

/// <summary>
/// Represents translated card text returned by the REST API.
/// </summary>
public sealed record TranslationResultDto(
    string Translation,
    string Transcription,
    string Example);
