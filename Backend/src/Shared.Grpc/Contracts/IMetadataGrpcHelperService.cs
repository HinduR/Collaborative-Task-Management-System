
using Shared.Common.Dto;

namespace Shared.Grpc.Contracts;

public interface IMetadataGrpcHelperService
{
    Task<RefTermDto> GetRefTermByIdAsync(
        Guid refTermId,
        CancellationToken cancellationToken = default);

    Task<RefTermDto> GetRefTermByKeyAsync(
        string refTermKey,
        CancellationToken cancellationToken = default);

    Task<List<RefTermDto>>
        GetRefTermListByRefSetKeyAsync(
            string refSetKey,
            CancellationToken cancellationToken = default);
}