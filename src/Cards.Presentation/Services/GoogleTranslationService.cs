using System.Text.Json;
using Grpc.Core;
using LanguageCardsBot.Contracts.Cards.V3;

namespace Cards.Presentation.Services;

public sealed class GoogleTranslationService(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration) : TranslationService.TranslationServiceBase
{
    public override async Task<TranslateResponse> Translate(TranslateRequest request, ServerCallContext context)
    {
        var term = request.Term.Trim();
        if (string.IsNullOrWhiteSpace(term))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Term is required."));
        }

        var targetLanguage = configuration["Translation:TargetLanguage"]
                             ?? Environment.GetEnvironmentVariable("TRANSLATION_TARGET_LANGUAGE")
                             ?? "ru";
        var sourceLanguage = configuration["Translation:SourceLanguage"]
                             ?? Environment.GetEnvironmentVariable("TRANSLATION_SOURCE_LANGUAGE")
                             ?? "auto";

        try
        {
            var httpClient = httpClientFactory.CreateClient(nameof(GoogleTranslationService));
            var uri = "https://translate.googleapis.com/translate_a/single" +
                      $"?client=gtx&sl={Uri.EscapeDataString(sourceLanguage)}" +
                      $"&tl={Uri.EscapeDataString(targetLanguage)}" +
                      $"&dt=t&q={Uri.EscapeDataString(term)}";

            var json = await httpClient.GetStringAsync(uri, context.CancellationToken);
            var translation = ParseTranslation(json);

            if (string.IsNullOrWhiteSpace(translation))
            {
                throw new RpcException(new Status(StatusCode.Unavailable, "Translation provider returned an empty result."));
            }

            return new TranslateResponse
            {
                Result = new TranslationResult
                {
                    Translation = translation,
                    Transcription = string.Empty,
                    Example = string.Empty
                }
            };
        }
        catch (RpcException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            throw new RpcException(new Status(StatusCode.Unavailable, $"Translation provider is unavailable: {ex.Message}"));
        }
        catch (JsonException ex)
        {
            throw new RpcException(new Status(StatusCode.Internal, $"Translation response could not be parsed: {ex.Message}"));
        }
    }

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
