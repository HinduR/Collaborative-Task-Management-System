using System.Linq.Expressions;
using BoardTaskService.Infrastructure.Common;
using BoardTaskService.Application.TaskModule.Contract.IService;
using BoardTaskService.Application.TaskModule.Dto;
using BoardTaskService.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Shared.Common.Dto;
using Shared.Grpc.Common.Dto;
using Shared.Grpc.Contracts;
using Shared.Logging.Contracts;

namespace BoardTaskService.Infrastructure.TaskModule.Service;

/// <summary>
/// Represents the TaskQueryService component.
/// </summary>
public class TaskQueryService : ITaskQueryService
{
    private readonly IRepoWrapper _repoWrapper;
    private readonly IMetadataGrpcHelperService _metadataGrpcHelperService;
    private readonly IIdentityGrpcHelperService _identityGrpcHelperService;
    private readonly ILoggerManager<TaskQueryService> _logger;

    public TaskQueryService(
        IRepoWrapper repoWrapper,
        IMetadataGrpcHelperService metadataGrpcHelperService,
        IIdentityGrpcHelperService identityGrpcHelperService,
        ILoggerManager<TaskQueryService> logger)
    {
        _repoWrapper = repoWrapper;
        _metadataGrpcHelperService = metadataGrpcHelperService;
        _identityGrpcHelperService = identityGrpcHelperService;
        _logger = logger;
    }

    public async Task<TaskQueryResponseDto> QueryTasksAsync(
        Guid currentUserId,
        Expression<Func<BoardTask, bool>> conditionExpression,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Fetching accessible projects for user {UserId}.",
            currentUserId);

        List<Guid> projectIds = await _repoWrapper
            .UserProjectMappingRepository
            .FindByCondition(mapping =>
                mapping.IsActive &&
                mapping.UserId == currentUserId)
            .Select(mapping => mapping.ProjectId)
            .ToListAsync(cancellationToken);

        if (projectIds.Count == 0)
        {
            return EmptyResponse();
        }

        List<Guid> boardIds = await _repoWrapper
            .BoardRepository
            .FindByCondition(board =>
                board.IsActive &&
                projectIds.Contains(board.ProjectId))
            .Select(board => board.Id)
            .ToListAsync(cancellationToken);

        if (boardIds.Count == 0)
        {
            return EmptyResponse();
        }

        List<Guid> workflowColumnIds = await _repoWrapper
            .WorkflowColumnRepository
            .FindByCondition(column =>
                column.IsActive &&
                boardIds.Contains(column.BoardId))
            .Select(column => column.Id)
            .ToListAsync(cancellationToken);

        if (workflowColumnIds.Count == 0)
        {
            return EmptyResponse();
        }

        IQueryable<BoardTask> query = _repoWrapper
            .BoardTaskRepository
            .FindByCondition(task =>
                task.IsActive &&
                workflowColumnIds.Contains(task.WorkflowColumnId))
            .Where(conditionExpression);

        int totalCount = await query.CountAsync(
            cancellationToken);

        if (totalCount == 0)
        {
            return EmptyResponse();
        }

        List<BoardTask> tasks = await query
            .OrderByDescending(task => task.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        Dictionary<Guid, string> metadataNames =
            await GetMetadataNames(
                tasks,
                cancellationToken);

        Dictionary<Guid, Guid> assigneeUserIds =
            await GetAssigneeUserIds(
                tasks,
                cancellationToken);

        Dictionary<Guid, string> userNames =
            await GetUserNames(
                assigneeUserIds.Values,
                cancellationToken);

        List<TaskDetailsDto> items = tasks
            .Select(task => MapTask(
                task,
                currentUserId,
                metadataNames,
                assigneeUserIds,
                userNames))
            .ToList();

        _logger.LogInformation(
            "Task query returned {ItemCount} of {TotalCount} matching tasks.",
            items.Count,
            totalCount);

        return new TaskQueryResponseDto
        {
            Items = items,
            TotalCount = totalCount
        };
    }

