namespace Cards.Application.Imports;

/// <summary>
/// Represents the result of importing cards from JSON.
/// </summary>
public sealed record ImportCardsFromJsonResult(
    bool IsSuccess,
    CardsImportData? Data,
    IReadOnlyCollection<OperationErrorResult> Errors);
