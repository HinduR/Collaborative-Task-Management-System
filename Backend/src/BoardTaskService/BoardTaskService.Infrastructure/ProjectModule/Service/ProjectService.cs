using BoardTaskService.Infrastructure.Common;
using BoardTaskService.Application.ProjectModule.Contract.IService;
using BoardTaskService.Application.ProjectModule.Dto;
using BoardTaskService.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Shared.Common.contracts;
using Shared.Exceptions.Infrastructure;
using Shared.Grpc.Common.Dto;
using Shared.Grpc.Contracts;
using Shared.Logging.Contracts;

namespace BoardTaskService.Application.ProjectModule.Service;

/// <summary>Handles project and user-project mapping business operations.</summary>
public class ProjectService : IProjectService
{
    private readonly IRepoWrapper _repoWrapper;
    private readonly IUserContext _userContext;
    private readonly IIdentityGrpcHelperService _identityGrpcHelperService;
    private readonly ILoggerManager<ProjectService> _logger;

    /// <summary>Initializes a new instance of the <see cref="ProjectService"/> class.</summary>
    /// <param name="repoWrapper">Provides access to the required repositories.</param>
    /// <param name="userContext">Provides information about the current authenticated user.</param>
    /// <param name="identityGrpcHelperService">Provides access to user information from the Identity service.</param>
    /// <param name="logger">Logger used to record project operations.</param>
    public ProjectService(
        IRepoWrapper repoWrapper,
        IUserContext userContext,
        IIdentityGrpcHelperService identityGrpcHelperService,
        ILoggerManager<ProjectService> logger)
    {
        _repoWrapper = repoWrapper;
        _userContext = userContext;
        _identityGrpcHelperService = identityGrpcHelperService;
        _logger = logger;
    }

    /// <summary>Retrieves all active projects and indicates which projects are assigned to the current user.</summary>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>A list containing active project summaries and their assignment status.</returns>
    public async Task<List<ProjectSummaryDto>> GetProjects(
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Executing GetProjects.");

        Guid currentUserId =
            _userContext.GetUserId();

        _logger.LogDebug(
            "Fetching active projects for user {UserId}.",
            currentUserId);

        if (currentUserId == Guid.Empty)
        {
            _logger.LogError(
                "Project retrieval failed because the authenticated user ID is unavailable.",
                null);

            throw new UnauthorizedAccessException(
                "Authenticated user ID is unavailable.");
        }

        List<Guid> assignedProjectIds =
            await _repoWrapper
                .UserProjectMappingRepository
                .FindByCondition(mapping =>
                    mapping.IsActive &&
                    mapping.UserId == currentUserId)
                .Select(mapping => mapping.ProjectId)
                .Distinct()
                .ToListAsync(cancellationToken);

        _logger.LogDebug(
            "User {UserId} is assigned to {AssignedProjectCount} active projects.",
            currentUserId,
            assignedProjectIds.Count);

        return await _repoWrapper.ProjectRepository
            .FindByCondition(project =>
                project.IsActive)
            .OrderBy(project => project.Name)
            .Select(project => new ProjectSummaryDto
            {
                Id = project.Id,
                ProjectName = project.Name,

                IsAssigned =
                    assignedProjectIds.Contains(
                        project.Id)
            })
            .ToListAsync(cancellationToken);
    }

    /// <summary>Retrieves all active user-project mappings grouped by user.</summary>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>A list containing each user and their assigned project identifiers.</returns>
    public async Task<List<UserProjectMappingsDto>> GetUserProjectMappings(
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Executing GetUserProjectMappings.");

        List<UserProjectMapping> mappings = await _repoWrapper
            .UserProjectMappingRepository
            .FindByCondition(mapping => mapping.IsActive)
            .ToListAsync(cancellationToken);


        return mappings
            .GroupBy(mapping => mapping.UserId)
            .Select(group => new UserProjectMappingsDto
            {
                UserId = group.Key,
                ProjectIdList = group
                    .Select(mapping => mapping.ProjectId)
                    .ToList()
            })
            .ToList();
    }

