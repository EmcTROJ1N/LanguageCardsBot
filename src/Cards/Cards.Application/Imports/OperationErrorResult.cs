namespace Cards.Application.Imports;

/// <summary>
/// Represents a user-facing operation error.
/// </summary>
public sealed record OperationErrorResult(
    string Message,
    string? Code = null,
    string? Target = null);
