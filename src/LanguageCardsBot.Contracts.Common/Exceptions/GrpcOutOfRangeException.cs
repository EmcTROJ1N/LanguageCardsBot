using Cards.Domain.Enum;
using LanguageCardsBot.Contracts.Common.Exceptions;

namespace Cards.Domain.Exceptions;

/// <summary>
/// Thrown when an operation is attempted past a valid range
/// (e.g., a pagination offset beyond the total number of records).
/// Maps to gRPC <c>StatusCode.OutOfRange</c> (HTTP 400 Bad Request).
/// Unlike <see cref="GrpcInvalidArgumentException"/>, this indicates a range boundary
/// violation rather than a structurally invalid value.
/// </summary>
public sealed class GrpcOutOfRangeException(
    string? message = null,
    Exception? innerException = null,
    IReadOnlyDictionary<string, object?>? metadata = null)
    : GrpcException(GrpcError.OutOfRange, message, innerException, metadata);