    /// <summary>Retrieves the active users mapped to the specified project.</summary>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>A list containing project-user mappings and user display names.</returns>
    public async Task<List<ProjectUserMappingDto>> GetProjectUserMappings(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Executing GetProjectUserMappings.");

        Guid currentUserId = _userContext.GetUserId();

        _logger.LogDebug(
            "Fetching user mappings for project {ProjectId} requested by user {UserId}.",
            projectId,
            currentUserId);

        bool projectExists = await _repoWrapper
            .ProjectRepository
            .AnyByConditionAsync(
                project => project.IsActive && project.Id == projectId,
                cancellationToken);

        if (!projectExists)
        {
            _logger.LogError(
                "Project user mapping retrieval failed because project {ProjectId} was not found.",
                null,
                projectId);

            throw new NotFoundCustomException(
                "The selected project does not exist.",
                "Project not found.");
        }

        bool isAdmin = string.Equals(
            _userContext.GetRoleName(),
            "Admin",
            StringComparison.OrdinalIgnoreCase);

        bool hasProjectAccess = await _repoWrapper
            .UserProjectMappingRepository
            .AnyByConditionAsync(
                mapping =>
                    mapping.IsActive &&
                    mapping.UserId == currentUserId &&
                    mapping.ProjectId == projectId,
                cancellationToken);

        if (!isAdmin && !hasProjectAccess)
        {
            _logger.LogError(
                "Project user mapping retrieval denied because user {UserId} does not have access to project {ProjectId}.",
                null,
                currentUserId,
                projectId);

            throw new ForBiddenCustomException(
                "You do not have access to this project.",
                "Access denied.");
        }

        List<UserProjectMapping> mappings = await _repoWrapper
            .UserProjectMappingRepository
            .FindByCondition(mapping =>
                mapping.IsActive &&
                mapping.ProjectId == projectId)
            .ToListAsync(cancellationToken);

        List<Guid> userIdList = mappings
            .Select(mapping => mapping.UserId)
            .Distinct()
            .ToList();

        List<UserDto> users = await _identityGrpcHelperService
            .GetUserListAsync(
                userIdList,
                cancellationToken);

        Dictionary<Guid, string> userNamesById = users
            .ToDictionary(
                user => user.Id,
                user => user.DisplayName);

        return mappings
            .Select(mapping => new ProjectUserMappingDto
            {
                Id = mapping.Id,
                UserId = mapping.UserId,
                UserName = userNamesById.GetValueOrDefault(
                    mapping.UserId,
                    string.Empty)
            })
            .OrderBy(mapping => mapping.UserName)
            .ToList();
    }

    /// <summary>Replaces all project mappings for the specified user with the supplied project identifiers.</summary>
    /// <param name="userId">The unique identifier of the user whose mappings are being replaced.</param>
    /// <param name="projectIds">The project identifiers that should remain assigned to the user.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    public async Task ReplaceUserProjectMappings(
        Guid userId,
        List<Guid> projectIds,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Executing ReplaceUserProjectMappings.");

        Guid currentUserId = _userContext.GetUserId();

        _logger.LogDebug(
            "Replacing project mappings for user {UserId} requested by user {UpdatedBy}.",
            userId,
            currentUserId);

        foreach (Guid projectId in projectIds)
        {
            bool projectExists = await _repoWrapper
                .ProjectRepository
                .AnyByConditionAsync(
                    project => project.IsActive && project.Id == projectId,
                    cancellationToken);

            if (!projectExists)
            {
                _logger.LogError(
                    "Project mapping update failed because project {ProjectId} was not found.",
                    null,
                    projectId);

                throw new NotFoundCustomException(
                    "One or more selected projects do not exist.",
                    "Project not found.");
            }
        }

        HashSet<Guid> requestedProjectIdList = projectIds.ToHashSet();

        List<UserProjectMapping> existingMappings = await _repoWrapper
            .UserProjectMappingRepository
            .FindByCondition(mapping => mapping.UserId == userId)
            .ToListAsync(cancellationToken);

        HashSet<Guid> existingProjectIds = new(existingMappings.Count);

        foreach (UserProjectMapping mapping in existingMappings)
        {
            mapping.IsActive = requestedProjectIdList.Contains(mapping.ProjectId);
            existingProjectIds.Add(mapping.ProjectId);
        }

        List<UserProjectMapping> newMappings = requestedProjectIdList
            .Where(projectId => !existingProjectIds.Contains(projectId))
            .Select(projectId => new UserProjectMapping
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ProjectId = projectId,
                IsActive = true
            })
            .ToList();

        if (existingMappings.Any())
        {
            _repoWrapper.UserProjectMappingRepository
                .UpdateRange(existingMappings);
        }

        if (newMappings.Any())
        {
            await _repoWrapper.UserProjectMappingRepository
                .CreateRangeAsync(newMappings, cancellationToken);
        }

        await _repoWrapper.SaveChangesAsync(cancellationToken);

        _logger.LogDebug(
            "Project mappings updated for user {UserId} by {UpdatedBy}. Requested mappings: {RequestedCount}, new mappings: {NewMappingCount}.",
            userId,
            _userContext.GetUserId(),
            requestedProjectIdList.Count,
            newMappings.Count);
    }
}
