using System.Text.Json;
using Grpc.Core;
using LanguageCardsBot.Contracts.Cards.V3;

namespace Cards.Presentation.Services;

sealed class ImportPayload
{
    public string? Version { get; set; }
    public string? ExportedAt { get; set; }
    public int? TotalCards { get; set; }
    public List<Card> Cards { get; set; } = [];
}

public class CardsImportGrpcService(CardGrpcService cardService) : CardsImportService.CardsImportServiceBase
{
    public override async Task<ImportCardsFromJsonResponse> ImportCardsFromJson(ImportCardsFromJsonRequest request, ServerCallContext context)
    {
        var payload = TryParseImportPayload(request.Json);
        var response = new ImportCardsFromJsonResponse();

        if (payload?.Cards == null || payload.Cards.Count == 0)
        {
            response.IsSuccess = false;
            response.Errors.Add(new OperationError
            {
                Message = "В файле не найден массив карточек `cards` или он пустой. Используйте /export для примера."
            });
            return response;
        }

        const int maxCards = 5000;
        if (payload.Cards.Count > maxCards)
        {
            response.IsSuccess = false;
            response.Errors.Add(new OperationError
            {
                Message = $"Слишком много карточек ({payload.Cards.Count}). Максимум за раз: {maxCards}"
            });
        }

        var imported = 0;
        var skipped = 0;
        var errors = new List<OperationError>();

        foreach (var card in payload.Cards)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var term = (card.Term ?? string.Empty).Trim();
            var translation = (card.Translation ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(term) || string.IsNullOrWhiteSpace(translation))
            {
                skipped++;
                errors.Add(new OperationError { Message = $"Пропуск: term/translation пустые (term='{term}', translation='{translation}" });
                continue;
            }

            if (term.Length > 200 || translation.Length > 500)
            {
                skipped++;
                errors.Add(new OperationError { Message = $"Пропуск: слишком длинные поля (term={term.Length}, translation={translation.Length})" });

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

                // TODO: is that right?
                await cardService.Add(new AddCardRequest
                {
                    UserId = request.UserId,
                    Term = term,
                    Translation = translation,
                    Transcription = transcription,
                    Example = example
                }, context);

                imported++;
            }
            catch (Exception ex)
            {
                skipped++;
                errors.Add(new OperationError { Message = $"Ошибка для '{term}': {ex.Message}" });
            }
        }

        response.IsSuccess = true;
        response.Errors.AddRange(errors);
        response.Data.Skipped = skipped;
        response.Data.Imported = imported;
        return response;
    }

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
            {
                return payload;
            }
        }
        catch
        {
        }

        try
        {
            var cards = JsonSerializer.Deserialize<List<Card>>(json, options);
            if (cards != null && cards.Count > 0)
            {
                return new ImportPayload { Version = "unknown", Cards = cards };
            }
        }
        catch
        {
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            foreach (var property in document.RootElement.EnumerateObject())
            {
                if (!string.Equals(property.Name, "cards", StringComparison.OrdinalIgnoreCase) ||
                    property.Value.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                var cards = JsonSerializer.Deserialize<List<Card>>(property.Value.GetRawText(), options);
                if (cards != null && cards.Count > 0)
                {
                    return new ImportPayload { Version = "unknown", Cards = cards };
                }
            }
        }
        catch
        {
        }

        return null;
    }
}