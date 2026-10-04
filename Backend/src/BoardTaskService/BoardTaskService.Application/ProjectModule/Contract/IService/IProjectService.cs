using BoardTaskService.Application.ProjectModule.Dto;

namespace BoardTaskService.Application.ProjectModule.Contract.IService;

/// <summary>
/// Defines application operations for retrieving projects and managing user-project access mappings.
/// </summary>

public interface IProjectService
{
    Task<List<ProjectSummaryDto>> GetProjects(
        CancellationToken cancellationToken);

    Task<List<UserProjectMappingsDto>> GetUserProjectMappings(
        CancellationToken cancellationToken);

    Task<List<ProjectUserMappingDto>> GetProjectUserMappings(
        Guid projectId,
        CancellationToken cancellationToken);

    Task ReplaceUserProjectMappings(
        Guid userId,
        List<Guid> projectIds,
        CancellationToken cancellationToken);
}
