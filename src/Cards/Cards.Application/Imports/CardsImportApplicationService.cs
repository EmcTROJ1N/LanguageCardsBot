using System.Text.Json;
using Cards.Application.Cards;

namespace Cards.Application.Imports;

/// <summary>
/// Implements JSON card import use cases.
/// </summary>
public sealed class CardsImportApplicationService(ICardApplicationService cardApplicationService)
    : ICardsImportApplicationService
{
    private const int MaxCards = 5000;

    /// <inheritdoc />
    public async Task<ImportCardsFromJsonResult> ImportCardsFromJsonAsync(
        string json,
        int userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new ImportCardsFromJsonResult(
                false,
                null,
                [
                    new OperationErrorResult(
                        "В файле не найден массив карточек `cards` или он пустой. Используйте /export для примера.")
                ]);
        }

        var payload = TryParseImportPayload(json);
        if (payload?.Cards == null || payload.Cards.Count == 0)
        {
            return new ImportCardsFromJsonResult(
                false,
                null,
                [
                    new OperationErrorResult(
                        "В файле не найден массив карточек `cards` или он пустой. Используйте /export для примера.")
                ]);
        }

        if (payload.Cards.Count > MaxCards)
        {
            return new ImportCardsFromJsonResult(
                false,
                new CardsImportData(0, payload.Cards.Count, []),
                [
                    new OperationErrorResult(
                        $"Слишком много карточек ({payload.Cards.Count}). Максимум за раз: {MaxCards}")
                ]);
        }

        var imported = 0;
        var skipped = 0;
        var errors = new List<OperationErrorResult>();

        foreach (var card in payload.Cards)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var term = (card.Term ?? string.Empty).Trim();
            var translation = (card.Translation ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(term) || string.IsNullOrWhiteSpace(translation))
            {
                skipped++;
                errors.Add(new OperationErrorResult(
                    $"Пропуск: term/translation пустые (term='{term}', translation='{translation}')"));
                continue;
            }

            if (term.Length > 200 || translation.Length > 500)
            {
                skipped++;
                errors.Add(new OperationErrorResult(
                    $"Пропуск: слишком длинные поля (term={term.Length}, translation={translation.Length})"));
                continue;
            }

            try
            {
                var transcription = string.IsNullOrWhiteSpace(card.Transcription)
                    ? $"/{term}/"
                    : card.Transcription.Trim();

                var example = string.IsNullOrWhiteSpace(card.Example)
                    ? null
                    : card.Example.Trim();

                await cardApplicationService.AddAsync(
                    new AddCardCommand(userId, term, translation, transcription, example),
                    cancellationToken);

                imported++;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                skipped++;
                errors.Add(new OperationErrorResult($"Ошибка для '{term}': {ex.Message}"));
            }
        }

        return new ImportCardsFromJsonResult(
            true,
            new CardsImportData(imported, skipped, errors.Select(x => x.Message).ToList()),
            errors);
    }

    /// <summary>
    /// Parses supported JSON import payload shapes.
    /// </summary>
    private static ImportPayload? TryParseImportPayload(string json)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        try
        {
            var payload = JsonSerializer.Deserialize<ImportPayload>(json, options);
            if (payload?.Cards != null && payload.Cards.Count > 0)
                return payload;
        }
        catch (JsonException)
        {
        }

        try
        {
            var cards = JsonSerializer.Deserialize<List<ImportCardPayload>>(json, options);
            if (cards != null && cards.Count > 0)
                return new ImportPayload { Version = "unknown", Cards = cards };
        }
        catch (JsonException)
        {
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
                return null;

            foreach (var property in document.RootElement.EnumerateObject())
            {
                if (!string.Equals(property.Name, "cards", StringComparison.OrdinalIgnoreCase) ||
                    property.Value.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                var cards = JsonSerializer.Deserialize<List<ImportCardPayload>>(property.Value.GetRawText(), options);
                if (cards != null && cards.Count > 0)
                    return new ImportPayload { Version = "unknown", Cards = cards };
            }
        }
        catch (JsonException)
        {
        }

        return null;
    }

    /// <summary>
    /// Represents the supported top-level card import JSON payload.
    /// </summary>
    private sealed class ImportPayload
    {
        public string? Version { get; set; }
        public string? ExportedAt { get; set; }
        public int? TotalCards { get; set; }
        public List<ImportCardPayload> Cards { get; set; } = [];
    }

    /// <summary>
    /// Represents a card row inside an import payload.
    /// </summary>
    private sealed class ImportCardPayload
    {
        public string? Term { get; set; }
        public string? Translation { get; set; }
        public string? Transcription { get; set; }
        public string? Example { get; set; }
    }
}
