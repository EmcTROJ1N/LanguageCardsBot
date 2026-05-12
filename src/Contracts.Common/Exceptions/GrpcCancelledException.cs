using Cards.Domain.Enum;
using LanguageCardsBot.Contracts.Common.Exceptions;

namespace Cards.Domain.Exceptions;

/// <summary>
/// Thrown when the operation was cancelled, typically via a <see cref="CancellationToken"/>.
/// Maps to gRPC <c>StatusCode.Cancelled</c> (HTTP 499 Client Closed Request).
/// </summary>
public sealed class GrpcCancelledException(
    string? message = null,
    Exception? innerException = null,
    IReadOnlyDictionary<string, object?>? metadata = null)
    : GrpcException(GrpcError.Cancelled, message, innerException, metadata);
