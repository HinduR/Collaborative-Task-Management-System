using BoardTaskService.Infrastructure.Common;
using BoardTaskService.Application.TaskModule.Contract.IService;
using BoardTaskService.Application.TaskModule.Dto;
using BoardTaskService.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Shared.Common.contracts;
using Shared.Common.Dto;
using Shared.Exceptions.Infrastructure;
using Shared.Grpc.Common.Dto;
using Shared.Grpc.Contracts;
using Shared.Logging.Contracts;

namespace BoardTaskService.Infrastructure.TaskModule.Service;

/// <summary>
/// Represents the TaskService component.
/// </summary>
public class TaskService : ITaskService
{
    private const string TaskPriorityRefSetKey = "PRIORITY";
    private const string TaskTypeRefSetKey = "TASK_TYPE";

    private readonly IRepoWrapper _repoWrapper;
    private readonly IUserContext _userContext;
    private readonly IMetadataGrpcHelperService _metadataGrpcHelperService;
    private readonly IIdentityGrpcHelperService _identityGrpcHelperService;
    private readonly ILoggerManager<TaskService> _logger;

    public TaskService(
        IRepoWrapper repoWrapper,
        IUserContext userContext,
        IMetadataGrpcHelperService metadataGrpcHelperService,
        IIdentityGrpcHelperService identityGrpcHelperService,
        ILoggerManager<TaskService> logger)
    {
        _repoWrapper = repoWrapper;
        _userContext = userContext;
        _metadataGrpcHelperService = metadataGrpcHelperService;
        _identityGrpcHelperService = identityGrpcHelperService;
        _logger = logger;
    }

    public async Task<TaskSummaryDto> CreateTask(
    Guid boardId,
    TaskRequestDto request,
    CancellationToken cancellationToken)
{
    _logger.LogDebug(
        "Executing CreateTask.");

    Guid currentUserId = _userContext.GetUserId();
   Shared.Grpc.Common.Dto.UserDto user = await _identityGrpcHelperService.GetUserByIdAsync(currentUserId);

    Board board = await GetBoardWithAccess(
        boardId,
        cancellationToken);

    WorkflowColumn workflowColumn = await GetWorkflowColumn(
        boardId,
        request.WorkflowColumnId,
        cancellationToken);

    Task<RefTermDto> priorityTask =
        _metadataGrpcHelperService.GetRefTermByKeyAsync(
            request.PriorityRefTermKey,
            cancellationToken);

    Task<RefTermDto> taskTypeTask =
        _metadataGrpcHelperService.GetRefTermByKeyAsync(
            request.TaskTypeRefTermKey,
            cancellationToken);

    Task<Guid?> assigneeBoardAccessTask =
        GetAssigneeBoardAccessId(
            board.Id,
            board.ProjectId,
            request.AssigneeUserId,
            cancellationToken);

    await Task.WhenAll(
        priorityTask,
        taskTypeTask,
        assigneeBoardAccessTask);

    RefTermDto priority = await priorityTask;
    RefTermDto taskType = await taskTypeTask;
    Guid? assigneeBoardAccessId =
        await assigneeBoardAccessTask;

    BoardTask task = new()
    {
        Id = Guid.NewGuid(),
        WorkflowColumnId = workflowColumn.Id,
        AssigneeBoardAccessId = assigneeBoardAccessId,
        PriorityId = priority.RefTermId,
        TaskTypeId = taskType.RefTermId,
        Title = request.Title.Trim(),
        Description = request.Description?.Trim(),
        IsActive = true
    };

    _repoWrapper.BoardTaskRepository.Create(task);

    await _repoWrapper.SaveChangesAsync(cancellationToken);

    _logger.LogDebug(
        "Task {TaskId} was created in board {BoardId} by user {UserId}.",
        task.Id,
        boardId,
        currentUserId);

    return new TaskSummaryDto
    {
        Id = task.Id,
        WorkflowColumnId = task.WorkflowColumnId,
        Title = task.Title,
        Description = task.Description,
        PriorityName = priority.RefTermKey,
        TaskTypeName = taskType.RefTermKey,
        AssigneeUserId = request.AssigneeUserId,
        AssigneeName =user.DisplayName,
        IsTaskOwner = true
    };
}

