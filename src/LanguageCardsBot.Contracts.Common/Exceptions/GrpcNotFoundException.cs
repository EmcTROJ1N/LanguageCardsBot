using Cards.Domain.Enum;
using LanguageCardsBot.Contracts.Common.Exceptions;

namespace Cards.Domain.Exceptions;

/// <summary>
/// Thrown when a requested entity does not exist and the absence is truly exceptional
/// (e.g., a foreign-key reference that must exist by invariant).
/// Maps to gRPC <c>StatusCode.NotFound</c> (HTTP 404 Not Found).
/// For optional lookups, prefer returning an empty response instead of throwing.
/// </summary>
public sealed class GrpcNotFoundException(
    string? message = null,
    Exception? innerException = null,
    IReadOnlyDictionary<string, object?>? metadata = null)
    : GrpcException(GrpcError.NotFound, message, innerException, metadata);
