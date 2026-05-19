namespace Cards.Presentation.Contracts;

public sealed class TranslateRequestDto
{
    public string Term { get; init; } = string.Empty;
}

public sealed record TranslationResultDto(
    string Translation,
    string Transcription,
    string Example);

public sealed record TranslateResponseDto(TranslationResultDto Result);
