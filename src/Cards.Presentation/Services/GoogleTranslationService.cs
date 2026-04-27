using Grpc.Core;
using LanguageCardsBot.Contracts.Cards.V3;

namespace Cards.Presentation.Services;

public sealed class GoogleTranslationService : TranslationService.TranslationServiceBase
{
    public override Task<TranslateResponse> Translate(TranslateRequest request, ServerCallContext context)
    {
        throw GrpcServiceBase.CreateUnimplementedException(nameof(Translate));
    }
}
