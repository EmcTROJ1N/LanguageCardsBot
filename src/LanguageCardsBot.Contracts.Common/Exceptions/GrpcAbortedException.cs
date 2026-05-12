using Cards.Domain.Enum;

namespace LanguageCardsBot.Contracts.Common.Exceptions;

/// <summary>
/// Thrown when an operation was aborted due to a concurrency conflict
/// (e.g., an optimistic concurrency violation detected by EF Core during SaveChanges).
/// The caller may safely retry the operation.
/// Maps to gRPC <c>StatusCode.Aborted</c> (HTTP 409 Conflict).
/// </summary>
public sealed class GrpcAbortedException(
    string? message = null,
    Exception? innerException = null,
    IReadOnlyDictionary<string, object?>? metadata = null)
    : GrpcException(GrpcError.Aborted, message, innerException, metadata);
