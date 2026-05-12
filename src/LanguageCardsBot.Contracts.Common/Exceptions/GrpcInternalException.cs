using Cards.Domain.Enum;
using LanguageCardsBot.Contracts.Common.Exceptions;

namespace Cards.Domain.Exceptions;

/// <summary>
/// Thrown when an unexpected internal error occurs, typically wrapping a database
/// or infrastructure failure (e.g., <see cref="Microsoft.EntityFrameworkCore.DbUpdateException"/>).
/// Maps to gRPC <c>StatusCode.Internal</c> (HTTP 500 Internal Server Error).
/// </summary>
public sealed class GrpcInternalException(
    string? message = null,
    Exception? innerException = null,
    IReadOnlyDictionary<string, object?>? metadata = null)
    : GrpcException(GrpcError.Internal, message, innerException, metadata);
