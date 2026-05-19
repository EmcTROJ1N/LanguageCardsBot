namespace Cards.Application.Imports;

public sealed record ImportCardsFromJsonResult(
    bool IsSuccess,
    CardsImportData? Data,
    IReadOnlyCollection<OperationErrorResult> Errors);

public sealed record CardsImportData(
    int Imported,
    int Skipped,
    IReadOnlyCollection<string> Errors);

public sealed record OperationErrorResult(
    string Message,
    string? Code = null,
    string? Target = null);
