using Cards.Domain.Enum;

namespace LanguageCardsBot.Contracts.Common.Exceptions;

public class GrpcException(
    GrpcError code,
    string? message = null,
    Exception? innerException = null,
    IReadOnlyDictionary<string, object?>? metadata = null)
    : Exception(message ?? code.ToString(), innerException)
{
    public GrpcError Code { get; } = code;
    public IReadOnlyDictionary<string, object?> Metadata { get; } = metadata ?? new Dictionary<string, object?>();

    public override string ToString()
    {
        return $"{nameof(GrpcException)}: " +
               $"Code={Code}, " +
               $"Message={Message}";
    }
}
