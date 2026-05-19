namespace Cards.Application.Translations;

/// <summary>
/// Represents translated card text produced by the translation use case.
/// </summary>
public sealed record TranslationResult(
    string Translation,
    string Transcription,
    string Example);
