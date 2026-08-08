namespace Cards.Contracts.Rest.Imports;

/// <summary>
/// Represents an operation error returned by the REST API.
/// </summary>
public sealed record OperationErrorDto(
    string Message,
    string? Code,
    string? Target);
