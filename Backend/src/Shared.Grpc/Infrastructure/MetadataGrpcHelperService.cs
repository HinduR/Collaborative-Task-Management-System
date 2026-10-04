using Grpc.Core;
using Shared.Common.Dto;
using Shared.Grpc.Contracts;
using Shared.Grpc.Proto.Metadata;
using Shared.Logging.Contracts;

namespace Shared.Grpc.Infrastructure;

public class MetadataGrpcHelperService
    : IMetadataGrpcHelperService
{
    private readonly IGrpcClientManager _grpcClientManager;
    private readonly ILoggerManager<MetadataGrpcHelperService> _logger;

    public MetadataGrpcHelperService(
        IGrpcClientManager grpcClientManager,
        ILoggerManager<MetadataGrpcHelperService> logger)
    {
        _grpcClientManager = grpcClientManager;
        _logger = logger;
    }

    public async Task<RefTermDto> GetRefTermByIdAsync(
        Guid refTermId,
        CancellationToken cancellationToken = default)
    {
        if (refTermId == Guid.Empty)
        {
            throw new ArgumentException(
                "Ref term ID cannot be empty.",
                nameof(refTermId));
        }

        GetRefTermByIdRequest request = new()
        {
            RefTermId = refTermId.ToString()
        };

        try
        {
            GetRefTermResponse response =
                await _grpcClientManager.MetadataGrpc
                    .GetRefTermByIdAsync(
                        request,
                        cancellationToken:
                            cancellationToken);

            return MapRefTermDto(response);
        }
        catch (RpcException exception)
        {
            _logger.LogError(
                "Metadata gRPC GetRefTermById call failed for {RefTermId}. Status: {StatusCode}. Detail: {Detail}.",
                exception);

            throw;
        }
    }

    public async Task<RefTermDto> GetRefTermByKeyAsync(
        string refTermKey,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refTermKey))
        {
            throw new ArgumentException(
                "Ref term key cannot be empty.",
                nameof(refTermKey));
        }

        GetRefTermByKeyRequest request = new()
        {
            RefTermKey = refTermKey.Trim()
        };

        try
        {
            GetRefTermResponse response =
                await _grpcClientManager.MetadataGrpc
                    .GetRefTermByKeyAsync(
                        request,
                        cancellationToken:
                            cancellationToken);

            return MapRefTermDto(response);
        }
        catch (RpcException exception)
        {
            _logger.LogError(
                "Metadata gRPC GetRefTermByKey call failed for {RefTermKey}. Status: {StatusCode}. Detail: {Detail}.",
                exception);

            throw;
        }
    }

    public async Task<List<RefTermDto>>
        GetRefTermListByRefSetKeyAsync(
            string refSetKey,
            CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refSetKey))
        {
            return [];
        }

        GetRefTermListByRefSetKeyRequest request = new()
        {
            RefSetKey = refSetKey.Trim()
        };

        try
        {
            GetRefTermListResponse response =
                await _grpcClientManager.MetadataGrpc
                    .GetRefTermListByRefSetKeyAsync(
                        request,
                        cancellationToken:
                            cancellationToken);

            return response.RefTermList
                .Select(refTerm => new RefTermDto
                {
                    RefTermId = Guid.Parse(refTerm.Id),
                    RefTermKey = refTerm.RefTermKey,
                    Description = refTerm.Description
                })
                .ToList();
        }
        catch (RpcException exception)
        {
            _logger.LogError(
                "Metadata gRPC GetRefTermListByRefSetKey call failed for {RefSetKey}. Status: {StatusCode}. Detail: {Detail}.",
                exception);

            throw;
        }
    }

    private static RefTermDto MapRefTermDto(
        GetRefTermResponse response)
    {
        return new RefTermDto
        {
            RefTermId = Guid.Parse(response.Id),
            RefTermKey = response.RefTermKey,
            Description = response.Description
        };
    }
}