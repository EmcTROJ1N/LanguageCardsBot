using Cards.Domain.Enum;
using LanguageCardsBot.Contracts.Common.Exceptions;

namespace Cards.Domain.Exceptions;

/// <summary>
/// Thrown when the caller does not have permission to execute the operation.
/// Maps to gRPC <c>StatusCode.PermissionDenied</c> (HTTP 403 Forbidden).
/// Unlike <see cref="GrpcUnauthenticatedException"/>, the caller's identity is known
/// but lacks the required authorization.
/// </summary>
public sealed class GrpcPermissionDeniedException(
    string? message = null,
    Exception? innerException = null,
    IReadOnlyDictionary<string, object?>? metadata = null)
    : GrpcException(GrpcError.PermissionDenied, message, innerException, metadata);
