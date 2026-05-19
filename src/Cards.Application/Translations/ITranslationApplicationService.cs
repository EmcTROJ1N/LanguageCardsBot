namespace Cards.Application.Translations;

public interface ITranslationApplicationService
{
    Task<TranslationResult> TranslateAsync(string term, CancellationToken cancellationToken = default);
}
