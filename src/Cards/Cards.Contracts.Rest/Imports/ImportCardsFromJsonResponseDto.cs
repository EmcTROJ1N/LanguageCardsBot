namespace Cards.Contracts.Rest.Imports;

/// <summary>
/// Represents the REST response for importing cards from JSON.
/// </summary>
public sealed record ImportCardsFromJsonResponseDto(
    bool IsSuccess,
    CardsImportDataDto? Data,
    IReadOnlyCollection<OperationErrorDto> Errors);
