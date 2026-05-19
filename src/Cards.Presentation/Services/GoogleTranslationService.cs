using System.Text.Json;
using Cards.Application.Translations;
using Grpc.Core;
using LanguageCardsBot.Contracts.Cards.V3;

namespace Cards.Presentation.Services;

public sealed class GoogleTranslationService(ITranslationApplicationService translationApplicationService)
    : TranslationService.TranslationServiceBase
{
    public override async Task<TranslateResponse> Translate(TranslateRequest request, ServerCallContext context)
    {
        try
        {
            var result = await translationApplicationService.TranslateAsync(
                request.Term,
                context.CancellationToken);

            return new TranslateResponse
            {
                Result = new LanguageCardsBot.Contracts.Cards.V3.TranslationResult
                {
                    Translation = result.Translation,
                    Transcription = result.Transcription,
                    Example = result.Example
                }
            };
        }
        catch (ArgumentException ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
        catch (HttpRequestException ex)
        {
            throw new RpcException(new Status(StatusCode.Unavailable, $"Translation provider is unavailable: {ex.Message}"));
        }
        catch (JsonException ex)
        {
            throw new RpcException(new Status(StatusCode.Internal, $"Translation response could not be parsed: {ex.Message}"));
        }
        catch (InvalidOperationException ex)
        {
            throw new RpcException(new Status(StatusCode.Unavailable, ex.Message));
        }
    }
}
