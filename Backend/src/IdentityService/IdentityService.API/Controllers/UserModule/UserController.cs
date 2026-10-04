using IdentityService.Application.UserModule.Command.AssignRole;
using IdentityService.Application.UserModule.Dto;
using IdentityService.Application.UserModule.Query.Get;
using IdentityService.Application.UserModule.Query.List;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Logging.Contracts;
using Swashbuckle.AspNetCore.Annotations;

namespace IdentityService.API.Controllers.UserModule;

[ApiController]
[Route("identity")]
/// <summary>
/// Handles requests related to user retrieval and role assignment.
/// </summary>
public class UserController : ControllerBase
{
    private readonly ILoggerManager<UserController> _logger;

    private readonly IMediator _mediator;

    public UserController(IMediator mediator, ILoggerManager<UserController> logger)
    {
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves a list of users, optionally filtered by a search term.
    /// </summary>
    /// <param name="search">An optional search term to filter users by name or email.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>A list of users matching the search criteria.</returns>
    [HttpGet("users/list")]
    [SwaggerResponse(StatusCodes.Status200OK, "Users returned successfully.", typeof(UserListResponseDto))]
    [SwaggerResponse(StatusCodes.Status204NoContent, "No Users")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error.")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
           "Fetching users with search term '{Search}'.",
           search);
        UserListResponseDto result = await _mediator.Send(
            new GetUsersQuery(search),
            cancellationToken);
        _logger.LogDebug(
                "User list fetch completed for search term '{Search}'.",
                search);

        return Ok(result);
    }

  /// <summary>
/// Retrieves the specified user, or the currently authenticated user
/// when no user ID is provided.
/// </summary>
[HttpGet("user/details")]
[SwaggerResponse(
    StatusCodes.Status200OK,
    "User returned successfully.",
    typeof(AuthenticatedUserDto))]
[SwaggerResponse(
    StatusCodes.Status401Unauthorized,
    "The current user could not be identified.")]
[SwaggerResponse(
    StatusCodes.Status404NotFound,
    "The specified user does not exist.")]
[SwaggerResponse(
    StatusCodes.Status500InternalServerError,
    "Internal server error.")]
public async Task<IActionResult> GetUser(
    [FromQuery] Guid? userId,
    CancellationToken cancellationToken)
{
    _logger.LogDebug(
        "Fetching user. Requested user ID: {UserId}.",
        userId);

    AuthenticatedUserDto result = await _mediator.Send(
        new GetUserByIdQuery(userId),
        cancellationToken);

    _logger.LogDebug(
        "User fetch completed. Requested user ID: {UserId}.",
        userId);

    return Ok(result);
}

    /// <summary>
    /// Assigns a role to the specified user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user being assigned a role.</param>
    /// <param name="request">The role assignment details.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>The resulting user-role mapping.</returns>
    [HttpPut("users/{user-id}/role")]
    [SwaggerResponse(StatusCodes.Status200OK, "Role assigned successfully.", typeof(UserRoleMappingResponseDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid role assignment request.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "The specified user or role does not exist.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error.")]
    public async Task<ActionResult<UserRoleMappingResponseDto>> AssignRole([FromRoute(Name = "user-id")]
            Guid userId, [FromBody] AssignUserRoleRequestDto request, CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Assigning role to user {UserId}.",
            userId);
        UserRoleMappingResponseDto response =
            await _mediator.Send(
                new AssignUserRoleCommand(
                    userId,
                    request),
                cancellationToken);

        return Ok(response);
    }
}
