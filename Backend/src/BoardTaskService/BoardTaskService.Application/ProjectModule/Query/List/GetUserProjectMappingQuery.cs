using BoardTaskService.Application.ProjectModule.Contract.IService;
using BoardTaskService.Application.ProjectModule.Dto;
using MediatR;
using Shared.Logging.Contracts;

namespace BoardTaskService.Application.ProjectModule.Query.List;

/// <summary>
/// Represents the query to retrieve active project mappings grouped by user.
/// </summary>
public record GetUserProjectMappingsQuery()
    : IRequest<List<UserProjectMappingsDto>>;

/// <summary>
/// Handles retrieval of active user-project mappings.
/// </summary>
public class GetUserProjectMappingsQueryHandler
    : IRequestHandler<GetUserProjectMappingsQuery, List<UserProjectMappingsDto>>
{
    private readonly IProjectService _projectService;
    private readonly ILoggerManager<GetUserProjectMappingsQueryHandler> _logger;

    public GetUserProjectMappingsQueryHandler(
        IProjectService projectService,
        ILoggerManager<GetUserProjectMappingsQueryHandler> logger)
    {
        _projectService = projectService;
        _logger = logger;
    }

    public async Task<List<UserProjectMappingsDto>> Handle(
        GetUserProjectMappingsQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching user project mappings.");

        List<UserProjectMappingsDto> result = await _projectService
            .GetUserProjectMappings(cancellationToken);

        _logger.LogInformation(
            "Successfully fetched project mappings for {Count} users.",
            result.Count);

        return result;
    }
}