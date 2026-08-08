namespace Cards.Contracts.Rest.Imports;

/// <summary>
/// Contains import counters and row-level import errors for REST responses.
/// </summary>
public sealed record CardsImportDataDto(
    int Imported,
    int Skipped,
    IReadOnlyCollection<string> Errors);
