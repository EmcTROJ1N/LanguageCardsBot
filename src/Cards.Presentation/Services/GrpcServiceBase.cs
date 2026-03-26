using Grpc.Core;

namespace Cards.Presentation.Services;

internal abstract class GrpcServiceBase
{
    internal static RpcException CreateUnimplementedException(string methodName)
    {
        return new RpcException(new Status(StatusCode.Unimplemented, $"{methodName} is not implemented yet."));
    }
}
