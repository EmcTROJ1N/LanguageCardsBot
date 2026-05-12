using Cards.Domain.Enum;
using LanguageCardsBot.Contracts.Common.Exceptions;

namespace Cards.Domain.Exceptions;

/// <summary>
/// Thrown when the service or a downstream dependency (e.g., the database) is temporarily
/// unavailable. The caller may retry with backoff.
/// Maps to gRPC <c>StatusCode.Unavailable</c> (HTTP 503 Service Unavailable).
/// </summary>
public sealed class GrpcUnavailableException(
    string? message = null,
    Exception? innerException = null,
    IReadOnlyDictionary<string, object?>? metadata = null)
    : GrpcException(GrpcError.Unavailable, message, innerException, metadata);
