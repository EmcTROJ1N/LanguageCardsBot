using Cards.Application.Imports;
using Grpc.Core;
using LanguageCardsBot.Contracts.Cards.V3;
using Mapster;

namespace Cards.Presentation.Services;

/// <summary>
/// Adapts card import gRPC requests to shared import application use cases.
/// </summary>
public class CardsImportGrpcService(ICardsImportApplicationService cardsImportApplicationService)
    : CardsImportService.CardsImportServiceBase
{
    /// <summary>
    /// Handles a gRPC request to import cards from a JSON document.
    /// </summary>
    public override async Task<ImportCardsFromJsonResponse> ImportCardsFromJson(
        ImportCardsFromJsonRequest request,
        ServerCallContext context)
    {
        var result = await cardsImportApplicationService.ImportCardsFromJsonAsync(
            request.Json,
            request.UserId,
            context.CancellationToken);

        var response = new ImportCardsFromJsonResponse
        {
            IsSuccess = result.IsSuccess
        };

        if (result.Data is not null)
        {
            response.Data = result.Data.Adapt<CardsImportResult>();
            response.Data.Errors.AddRange(result.Data.Errors);
        }

        response.Errors.AddRange(result.Errors.Adapt<List<OperationError>>());

        return response;
    }
}
