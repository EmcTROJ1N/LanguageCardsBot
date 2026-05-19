namespace Cards.Application.Imports;

public interface ICardsImportApplicationService
{
    Task<ImportCardsFromJsonResult> ImportCardsFromJsonAsync(
        string json,
        int userId,
        CancellationToken cancellationToken = default);
}
