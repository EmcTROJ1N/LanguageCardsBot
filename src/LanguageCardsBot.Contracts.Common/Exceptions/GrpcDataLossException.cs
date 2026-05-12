using Cards.Domain.Enum;
using LanguageCardsBot.Contracts.Common.Exceptions;

namespace Cards.Domain.Exceptions;

/// <summary>
/// Thrown when unrecoverable data loss or corruption is detected
/// (e.g., a write was acknowledged but subsequent reads return unexpected results).
/// Maps to gRPC <c>StatusCode.DataLoss</c> (HTTP 500 Internal Server Error).
/// </summary>
public sealed class GrpcDataLossException(
    string? message = null,
    Exception? innerException = null,
    IReadOnlyDictionary<string, object?>? metadata = null)
    : GrpcException(GrpcError.DataLoss, message, innerException, metadata);
