namespace Cards.Presentation.Contracts;

/// <summary>
/// Represents the REST request body for importing cards from JSON.
/// </summary>
public sealed class ImportCardsFromJsonRequestDto
{
    /// <summary>Gets the raw JSON payload with cards to import.</summary>
    public string Json { get; init; } = string.Empty;

    /// <summary>Gets the identifier of the user who will own the imported cards.</summary>
    public int UserId { get; init; }
}

/// <summary>
/// Represents an operation error returned by the REST API.
/// </summary>
public sealed record OperationErrorDto(
    string Message,
    string? Code,
    string? Target);

/// <summary>
/// Contains import counters and row-level import errors for REST responses.
/// </summary>
public sealed record CardsImportDataDto(
    int Imported,
    int Skipped,
    IReadOnlyCollection<string> Errors);

/// <summary>
/// Represents the REST response for importing cards from JSON.
/// </summary>
public sealed record ImportCardsFromJsonResponseDto(
    bool IsSuccess,
    CardsImportDataDto? Data,
    IReadOnlyCollection<OperationErrorDto> Errors);
