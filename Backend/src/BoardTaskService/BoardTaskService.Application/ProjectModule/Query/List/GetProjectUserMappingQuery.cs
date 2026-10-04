using BoardTaskService.Application.ProjectModule.Contract.IService;
using BoardTaskService.Application.ProjectModule.Dto;
using FluentValidation;
using MediatR;
using Shared.Logging.Contracts;

namespace BoardTaskService.Application.ProjectModule.Query.List;

/// <summary>
/// Represents the query to retrieve users mapped to a project.
/// </summary>
public record GetProjectUserMappingQuery(Guid ProjectId)
    : IRequest<List<ProjectUserMappingDto>>;

/// <summary>
/// Validates the project user mapping query.
/// </summary>
public class GetProjectUserMappingQueryValidator
    : AbstractValidator<GetProjectUserMappingQuery>
{
    public GetProjectUserMappingQueryValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage("ProjectId cannot be empty.");
    }
}

/// <summary>
/// Handles retrieval of users mapped to a project.
/// </summary>
public class GetProjectUserMappingQueryHandler
    : IRequestHandler<GetProjectUserMappingQuery, List<ProjectUserMappingDto>>
{
    private readonly IProjectService _projectService;
    private readonly ILoggerManager<GetProjectUserMappingQueryHandler> _logger;

    public GetProjectUserMappingQueryHandler(
        IProjectService projectService,
        ILoggerManager<GetProjectUserMappingQueryHandler> logger)
    {
        _projectService = projectService;
        _logger = logger;
    }

    public async Task<List<ProjectUserMappingDto>> Handle(
        GetProjectUserMappingQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Fetching users mapped to project {ProjectId}.",
            request.ProjectId);

        List<ProjectUserMappingDto> result = await _projectService.GetProjectUserMappings(request.ProjectId, cancellationToken);

        _logger.LogInformation(
            "Successfully fetched {Count} users mapped to project {ProjectId}.",
            result.Count,
            request.ProjectId);

        return result;
    }
}
