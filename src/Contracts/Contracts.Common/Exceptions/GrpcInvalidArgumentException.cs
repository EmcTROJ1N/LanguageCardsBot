using Cards.Domain.Enum;
using LanguageCardsBot.Contracts.Common.Exceptions;

namespace Cards.Domain.Exceptions;

/// <summary>
/// Thrown when the caller passes an invalid argument that is independent of system state.
/// Maps to gRPC <c>StatusCode.InvalidArgument</c> (HTTP 400 Bad Request).
/// Use when input validation fails at the domain or repository boundary.
/// </summary>
public sealed class GrpcInvalidArgumentException(
    string? message = null,
    Exception? innerException = null,
    IReadOnlyDictionary<string, object?>? metadata = null)
    : GrpcException(GrpcError.InvalidArgument, message, innerException, metadata);
