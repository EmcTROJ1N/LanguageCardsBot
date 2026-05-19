using Cards.Application.Imports;

namespace Cards.Presentation.Contracts;

/// <summary>
/// Represents the REST request body for importing cards from JSON.
/// </summary>
public sealed class ImportCardsFromJsonRequestDto
{
    public string Json { get; init; } = string.Empty;
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

/// <summary>
/// Provides mapping helpers for REST import DTOs.
/// </summary>
internal static partial class ApiMappingExtensions
{
    /// <summary>
    /// Converts an application import result to a REST DTO.
    /// </summary>
    public static ImportCardsFromJsonResponseDto ToDto(this ImportCardsFromJsonResult result)
    {
        return new ImportCardsFromJsonResponseDto(
            result.IsSuccess,
            result.Data is null
                ? null
                : new CardsImportDataDto(
                    result.Data.Imported,
                    result.Data.Skipped,
                    result.Data.Errors),
            result.Errors
                .Select(x => new OperationErrorDto(x.Message, x.Code, x.Target))
                .ToList());
    }
}
