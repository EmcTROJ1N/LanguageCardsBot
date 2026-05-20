using System.Text.Json;
using Cards.Application.Translations;
using Grpc.Core;
using LanguageCardsBot.Contracts.Cards.V3;
using Mapster;

namespace Cards.Presentation.Services;

/// <summary>
/// Adapts translation gRPC requests to the shared translation application use case.
/// </summary>
public sealed class GoogleTranslationService(ITranslationApplicationService translationApplicationService)
    : TranslationService.TranslationServiceBase
{
    /// <summary>
    /// Handles a gRPC request to translate a term.
    /// </summary>
    public override async Task<TranslateResponse> Translate(TranslateRequest request, ServerCallContext context)
    {
        try
        {
            var result = await translationApplicationService.TranslateAsync(
                request.Term,
                context.CancellationToken);

            return new TranslateResponse
            {
                Result = result.Adapt<LanguageCardsBot.Contracts.Cards.V3.TranslationResult>()
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