    private async Task<Dictionary<Guid, string>> GetMetadataNames(
        IEnumerable<BoardTask> tasks,
        CancellationToken cancellationToken)
    {
        Guid[] refTermIds = tasks
            .SelectMany(task => new[]
            {
                task.PriorityId,
                task.TaskTypeId
            })
            .Distinct()
            .ToArray();

        Dictionary<Guid, string> metadataNames = [];

        foreach (Guid refTermId in refTermIds)
        {
            RefTermDto? refTerm = await _metadataGrpcHelperService
                .GetRefTermByIdAsync(
                    refTermId,
                    cancellationToken);

            metadataNames[refTermId] = refTerm.RefTermKey;
        }

        return metadataNames;
    }

    private async Task<Dictionary<Guid, Guid>> GetAssigneeUserIds(
        IEnumerable<BoardTask> tasks,
        CancellationToken cancellationToken)
    {
        Guid[] boardAccessIds = tasks
            .Where(task => task.AssigneeBoardAccessId.HasValue)
            .Select(task => task.AssigneeBoardAccessId!.Value)
            .Distinct()
            .ToArray();

        if (boardAccessIds.Length == 0)
        {
            return [];
        }

        List<BoardAccess> boardAccessRecords = await _repoWrapper
            .BoardAccessRepository
            .FindByCondition(access =>
                access.IsActive &&
                boardAccessIds.Contains(access.Id))
            .ToListAsync(cancellationToken);

        Guid[] mappingIds = boardAccessRecords
            .Select(access => access.UserProjectMappingId)
            .Distinct()
            .ToArray();

        List<UserProjectMapping> userMappings = await _repoWrapper
            .UserProjectMappingRepository
            .FindByCondition(mapping =>
                mapping.IsActive &&
                mappingIds.Contains(mapping.Id))
            .ToListAsync(cancellationToken);

        Dictionary<Guid, Guid> userIdByMappingId =
            userMappings.ToDictionary(
                mapping => mapping.Id,
                mapping => mapping.UserId);

        Dictionary<Guid, Guid> userIdByBoardAccessId = [];

        foreach (BoardAccess boardAccess in boardAccessRecords)
        {
            if (userIdByMappingId.TryGetValue(
                boardAccess.UserProjectMappingId,
                out Guid userId))
            {
                userIdByBoardAccessId[boardAccess.Id] = userId;
            }
        }

        return userIdByBoardAccessId;
    }

    private async Task<Dictionary<Guid, string>> GetUserNames(
        IEnumerable<Guid> userIds,
        CancellationToken cancellationToken)
    {
        Guid[] distinctUserIds = userIds
            .Distinct()
            .ToArray();

        Dictionary<Guid, string> userNames = [];

        foreach (Guid userId in distinctUserIds)
        {
            UserDto? user = await _identityGrpcHelperService
                .GetUserByIdAsync(
                    userId,
                    cancellationToken);

            userNames[userId] = user.DisplayName;
        }

        return userNames;
    }

    private static TaskDetailsDto MapTask(
        BoardTask task,
        Guid currentUserId,
        IReadOnlyDictionary<Guid, string> metadataNames,
        IReadOnlyDictionary<Guid, Guid> assigneeUserIds,
        IReadOnlyDictionary<Guid, string> userNames)
    {
        metadataNames.TryGetValue(
            task.PriorityId,
            out string? priorityName);

        metadataNames.TryGetValue(
            task.TaskTypeId,
            out string? taskTypeName);

        Guid? assigneeUserId = null;
        string? assigneeName = null;

        if (task.AssigneeBoardAccessId.HasValue &&
            assigneeUserIds.TryGetValue(
                task.AssigneeBoardAccessId.Value,
                out Guid userId))
        {
            assigneeUserId = userId;
            userNames.TryGetValue(
                userId,
                out assigneeName);
        }

        return new TaskDetailsDto
        {
            Id = task.Id,
            Title = task.Title,
            PriorityName = priorityName ?? string.Empty,
            TaskTypeName = taskTypeName ?? string.Empty,
            AssigneeUserId = assigneeUserId,
            AssigneeName = assigneeName,
            IsTaskOwner = task.CreatedBy == currentUserId,
            Description = null,
            Comments = []
        };
    }

    private static TaskQueryResponseDto EmptyResponse()
    {
        return new TaskQueryResponseDto
        {
            Items = [],
            TotalCount = 0
        };
    }
}
