using Shared.Grpc.Common.Dto;

namespace Shared.Grpc.Contracts;

/// <summary>Defines helper operations for communicating with the Identity service through gRPC.</summary>
public interface IIdentityGrpcHelperService
{
    /// <summary>Retrieves the users matching the supplied user identifiers.</summary>
    /// <param name="userIdList">The user identifiers to retrieve.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>A list containing the matching users.</returns>
    Task<List<UserDto>> GetUserListAsync(
        List<Guid> userIdList,
        CancellationToken cancellationToken = default);

    /// <summary>Validates the supplied access token through the Identity service.</summary>
    /// <param name="accessToken">The access token to validate.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns><see langword="true"/> when the access token is valid; otherwise, <see langword="false"/>.</returns>
    Task<bool> ValidateAccessTokenAsync(
        string accessToken,
        CancellationToken cancellationToken = default);

    /// <summary>Retrieves a user by their unique identifier.</summary>
    /// <param name="userId">The unique identifier of the user to retrieve.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>The matching user.</returns>
    Task<UserDto> GetUserByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}