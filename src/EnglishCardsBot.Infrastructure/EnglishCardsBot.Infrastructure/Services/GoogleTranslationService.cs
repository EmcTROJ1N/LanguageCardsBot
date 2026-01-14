using EnglishCardsBot.Application.Interfaces;

namespace EnglishCardsBot.Infrastructure.Services;

public class GoogleTranslationService : ITranslationService
{
    public async Task<(string Translation, string Transcription, string Example)> TranslateAsync(string term, CancellationToken cancellationToken = default)
    {
        // Simple heuristic: check if term contains Latin letters
        var hasLatinLetters = term.Any(ch => (ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z'));
        
        var src = hasLatinLetters ? "en" : "ru";
        var dest = hasLatinLetters ? "ru" : "en";

        try
        {
            // Using Google Translate API (you'll need to add a package like Google.Cloud.Translate.V3)
            // For now, returning placeholder - you can integrate with actual translation service
            var translation = await Task.FromResult($"[{src}->{dest}] {term}");
            var transcription = $"/{term}/";
            var example = src == "en" 
                ? $"I learned the word \"{term}\" today."
                : $"Сегодня я выучил слово \"{term}\".";

            return (translation, transcription, example);
        }
        catch
        {
            return ("", $"/{term}/", "");
        }
    }
}

