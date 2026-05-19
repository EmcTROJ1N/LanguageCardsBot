using Cards.Application.Imports;

namespace Cards.Presentation.Contracts;

public sealed class ImportCardsFromJsonRequestDto
{
    public string Json { get; init; } = string.Empty;
    public int UserId { get; init; }
}

public sealed record OperationErrorDto(
    string Message,
    string? Code,
    string? Target);

public sealed record CardsImportDataDto(
    int Imported,
    int Skipped,
    IReadOnlyCollection<string> Errors);

public sealed record ImportCardsFromJsonResponseDto(
    bool IsSuccess,
    CardsImportDataDto? Data,
    IReadOnlyCollection<OperationErrorDto> Errors);

internal static partial class ApiMappingExtensions
{
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
