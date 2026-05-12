using Cards.Domain.Enum;
using LanguageCardsBot.Contracts.Common.Exceptions;

namespace Cards.Domain.Exceptions;

/// <summary>
/// Thrown when an unknown or unclassifiable error occurs.
/// Maps to gRPC <c>StatusCode.Unknown</c> (HTTP 500 Internal Server Error).
/// Use as a last resort when no more specific exception applies.
/// </summary>
public sealed class GrpcUnknownException(
    string? message = null,
    Exception? innerException = null,
    IReadOnlyDictionary<string, object?>? metadata = null)
    : GrpcException(GrpcError.Unknown, message, innerException, metadata);
