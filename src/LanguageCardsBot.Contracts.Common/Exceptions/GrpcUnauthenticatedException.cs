using Cards.Domain.Enum;
using LanguageCardsBot.Contracts.Common.Exceptions;

namespace Cards.Domain.Exceptions;

/// <summary>
/// Thrown when a request does not carry valid authentication credentials.
/// Maps to gRPC <c>StatusCode.Unauthenticated</c> (HTTP 401 Unauthorized).
/// Unlike <see cref="GrpcPermissionDeniedException"/>, the caller's identity is unknown
/// or unverifiable.
/// </summary>
public sealed class GrpcUnauthenticatedException(
    string? message = null,
    Exception? innerException = null,
    IReadOnlyDictionary<string, object?>? metadata = null)
    : GrpcException(GrpcError.Unauthenticated, message, innerException, metadata);
