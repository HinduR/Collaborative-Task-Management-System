using Shared.Grpc.Proto.Identity;
using Shared.Grpc.Proto.Metadata;
namespace Shared.Grpc.Contracts;

public interface IGrpcClientManager
{
    IdentityUserService.IdentityUserServiceClient IdentityUserGrpc { get; }

    MetadataLookupService.MetadataLookupServiceClient MetadataGrpc { get; }
}