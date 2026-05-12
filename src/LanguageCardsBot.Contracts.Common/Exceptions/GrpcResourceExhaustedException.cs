using Cards.Domain.Enum;
using LanguageCardsBot.Contracts.Common.Exceptions;

namespace Cards.Domain.Exceptions;

/// <summary>
/// Thrown when a resource quota or rate limit has been exhausted
/// (e.g., connection pool full, per-user card limit exceeded).
/// Maps to gRPC <c>StatusCode.ResourceExhausted</c> (HTTP 429 Too Many Requests).
/// </summary>
public sealed class GrpcResourceExhaustedException(
    string? message = null,
    Exception? innerException = null,
    IReadOnlyDictionary<string, object?>? metadata = null)
    : GrpcException(GrpcError.ResourceExhausted, message, innerException, metadata);
