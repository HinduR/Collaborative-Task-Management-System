
using BoardTaskService.Application.ProjectModule.Contract.IService;
using BoardTaskService.Application.ProjectModule.Dto;
using MediatR;
using Shared.Logging.Contracts;

namespace BoardTaskService.Application.ProjectModule.Query.List;

/// <summary>
/// Represents the query to retrieve all active projects.
/// </summary>
public record GetProjectQuery()
    : IRequest<List<ProjectSummaryDto>>;

/// <summary>
/// Handles retrieval of active projects.
/// </summary>
public class GetProjectQueryHandler
    : IRequestHandler<GetProjectQuery, List<ProjectSummaryDto>>
{
    private readonly IProjectService _projectService;
    private readonly ILoggerManager<GetProjectQueryHandler> _logger;

    public GetProjectQueryHandler(
        IProjectService projectService,
        ILoggerManager<GetProjectQueryHandler> logger)
    {
        _projectService = projectService;
        _logger = logger;
    }

    public async Task<List<ProjectSummaryDto>> Handle(
        GetProjectQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching active projects.");

        List<ProjectSummaryDto> result = await _projectService
            .GetProjects(cancellationToken);

        _logger.LogInformation(
            "Successfully fetched {Count} active projects.",
            result.Count);

        return result;
    }
}