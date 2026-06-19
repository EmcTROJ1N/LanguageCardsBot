namespace Cards.Application.Imports;

/// <summary>
/// Coordinates card import use cases shared by gRPC and REST transports.
/// </summary>
public interface ICardsImportApplicationService
{
    /// <summary>
    /// Imports cards for a user from a JSON document.
    /// </summary>
    Task<ImportCardsFromJsonResult> ImportCardsFromJsonAsync(
        string json,
        int userId,
        CancellationToken cancellationToken = default);
}
