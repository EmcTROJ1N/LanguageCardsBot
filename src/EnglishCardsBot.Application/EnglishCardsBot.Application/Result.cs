namespace EnglishCardsBot.Application;

public sealed record OperationError(
    string Message,
    string Code = "",
    string? Target = null
);

public sealed class Result<T>
{
    public bool IsSuccess { get; init; }
    public T? Data { get; init; }
    public IReadOnlyCollection<OperationError> Errors { get; init; } = [];

    public static Result<T> Success(
        T data)
        => new()
        {
            IsSuccess = true,
            Data = data,
        };

    public static Result<T> Failure(
        params OperationError[] errors)
        => new()
        {
            IsSuccess = false,
            Errors = errors
        };
}

/*public sealed class Result
{
    public bool IsSuccess { get; init; }
    public IReadOnlyCollection<OperationError> Errors { get; init; } = [];

    public static Result Success() => new() { IsSuccess = true, };

    public static Result Failure(params OperationError[] errors) => new()
        {
            IsSuccess = false,
            Errors = errors
        };
}*/