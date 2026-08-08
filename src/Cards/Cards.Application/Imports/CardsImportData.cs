namespace Cards.Application.Imports;

/// <summary>
/// Contains counters and row-level errors produced by a card import.
/// </summary>
public sealed record CardsImportData(
    int Imported,
    int Skipped,
    IReadOnlyCollection<string> Errors);
