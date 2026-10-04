using Shared.Grpc.Contracts;
using Shared.Grpc.Proto.Identity;
using Shared.Grpc.Proto.Metadata;

namespace Shared.Grpc.Util;

/// <summary>Provides centralized access to the configured gRPC clients used by the application.</summary>
public class GrpcClientManager : IGrpcClientManager
{
    /// <summary>Gets the gRPC client used to communicate with the Identity user service.</summary>
    public IdentityUserService.IdentityUserServiceClient IdentityUserGrpc { get; }

    /// <summary>Gets the gRPC client used to communicate with the Metadata lookup service.</summary>
    public MetadataLookupService.MetadataLookupServiceClient MetadataGrpc { get; }

    /// <summary>Initializes a new instance of the <see cref="GrpcClientManager"/> class.</summary>
    /// <param name="identityUserGrpc">The gRPC client used to communicate with the Identity user service.</param>
    /// <param name="metadataGrpc">The gRPC client used to communicate with the Metadata lookup service.</param>
    public GrpcClientManager(
        IdentityUserService.IdentityUserServiceClient identityUserGrpc,
        MetadataLookupService.MetadataLookupServiceClient metadataGrpc)
    {
        IdentityUserGrpc = identityUserGrpc;
        MetadataGrpc = metadataGrpc;
    }
}