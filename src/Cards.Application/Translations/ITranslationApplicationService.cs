namespace Cards.Application.Translations;

/// <summary>
/// Coordinates translation use cases shared by gRPC and REST transports.
/// </summary>
public interface ITranslationApplicationService
{
    /// <summary>
    /// Translates a term into the configured target language.
    /// </summary>
    Task<TranslationResult> TranslateAsync(string term, CancellationToken cancellationToken = default);
}
