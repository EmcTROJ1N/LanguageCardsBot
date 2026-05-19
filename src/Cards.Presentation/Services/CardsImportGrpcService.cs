using Cards.Application.Imports;
using Grpc.Core;
using LanguageCardsBot.Contracts.Cards.V3;

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
            response.Data = new CardsImportResult
            {
                Imported = result.Data.Imported,
                Skipped = result.Data.Skipped
            };
            response.Data.Errors.AddRange(result.Data.Errors);
        }

        response.Errors.AddRange(result.Errors.Select(ToGrpcOperationError));

        return response;
    }

    /// <summary>
    /// Converts an application operation error to the gRPC contract type.
    /// </summary>
    private static OperationError ToGrpcOperationError(OperationErrorResult error)
    {
        var operationError = new OperationError
        {
            Message = error.Message,
            Code = error.Code ?? string.Empty
        };

        if (error.Target is not null)
            operationError.Target = error.Target;

        return operationError;
    }
}
