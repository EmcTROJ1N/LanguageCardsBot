using System.Text.Json;

namespace Cards.Application.Translations;

/// <summary>
/// Implements translation use cases through the Google Translate HTTP endpoint.
/// </summary>
public sealed class GoogleTranslationApplicationService(
    HttpClient httpClient,
    TranslationOptions options) : ITranslationApplicationService
{
    /// <inheritdoc />
    public async Task<TranslationResult> TranslateAsync(
        string term,
        CancellationToken cancellationToken = default)
    {
        var normalizedTerm = (term ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalizedTerm))
            throw new ArgumentException("Term is required.", nameof(term));

        var uri = "https://translate.googleapis.com/translate_a/single" +
                  $"?client=gtx&sl={Uri.EscapeDataString(options.SourceLanguage)}" +
                  $"&tl={Uri.EscapeDataString(options.TargetLanguage)}" +
                  $"&dt=t&q={Uri.EscapeDataString(normalizedTerm)}";

        var json = await httpClient.GetStringAsync(uri, cancellationToken);
        var translation = ParseTranslation(json);

        if (string.IsNullOrWhiteSpace(translation))
            throw new InvalidOperationException("Translation provider returned an empty result.");

        return new TranslationResult(
            translation,
            string.Empty,
            string.Empty);
    }

    /// <summary>
    /// Extracts translated text from the provider response payload.
    /// </summary>
    private static string ParseTranslation(string json)
    {
        using var document = JsonDocument.Parse(json);
        if (document.RootElement.ValueKind != JsonValueKind.Array ||
            document.RootElement.GetArrayLength() == 0 ||
            document.RootElement[0].ValueKind != JsonValueKind.Array)
        {
            return string.Empty;
        }

        var parts = new List<string>();
        foreach (var segment in document.RootElement[0].EnumerateArray())
        {
            if (segment.ValueKind != JsonValueKind.Array ||
                segment.GetArrayLength() == 0 ||
                segment[0].ValueKind != JsonValueKind.String)
            {
                continue;
            }

            parts.Add(segment[0].GetString() ?? string.Empty);
        }

        return string.Concat(parts).Trim();
    }
}
