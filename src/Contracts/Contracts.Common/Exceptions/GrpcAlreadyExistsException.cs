using Cards.Domain.Enum;

namespace LanguageCardsBot.Contracts.Common.Exceptions;

/// <summary>
/// Thrown when an attempt to create an entity fails because it already exists
/// and the collision is an unexpected invariant violation rather than a normal flow.
/// Maps to gRPC <c>StatusCode.AlreadyExists</c> (HTTP 409 Conflict).
/// </summary>
public sealed class GrpcAlreadyExistsException(
    string? message = null,
    Exception? innerException = null,
    IReadOnlyDictionary<string, object?>? metadata = null)
    : GrpcException(GrpcError.AlreadyExists, message, innerException, metadata);
