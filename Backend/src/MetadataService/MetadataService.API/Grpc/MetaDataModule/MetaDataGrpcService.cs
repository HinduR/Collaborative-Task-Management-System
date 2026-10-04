using Grpc.Core;
using MediatR;
using MetadataService.Application.Query;
using Shared.Common.Dto;
using Shared.Exceptions.Infrastructure;
using Shared.Grpc.Proto.Metadata;
using Shared.Logging.Contracts;

namespace MetadataService.API.GrpcServices;

public class MetadataGrpcService
    : MetadataLookupService.MetadataLookupServiceBase
{
    private readonly IMediator _mediator;
    private readonly ILoggerManager<MetadataGrpcService> _logger;

    public MetadataGrpcService(
        IMediator mediator,
        ILoggerManager<MetadataGrpcService> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override async Task<GetRefTermResponse>
        GetRefTermById(
            GetRefTermByIdRequest request,
            ServerCallContext context)
    {
        if (!Guid.TryParse(
                request.RefTermId,
                out Guid refTermId))
        {
            throw new RpcException(
                new Status(
                    StatusCode.InvalidArgument,
                    "Ref term ID must be a valid GUID."));
        }

        try
        {
            _logger.LogDebug(
                "Processing gRPC request to fetch ref term {RefTermId}.",
                refTermId);

            RefTermDto refTerm = await _mediator.Send(
                new GetRefTermByIdQuery(refTermId),
                context.CancellationToken);

            return MapRefTermResponse(refTerm);
        }
        catch (NotFoundCustomException exception)
        {
            throw new RpcException(
                new Status(
                    StatusCode.NotFound,
                    exception.Message));
        }
    }

    public override async Task<GetRefTermResponse>
        GetRefTermByKey(
            GetRefTermByKeyRequest request,
            ServerCallContext context)
    {
        if (string.IsNullOrWhiteSpace(request.RefTermKey))
        {
            throw new RpcException(
                new Status(
                    StatusCode.InvalidArgument,
                    "Ref term key cannot be empty."));
        }

        try
        {
            string refTermKey = request.RefTermKey.Trim();

            _logger.LogDebug(
                "Processing gRPC request to fetch ref term with key {RefTermKey}.",
                refTermKey);

            RefTermDto refTerm = await _mediator.Send(
                new GetRefTermByKeyQuery(refTermKey),
                context.CancellationToken);

            return MapRefTermResponse(refTerm);
        }
        catch (NotFoundCustomException exception)
        {
            throw new RpcException(
                new Status(
                    StatusCode.NotFound,
                    exception.Message));
        }
    }

    public override async Task<GetRefTermListResponse>
        GetRefTermListByRefSetKey(
            GetRefTermListByRefSetKeyRequest request,
            ServerCallContext context)
    {
        if (string.IsNullOrWhiteSpace(request.RefSetKey))
        {
            throw new RpcException(
                new Status(
                    StatusCode.InvalidArgument,
                    "Ref set key cannot be empty."));
        }

        try
        {
            string refSetKey = request.RefSetKey.Trim();

            List<RefTermDto> refTermList =
                await _mediator.Send(
                    new GetRefTermByRefSetQuery(
                        refSetKey),
                    context.CancellationToken);

            GetRefTermListResponse response = new();

            response.RefTermList.AddRange(
                refTermList.Select(MapRefTermItem));

            return response;
        }
        catch (NotFoundCustomException exception)
        {
            throw new RpcException(
                new Status(
                    StatusCode.NotFound,
                    exception.Message));
        }
    }

    private static GetRefTermResponse MapRefTermResponse(
        RefTermDto refTerm)
    {
        return new GetRefTermResponse
        {
            Id = refTerm.RefTermId.ToString(),
            RefTermKey = refTerm.RefTermKey,
            Description = refTerm.Description ?? string.Empty
        };
    }

    private static RefTermItem MapRefTermItem(
        RefTermDto refTerm)
    {
        return new RefTermItem
        {
            Id = refTerm.RefTermId.ToString(),
            RefTermKey = refTerm.RefTermKey,
            Description = refTerm.Description ?? string.Empty
        };
    }
}