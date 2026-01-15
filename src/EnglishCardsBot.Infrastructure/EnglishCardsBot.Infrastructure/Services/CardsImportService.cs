using System.Text.Json;
using EnglishCardsBot.Application;
using EnglishCardsBot.Application.DTOs;
using EnglishCardsBot.Application.Interfaces;
using EnglishCardsBot.Application.Services;
using EnglishCardsBot.Domain.Entities;

namespace EnglishCardsBot.Infrastructure.Services;

sealed class ImportPayload
{
    public string? Version { get; set; }
    public string? ExportedAt { get; set; }
    public int? TotalCards { get; set; }
    public List<Card> Cards { get; set; } = [];
}

public class CardsImportService(CardService cardService): ICardsImportService
{
    public async Task<Result<CardsImportResult>> ImportCardsFromJsonAsync(string json, int userId, CancellationToken cancellationToken)
    {
        // 4) Parse payload
        var payload = TryParseImportPayload(json);

        if (payload?.Cards == null || payload.Cards.Count == 0)
            return Result<CardsImportResult>.Failure([
                new("В файле не найден массив карточек `cards` или он пустой. Используйте /export для примера.")
            ]);

        // Safety: limit count
        const int maxCards = 5000;
        if (payload.Cards.Count > maxCards)
            return Result<CardsImportResult>.Failure([
                new($"Слишком много карточек ({payload.Cards.Count}). Максимум за раз: {maxCards}")
            ]);

        // 5) Import
        var imported = 0;
        var skipped = 0;
        var errors = new List<string>();

        foreach (var c in payload.Cards)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var term = (c.Term ?? "").Trim();
            var translation = (c.Translation ?? "").Trim();

            if (string.IsNullOrWhiteSpace(term) || string.IsNullOrWhiteSpace(translation))
            {
                skipped++;
                errors.Add($"• Пропуск: term/translation пустые (term='{term}', translation='{translation}')");
                continue;
            }

            if (term.Length > 200 || translation.Length > 500)
            {
                skipped++;
                errors.Add($"• Пропуск: слишком длинные поля (term={term.Length}, translation={translation.Length})");
                continue;
            }

            try
            {
                var transcription = string.IsNullOrWhiteSpace(c.Transcription)
                    ? $"/{term}/"
                    : c.Transcription.Trim();

                var example = string.IsNullOrWhiteSpace(c.Example)
                    ? null
                    : c.Example.Trim();

                await cardService.AddCardAsync(
                    userId,
                    term,
                    translation,
                    transcription,
                    example,
                    cancellationToken);

                // NOTE:
                // Поля level/learned из файла сейчас не применяются, потому что текущая сигнатура AddCardAsync их не принимает.
                // Если нужно — добавь в CardService отдельный ImportCardAsync(...) или расширь AddCardAsync.

                imported++;
            }
            catch (Exception ex)
            {
                skipped++;
                errors.Add($"• Ошибка для '{term}': {ex.Message}");
            }
        }

        var result = new CardsImportResult
        {
            Imported = imported,
            Skipped = skipped,
            Errors = errors
        };
        return Result<CardsImportResult>.Success(result);
    }

    // =========================
    // Import helpers (DTO + parser)
    // =========================
    private static ImportPayload? TryParseImportPayload(string json)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        // 1) { version, exportedAt, totalCards, cards:[...] } or { cards:[...] }
        try
        {
            var payload = JsonSerializer.Deserialize<ImportPayload>(json, options);
            if (payload?.Cards != null && payload.Cards.Count > 0)
                return payload;
        }
        catch
        {
            // ignore and try other shapes
        }

        // 2) Root array: [ {term, translation, ...}, ... ]
        try
        {
            var cards = JsonSerializer.Deserialize<List<Card>>(json, options);
            if (cards != null && cards.Count > 0)
                return new ImportPayload { Version = "unknown", Cards = cards };
        }
        catch
        {
            // ignore
        }

        // 3) Fallback: locate "cards" property case-insensitively
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in root.EnumerateObject())
                {
                    if (string.Equals(prop.Name, "cards", StringComparison.OrdinalIgnoreCase) &&
                        prop.Value.ValueKind == JsonValueKind.Array)
                    {
                        var cards = JsonSerializer.Deserialize<List<Card>>(prop.Value.GetRawText(), options);
                        if (cards != null && cards.Count > 0)
                            return new ImportPayload { Version = "unknown", Cards = cards };
                    }
                }
            }
        }
        catch
        {
            // ignore
        }

        return null;
    }
}