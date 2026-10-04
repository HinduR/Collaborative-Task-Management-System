using IdentityService.Application.RoleModule.Dto;
using IdentityService.Application.RoleModule.Query.Get;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Authorisation.Domain.Attribute;
using Shared.Logging.Contracts;
using Swashbuckle.AspNetCore.Annotations;

namespace IdentityService.API.Controllers.RoleModule;

/// <summary>
/// Handles requests related to retrieving system roles.
/// </summary>
[ApiController]
[Route("identity/role")]
/// <summary>
/// Represents the RoleController component.
/// </summary>
public class RoleController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILoggerManager<RoleController> _logger;

    public RoleController(
        IMediator mediator,
        ILoggerManager<RoleController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all active roles in the system.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>A list of active roles.</returns>
    [HttpGet]
    [CustomAuthorize("TEAM_MEMBER_ACCESS")]
    [SwaggerResponse(StatusCodes.Status200OK, "Active roles returned successfully.", typeof(List<RoleDto>))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error.")]
    public async Task<IActionResult> GetRoles(
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Received request to fetch active roles.");

        List<RoleDto> response = await _mediator.Send(
            new GetRolesQuery(),
            cancellationToken);
        _logger.LogDebug(
                   "Active roles fetch completed. {Count} roles returned.",
                   response.Count);
        return Ok(response);
    }
}
