namespace Cards.Application.Imports;

/// <summary>
/// Represents the result of importing cards from JSON.
/// </summary>
public sealed record ImportCardsFromJsonResult(
    bool IsSuccess,
    CardsImportData? Data,
    IReadOnlyCollection<OperationErrorResult> Errors);

/// <summary>
/// Contains counters and row-level errors produced by a card import.
/// </summary>
public sealed record CardsImportData(
    int Imported,
    int Skipped,
    IReadOnlyCollection<string> Errors);

/// <summary>
/// Represents a user-facing operation error.
/// </summary>
public sealed record OperationErrorResult(
    string Message,
    string? Code = null,
    string? Target = null);
