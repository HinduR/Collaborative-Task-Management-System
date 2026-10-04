using Microsoft.AspNetCore.Mvc;
using MediatR;
using Shared.Logging.Contracts;
using Swashbuckle.AspNetCore.Annotations;
using BoardTaskService.Application.ProjectModule.Dto;
using BoardTaskService.Application.ProjectModule.Query.List;
using BoardTaskService.Application.ProjectModule.Command.Update;
using Shared.Authorisation.Domain.Attribute;

namespace BoardTaskService.API.Controller.ProjectModule;

/// <summary>
/// Handles project listing, project-user mapping, and project access requests.
/// </summary>
[ApiController]
[Route("project/")]
public class ProjectController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILoggerManager<ProjectController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectController"/> class.
    /// </summary>
    /// <param name="mediator">
    /// Mediator used to dispatch project commands and queries.
    /// </param>
    /// <param name="logger">
    /// Logger used to record project-related API activity.
    /// </param>
    public ProjectController(
        IMediator mediator,
        ILoggerManager<ProjectController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all active projects available for project access mapping.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A list containing the active projects.
    /// </returns>
    [HttpGet("admin/projects")]
    [CustomAuthorize("TEAM_MEMBER_ACCESS")]
    [SwaggerResponse(
        StatusCodes.Status200OK,
        "Active projects returned successfully.",
        typeof(List<ProjectSummaryDto>))]
    public async Task<IActionResult> GetProject(
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Fetching active projects for project access mapping.");

        List<ProjectSummaryDto> projects = await _mediator.Send(
            new GetProjectQuery(),
            cancellationToken);

        _logger.LogDebug(
            "Fetched {ProjectCount} active projects.",
            projects.Count);

        return Ok(projects);
    }

    /// <summary>
    /// Retrieves all active user-project mappings.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A list containing the user-project mappings.
    /// </returns>
    [HttpGet("admin/user-project-mappings")]
    [CustomAuthorize("ADMIN_ACCESS")]
    [SwaggerResponse(
        StatusCodes.Status200OK,
        "User project mappings returned successfully.",
        typeof(List<UserProjectMappingsDto>))]
    public async Task<IActionResult> GetUserProjectMappings(
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Fetching user project mappings.");

        List<UserProjectMappingsDto> mappings = await _mediator.Send(
            new GetUserProjectMappingsQuery(),
            cancellationToken);

        _logger.LogDebug(
            "Fetched {MappingCount} user project mappings.",
            mappings.Count);

        return Ok(mappings);
    }

    /// <summary>
    /// Replaces the complete project access mapping for the specified user.
    /// </summary>
    /// <param name="userId">
    /// The unique identifier of the user whose project mappings are being replaced.
    /// </param>
    /// <param name="request">
    /// The complete list of project identifiers to assign to the user.
    /// An empty list removes all existing project mappings.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A 200 OK response when the mappings are saved successfully.
    /// </returns>
    [HttpPut("admin/users/{user-id}/projects")]
    [CustomAuthorize("ADMIN_ACCESS")]
    [SwaggerResponse(
        StatusCodes.Status200OK,
        "Mappings saved successfully.")]
    public async Task<IActionResult> ReplaceUserProjectMappings(
        [FromRoute(Name = "user-id")] Guid userId,
        [FromBody] ProjectListDto request,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Replacing project mappings for user {UserId}.",
            userId);

        await _mediator.Send(
            new UpdateUserProjectMappingCommand(
                userId,
                request.ProjectIdList),
            cancellationToken);

        _logger.LogDebug(
            "Project mappings replaced successfully for user {UserId}.",
            userId);

        return Ok();
    }

    /// <summary>
    /// Retrieves all active users mapped to the specified project.
    /// </summary>
    /// <param name="projectId">
    /// The unique identifier of the project.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A list containing the users mapped to the specified project.
    /// </returns>
    [HttpGet("{project-id}/user-mappings")]
    [CustomAuthorize("TEAM_MEMBER_ACCESS")]
    [SwaggerResponse(
        StatusCodes.Status200OK,
        "Project user mappings returned successfully.",
        typeof(List<ProjectUserMappingDto>))]
    public async Task<IActionResult> GetProjectUserMappings(
        [FromRoute(Name = "project-id")] Guid projectId,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Fetching users mapped to project {ProjectId}.",
            projectId);

        List<ProjectUserMappingDto> mappings = await _mediator.Send(
            new GetProjectUserMappingQuery(projectId),
            cancellationToken);

        _logger.LogDebug(
            "Fetched {MappingCount} user mappings for project {ProjectId}.",
            mappings.Count,
            projectId);

        return Ok(mappings);
    }
}