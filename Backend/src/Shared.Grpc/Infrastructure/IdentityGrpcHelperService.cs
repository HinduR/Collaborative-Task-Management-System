using Grpc.Core;
using Shared.Grpc.Common.Dto;
using Shared.Grpc.Contracts;
using Shared.Grpc.Proto.Identity;
using Shared.Logging.Contracts;

namespace Shared.Grpc.Infrastructure;

public class IdentityGrpcHelperService : IIdentityGrpcHelperService
{
    private readonly IGrpcClientManager _grpcClientManager;
    private readonly ILoggerManager<IdentityGrpcHelperService> _logger;

    public IdentityGrpcHelperService(
        IGrpcClientManager grpcClientManager,
        ILoggerManager<IdentityGrpcHelperService> logger)
    {
        _grpcClientManager = grpcClientManager;
        _logger = logger;
    }

    public async Task<List<UserDto>> GetUserListAsync(
        List<Guid> userIdList,
        CancellationToken cancellationToken = default)
    {
        if (!userIdList.Any())
        {
            return [];
        }

        GetUserListRequest request = new();

        request.UserIdList.AddRange(
            userIdList
                .Distinct()
                .Select(userId => userId.ToString()));

        try
        {
            GetUserListResponse response = await _grpcClientManager
                .IdentityUserGrpc
                .GetUserListAsync(
                    request,
                    cancellationToken: cancellationToken);

            return response.UserList
                .Select(user => new UserDto
                {
                    Id = Guid.Parse(user.Id),
                    Email = user.Email,
                    DisplayName = user.DisplayName
                })
                .ToList();
        }
        catch (RpcException exception)
        {
            _logger.LogError(
                "Identity gRPC call failed. Status code: {StatusCode}. Detail: {Detail}", exception);

            throw;
        }
    }
    public async Task<bool> ValidateAccessTokenAsync(
       string accessToken,
       CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return false;
        }

        try
        {
            ValidateAccessTokenResponse response =
                await _grpcClientManager
                    .IdentityUserGrpc
                    .ValidateAccessTokenAsync(
                        new ValidateAccessTokenRequest
                        {
                            AccessToken = accessToken
                        },
                        cancellationToken: cancellationToken);

            return response.IsValid;
        }
        catch (RpcException exception)
        {
            _logger.LogError(
                "Identity gRPC token-validation call failed. Status: {StatusCode}. Detail: {Detail}",
                exception,
                exception.Status.Detail);

            throw;
        }
    }

    public async Task<UserDto> GetUserByIdAsync(
    Guid userId,
    CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User ID cannot be empty.",
                nameof(userId));
        }

        GetUserByIdRequest request = new()
        {
            UserId = userId.ToString()
        };

        try
        {
            GetUserByIdResponse response =
                await _grpcClientManager.IdentityUserGrpc
                    .GetUserByIdAsync(
                        request,
                        cancellationToken: cancellationToken);

            return new UserDto
            {
                Id = Guid.Parse(response.Id),
                Email = response.Email,
                DisplayName = response.DisplayName
            };
        }
        catch (RpcException exception)
        {
            _logger.LogError(
                $"Identity gRPC GetUserById failed. " +
                $"UserId: {userId}. " +
                $"StatusCode: {exception.StatusCode}. " +
                $"Detail: {exception.Status.Detail}");

            throw;
        }
    }
}