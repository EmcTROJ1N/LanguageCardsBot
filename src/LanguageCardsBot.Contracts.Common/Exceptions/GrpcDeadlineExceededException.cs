using Cards.Domain.Enum;
using LanguageCardsBot.Contracts.Common.Exceptions;

namespace Cards.Domain.Exceptions;

/// <summary>
/// Thrown when an operation deadline expired before it could complete.
/// Maps to gRPC <c>StatusCode.DeadlineExceeded</c> (HTTP 504 Gateway Timeout).
/// </summary>
public sealed class GrpcDeadlineExceededException(
    string? message = null,
    Exception? innerException = null,
    IReadOnlyDictionary<string, object?>? metadata = null)
    : GrpcException(GrpcError.DeadlineExceeded, message, innerException, metadata);
