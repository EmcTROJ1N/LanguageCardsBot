using Cards.Domain.Enum;
using LanguageCardsBot.Contracts.Common.Exceptions;

namespace Cards.Domain.Exceptions;

/// <summary>
/// Thrown when the system is not in the state required to execute the operation
/// (e.g., modifying a record that has already been deleted or finalized).
/// Maps to gRPC <c>StatusCode.FailedPrecondition</c> (HTTP 400 Bad Request / 412 Precondition Failed).
/// </summary>
public sealed class GrpcFailedPreconditionException(
    string? message = null,
    Exception? innerException = null,
    IReadOnlyDictionary<string, object?>? metadata = null)
    : GrpcException(GrpcError.FailedPrecondition, message, innerException, metadata);
