using Cards.Domain.Enum;
using Grpc.Core;
using Grpc.Core.Interceptors;
using LanguageCardsBot.Contracts.Common.Exceptions;

namespace Cards.Presentation.Interceptors;

/// <summary>
/// Server-side gRPC interceptor that catches <see cref="GrpcException"/> thrown by repositories
/// and services, and converts them into <see cref="RpcException"/> with the appropriate
/// <see cref="StatusCode"/>. This keeps gRPC transport concerns out of the domain and
/// infrastructure layers.
/// </summary>
public sealed class GrpcExceptionInterceptor : Interceptor
{
    /// <inheritdoc />
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (GrpcException ex)
        {
            throw new RpcException(new Status(Map(ex.Code), ex.Message));
        }
    }

    /// <summary>
    /// Maps a <see cref="GrpcError"/> domain code to the corresponding gRPC <see cref="StatusCode"/>.
    /// </summary>
    private static StatusCode Map(GrpcError error) => error switch
    {
        GrpcError.Ok => StatusCode.OK,
        GrpcError.Cancelled => StatusCode.Cancelled,
        GrpcError.Unknown => StatusCode.Unknown,
        GrpcError.InvalidArgument => StatusCode.InvalidArgument,
        GrpcError.DeadlineExceeded => StatusCode.DeadlineExceeded,
        GrpcError.NotFound => StatusCode.NotFound,
        GrpcError.AlreadyExists => StatusCode.AlreadyExists,
        GrpcError.PermissionDenied => StatusCode.PermissionDenied,
        GrpcError.ResourceExhausted => StatusCode.ResourceExhausted,
        GrpcError.FailedPrecondition => StatusCode.FailedPrecondition,
        GrpcError.Aborted => StatusCode.Aborted,
        GrpcError.OutOfRange => StatusCode.OutOfRange,
        GrpcError.Unimplemented => StatusCode.Unimplemented,
        GrpcError.Internal => StatusCode.Internal,
        GrpcError.Unavailable => StatusCode.Unavailable,
        GrpcError.DataLoss => StatusCode.DataLoss,
        GrpcError.Unauthenticated => StatusCode.Unauthenticated,
        _ => StatusCode.Unknown
    };
}
