namespace Cards.Contracts.Rest.Translations;

/// <summary>
/// Represents the REST response for a translation request.
/// </summary>
public sealed record TranslateResponseDto(TranslationResultDto Result);
