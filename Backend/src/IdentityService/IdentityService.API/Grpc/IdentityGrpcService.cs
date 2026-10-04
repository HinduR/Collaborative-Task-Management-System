using Grpc.Core;
using IdentityService.Application.AuthenticationModule.Query.Validate;
using IdentityService.Application.UserModule.Contract.IService;
using IdentityService.Application.UserModule.Dto;
using IdentityService.Application.UserModule.Query.Get;
using MediatR;
using Shared.Exceptions.Infrastructure;
using Shared.Grpc.Proto.Identity;
using Shared.Logging.Contracts;
using IdentityGrpc = Shared.Grpc.Proto.Identity.IdentityUserService;

namespace IdentityService.API.Grpc;

public class IdentityUserGrpcService : IdentityGrpc.IdentityUserServiceBase
{
    private readonly IUserService _userService;
    private readonly ILoggerManager<IdentityUserGrpcService> _logger;
    private readonly IMediator _mediator;


    public IdentityUserGrpcService(
        IUserService userService,
        ILoggerManager<IdentityUserGrpcService> logger,
                IMediator mediator)
    {
        _userService = userService;
        _logger = logger;
        _mediator = mediator;
    }

    public override async Task<GetUserListResponse> GetUserList(
        GetUserListRequest request,
        ServerCallContext context)
    {
        List<Guid> userIdList = [];

        foreach (string userIdText in request.UserIdList.Distinct())
        {
            if (!Guid.TryParse(userIdText, out Guid userId))
            {
                throw new RpcException(
                    new Status(
                        StatusCode.InvalidArgument,
                        $"Invalid user id: {userIdText}"));
            }

            userIdList.Add(userId);
        }

        List<UserListItemDto> userList = await _userService.GetUserListByIdAsync(
            userIdList,
            context.CancellationToken);

        GetUserListResponse response = new();

        response.UserList.AddRange(
            userList.Select(user => new UserItem
            {
                Id = user.Id.ToString(),
                Email = user.Email,
                DisplayName = user.DisplayName
            }));

        return response;
    }

    public override async Task<ValidateAccessTokenResponse>
    ValidateAccessToken(
        ValidateAccessTokenRequest request,
        ServerCallContext context)
    {
        if (string.IsNullOrWhiteSpace(request.AccessToken))
        {
            return new ValidateAccessTokenResponse
            {
                IsValid = false
            };
        }

        try
        {
            bool isValid = await _mediator.Send(
                new ValidateAccessTokenQuery(request.AccessToken),
                context.CancellationToken);

            _logger.LogDebug(
                "gRPC access-token validation completed. IsValid: {IsValid}",
                isValid);

            return new ValidateAccessTokenResponse
            {
                IsValid = isValid
            };
        }
        catch (Exception exception)
        {
            _logger.LogError(
                "gRPC access-token validation failed: {Message}",
                exception);

            throw new RpcException(
                new Status(
                    StatusCode.Internal,
                    "Identity token validation could not be completed."));
        }
    }

    public override async Task<GetUserByIdResponse> GetUserById(
    GetUserByIdRequest request,
    ServerCallContext context)
{
    if (!Guid.TryParse(request.UserId, out Guid userId))
    {
        throw new RpcException(
            new Status(
                StatusCode.InvalidArgument,
                "User ID must be a valid GUID."));
    }

    try
    {
        _logger.LogInformation(
            "Processing gRPC request for user {UserId}.",
            userId);

        AuthenticatedUserDto user = await _mediator.Send(
            new GetUserByIdQuery(userId),
            context.CancellationToken);

        return new GetUserByIdResponse
        {
            Id = user.Id.ToString(),
            Email = user.Email,
            DisplayName = user.UserName
        };
    }
    catch (NotFoundCustomException exception)
    {
        throw new RpcException(
            new Status(
                StatusCode.NotFound,
                exception.Message));
    }
}
}