    public async Task<TaskDetailsDto> GetTaskById(
        Guid boardId,
        Guid taskId,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Executing GetTaskById.");

        await GetBoardWithAccess(boardId, cancellationToken);

        BoardTask task = await GetTaskInBoard(
            boardId,
            taskId,
            cancellationToken);

        List<TaskComment> commentList = await _repoWrapper
            .TaskCommentRepository
            .FindByCondition(comment =>
                comment.IsActive &&
                comment.TaskId == taskId)
            .OrderBy(comment => comment.CreatedAt)
            .ToListAsync(cancellationToken);

        List<RefTermDto> priorityList = await _metadataGrpcHelperService
            .GetRefTermListByRefSetKeyAsync(
                TaskPriorityRefSetKey,
                cancellationToken);

        List<RefTermDto> taskTypeList = await _metadataGrpcHelperService
            .GetRefTermListByRefSetKeyAsync(
                TaskTypeRefSetKey,
                cancellationToken);

        RefTermDto? priority = priorityList.FirstOrDefault(
            term => term.RefTermId == task.PriorityId);

        RefTermDto? taskType = taskTypeList.FirstOrDefault(
            term => term.RefTermId == task.TaskTypeId);

        Guid? assigneeUserId = await GetAssigneeUserId(
            task.AssigneeBoardAccessId,
            cancellationToken);

        List<Guid> userIdList = commentList
            .Select(comment => comment.CreatedBy)
            .Append(assigneeUserId ?? Guid.Empty)
            .Where(userId => userId != Guid.Empty)
            .Distinct()
            .ToList();

        List<UserDto> userList = await _identityGrpcHelperService
            .GetUserListAsync(userIdList, cancellationToken);

        Dictionary<Guid, string> userNameById = userList
            .GroupBy(user => user.Id)
            .ToDictionary(
                group => group.Key,
                group => group.First().DisplayName);

        Guid currentUserId = _userContext.GetUserId();

        return new TaskDetailsDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            PriorityName = priority?.Description ?? priority?.RefTermKey ?? string.Empty,
            TaskTypeName = taskType?.Description ?? taskType?.RefTermKey ?? string.Empty,
            AssigneeUserId = assigneeUserId,
            AssigneeName = assigneeUserId.HasValue &&
                           userNameById.TryGetValue(
                               assigneeUserId.Value,
                               out string? assigneeName)
                ? assigneeName
                : null,
            IsTaskOwner = task.CreatedBy == currentUserId,
            Comments = commentList
                .Select(comment => new TaskCommentDto
                {
                    Id = comment.Id,
                    Comment = comment.Comment,
                    CreatedByName = userNameById.TryGetValue(
                        comment.CreatedBy,
                        out string? createdByName)
                        ? createdByName
                        : string.Empty,
                    IsCommentOwner = comment.CreatedBy == currentUserId
                })
                .ToList()
        };
    }

    public async Task<TaskDetailsDto> UpdateTask(
        Guid boardId,
        Guid taskId,
        TaskRequestDto request,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Executing UpdateTask.");

        Board board = await GetBoardWithAccess(
            boardId,
            cancellationToken);

        BoardTask task = await GetTaskInBoard(
            boardId,
            taskId,
            cancellationToken);

        EnsureTaskOwner(task);

        WorkflowColumn workflowColumn = await GetWorkflowColumn(
            boardId,
            request.WorkflowColumnId,
            cancellationToken);

        RefTermDto priority = await GetRefTermByRefSetKey(
            TaskPriorityRefSetKey,
            request.PriorityRefTermKey,
            "priority",
            cancellationToken);

        RefTermDto taskType = await GetRefTermByRefSetKey(
            TaskTypeRefSetKey,
            request.TaskTypeRefTermKey,
            "task type",
            cancellationToken);

        Guid? assigneeBoardAccessId = await GetAssigneeBoardAccessId(
            board.Id,
            board.ProjectId,
            request.AssigneeUserId,
            cancellationToken);

        task.Title = request.Title.Trim();
        task.Description = request.Description?.Trim();
        task.PriorityId = priority.RefTermId;
        task.TaskTypeId = taskType.RefTermId;
        task.AssigneeBoardAccessId = assigneeBoardAccessId;
        task.WorkflowColumnId = workflowColumn.Id;

        _repoWrapper.BoardTaskRepository.Update(task);

        await _repoWrapper.SaveChangesAsync(cancellationToken);

        _logger.LogDebug(
            "Task {TaskId} was updated by user {UserId}.",
            taskId,
            _userContext.GetUserId());

        return await GetTaskById(
            boardId,
            taskId,
            cancellationToken);
    }

    public async Task DeleteTask(
        Guid boardId,
        Guid taskId,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Executing DeleteTask.");

        await GetBoardWithAccess(boardId, cancellationToken);

        BoardTask task = await GetTaskInBoard(
            boardId,
            taskId,
            cancellationToken);

        EnsureTaskOwner(task);

        List<TaskComment> commentList = await _repoWrapper
            .TaskCommentRepository
            .FindByCondition(comment =>
                comment.IsActive &&
                comment.TaskId == taskId)
            .ToListAsync(cancellationToken);

        task.IsActive = false;

        foreach (TaskComment comment in commentList)
        {
            comment.IsActive = false;
        }

        _repoWrapper.BoardTaskRepository.Update(task);

        if (commentList.Any())
        {
            _repoWrapper.TaskCommentRepository.UpdateRange(commentList);
        }

        await _repoWrapper.SaveChangesAsync(cancellationToken);

        _logger.LogDebug(
            "Task {TaskId} was deleted by user {UserId}.",
            taskId,
            _userContext.GetUserId());
    }

    private async Task<Board> GetBoardWithAccess(
        Guid boardId,
        CancellationToken cancellationToken)
    {
        Board? board = await _repoWrapper.BoardRepository
            .FindFirstByConditionAsync(
                board => board.IsActive && board.Id == boardId,
                cancellationToken);

        if (board is null)
        {
            throw new NotFoundCustomException(
                "The selected board does not exist.",
                "Board not found.");
        }

        Guid currentUserId = _userContext.GetUserId();

        UserProjectMapping? userProjectMapping = await _repoWrapper
            .UserProjectMappingRepository
            .FindFirstByConditionAsync(
                mapping =>
                    mapping.IsActive &&
                    mapping.UserId == currentUserId &&
                    mapping.ProjectId == board.ProjectId,
                cancellationToken);

        if (userProjectMapping is null)
        {
            throw new ForBiddenCustomException(
                "You do not have access to this project.",
                "Access denied.");
        }

        bool hasBoardAccess = await _repoWrapper.BoardAccessRepository
            .AnyByConditionAsync(
                access =>
                    access.IsActive &&
                    access.BoardId == boardId &&
                    access.UserProjectMappingId == userProjectMapping.Id,
                cancellationToken);

        if (!hasBoardAccess)
        {
            throw new ForBiddenCustomException(
                "You do not have access to this board.",
                "Access denied.");
        }

        return board;
    }

    private async Task<BoardTask> GetTaskInBoard(
        Guid boardId,
        Guid taskId,
        CancellationToken cancellationToken)
    {
        BoardTask? task = await _repoWrapper.BoardTaskRepository
            .FindFirstByConditionAsync(
                task => task.IsActive && task.Id == taskId,
                cancellationToken);

        if (task is null)
        {
            throw new NotFoundCustomException(
                "The selected task does not exist.",
                "Task not found.");
        }

        bool belongsToBoard = await _repoWrapper.WorkflowColumnRepository
            .AnyByConditionAsync(
                column =>
                    column.IsActive &&
                    column.Id == task.WorkflowColumnId &&
                    column.BoardId == boardId,
                cancellationToken);

        if (!belongsToBoard)
        {
            throw new NotFoundCustomException(
                "The selected task does not belong to this board.",
                "Task not found.");
        }

        return task;
    }

    private async Task<WorkflowColumn> GetWorkflowColumn(
        Guid boardId,
        Guid? workflowColumnId,
        CancellationToken cancellationToken)
    {
        WorkflowColumn? workflowColumn;

        if (workflowColumnId.HasValue)
        {
            workflowColumn = await _repoWrapper.WorkflowColumnRepository
                .FindFirstByConditionAsync(
                    column =>
                        column.IsActive &&
                        column.Id == workflowColumnId.Value &&
                        column.BoardId == boardId,
                    cancellationToken);
        }
        else
        {
            workflowColumn = await _repoWrapper.WorkflowColumnRepository
                .FindByCondition(
                    column =>
                        column.IsActive &&
                        column.BoardId == boardId)
                .OrderBy(column => column.SortOrder)
                .FirstOrDefaultAsync(cancellationToken);
        }

        if (workflowColumn is null)
        {
            throw new BadRequestCustomException(
                "The selected workflow column is invalid.",
                "Workflow column not found.");
        }

        return workflowColumn;
    }

    private async Task<RefTermDto> GetRefTermByRefSetKey(
        string refSetKey,
        string refTermKey,
        string fieldName,
        CancellationToken cancellationToken)
    {
        List<RefTermDto> refTermList = await _metadataGrpcHelperService
            .GetRefTermListByRefSetKeyAsync(
                refSetKey,
                cancellationToken);

        RefTermDto? refTerm = refTermList.FirstOrDefault(
            term => string.Equals(
                term.RefTermKey,
                refTermKey.Trim(),
                StringComparison.OrdinalIgnoreCase));

        if (refTerm is null)
        {
            throw new BadRequestCustomException(
                string.Concat(
                    "The selected ",
                    fieldName,
                    " is invalid."),
                string.Concat(
                    "Invalid ",
                    fieldName,
                    "."));
        }

        return refTerm;
    }

    private async Task<Guid?> GetAssigneeBoardAccessId(
        Guid boardId,
        Guid projectId,
        Guid? assigneeUserId,
        CancellationToken cancellationToken)
    {
        if (!assigneeUserId.HasValue)
        {
            return null;
        }

        UserProjectMapping? assigneeProjectMapping = await _repoWrapper
            .UserProjectMappingRepository
            .FindFirstByConditionAsync(
                mapping =>
                    mapping.IsActive &&
                    mapping.UserId == assigneeUserId.Value &&
                    mapping.ProjectId == projectId,
                cancellationToken);

        if (assigneeProjectMapping is null)
        {
            throw new BadRequestCustomException(
                "The selected assignee does not belong to this project.",
                "Invalid assignee.");
        }

        BoardAccess? boardAccess = await _repoWrapper.BoardAccessRepository
            .FindFirstByConditionAsync(
                access =>
                    access.IsActive &&
                    access.BoardId == boardId &&
                    access.UserProjectMappingId == assigneeProjectMapping.Id,
                cancellationToken);

        if (boardAccess is null)
        {
            throw new BadRequestCustomException(
                "The selected assignee does not have access to this board.",
                "Invalid assignee.");
        }

        return boardAccess.Id;
    }

    private async Task<Guid?> GetAssigneeUserId(
        Guid? assigneeBoardAccessId,
        CancellationToken cancellationToken)
    {
        if (!assigneeBoardAccessId.HasValue)
        {
            return null;
        }

        BoardAccess? boardAccess = await _repoWrapper.BoardAccessRepository
            .FindFirstByConditionAsync(
                access =>
                    access.IsActive &&
                    access.Id == assigneeBoardAccessId.Value,
                cancellationToken);

        if (boardAccess is null)
        {
            return null;
        }

        UserProjectMapping? mapping = await _repoWrapper
            .UserProjectMappingRepository
            .FindFirstByConditionAsync(
                userProjectMapping =>
                    userProjectMapping.IsActive &&
                    userProjectMapping.Id == boardAccess.UserProjectMappingId,
                cancellationToken);

        return mapping?.UserId;
    }

    private void EnsureTaskOwner(BoardTask task)
    {
        if (task.CreatedBy != _userContext.GetUserId())
        {
            throw new ForBiddenCustomException(
                "Only the task creator can update or delete this task.",
                "Task ownership is required.");
        }
    }

}
