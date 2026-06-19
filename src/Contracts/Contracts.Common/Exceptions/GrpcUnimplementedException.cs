using Cards.Domain.Enum;
using LanguageCardsBot.Contracts.Common.Exceptions;

namespace Cards.Domain.Exceptions;

/// <summary>
/// Thrown when an operation is not implemented or supported by this service.
/// Maps to gRPC <c>StatusCode.Unimplemented</c> (HTTP 501 Not Implemented).
/// </summary>
public sealed class GrpcUnimplementedException(
    string? message = null,
    Exception? innerException = null,
    IReadOnlyDictionary<string, object?>? metadata = null)
    : GrpcException(GrpcError.Unimplemented, message, innerException, metadata);
