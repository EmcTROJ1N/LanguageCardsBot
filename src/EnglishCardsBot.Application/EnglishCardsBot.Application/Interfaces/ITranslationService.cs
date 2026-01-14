namespace EnglishCardsBot.Application.Interfaces;

public interface ITranslationService
{
    Task<(string Translation, string Transcription, string Example)> TranslateAsync(string term, CancellationToken cancellationToken = default);
}

