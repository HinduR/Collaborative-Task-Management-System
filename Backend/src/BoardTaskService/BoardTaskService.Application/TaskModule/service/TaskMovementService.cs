using BoardTaskService.Application.TaskModule.Contract.IService;
using BoardTaskService.Application.TaskModule.Dto;
using BoardTaskService.Domain.Models;
using Shared.Common.contracts;
using Shared.Exceptions.Infrastructure;
using Shared.Logging.Contracts;

namespace BoardTaskService.Application.TaskModule.Service;

/// <summary>Handles task movement validation, persistence, and real-time notifications.</summary>
public class TaskMovementService : ITaskMovementService
{
    private readonly ITaskMovementDataService _dataService;
    private readonly IBoardRealtimeService _realtimeService;
    private readonly IUserContext _userContext;
    private readonly ILoggerManager<TaskMovementService> _logger;

    /// <summary>Initializes a new instance of the <see cref="TaskMovementService"/> class.</summary>
    /// <param name="dataService">Provides data-access operations required for task movement.</param>
    /// <param name="realtimeService">Provides real-time task movement notifications.</param>
    /// <param name="userContext">Provides information about the current authenticated user.</param>
    /// <param name="logger">Logger used to record task movement activity and errors.</param>
    public TaskMovementService(
        ITaskMovementDataService dataService,
        IBoardRealtimeService realtimeService,
        IUserContext userContext,
        ILoggerManager<TaskMovementService> logger)
    {
        _dataService = dataService;
        _realtimeService = realtimeService;
        _userContext = userContext;
        _logger = logger;
    }

    /// <summary>Moves a task to the specified workflow column and broadcasts the movement to connected board users.</summary>
    /// <param name="boardId">The unique identifier of the board containing the task.</param>
    /// <param name="taskId">The unique identifier of the task to move.</param>
    /// <param name="workflowColumnId">The unique identifier of the destination workflow column.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>The result containing the task identifier and its current workflow column.</returns>
    public async Task<MoveTaskResponseDto> MoveTask(
        Guid boardId,
        Guid taskId,
        Guid workflowColumnId,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Executing MoveTask.");

        Guid currentUserId = _userContext.GetUserId();

        _logger.LogInformation(
            "Moving task {TaskId} to workflow column {WorkflowColumnId} in board {BoardId} requested by user {UserId}.",
            taskId,
            workflowColumnId,
            boardId,
            currentUserId);

        bool hasBoardAccess = await _dataService.HasBoardAccess(
            boardId,
            currentUserId,
            cancellationToken);

        if (!hasBoardAccess)
        {
            ForBiddenCustomException exception = new(
                "You do not have access to the selected board.",
                "Board access denied.");

            _logger.LogError(
                "Task movement denied because user {UserId} does not have access to board {BoardId}.",
                exception,
                currentUserId,
                boardId);

            throw exception;
        }

        BoardTask? task = await _dataService.GetTask(
            boardId,
            taskId,
            cancellationToken);

        if (task == null)
        {
            NotFoundCustomException exception = new(
                "The selected task does not exist in this board.",
                "Task not found.");

            _logger.LogError(
                "Task movement failed because task {TaskId} was not found in board {BoardId}.",
                exception,
                taskId,
                boardId);

            throw exception;
        }

        bool workflowColumnExists =
            await _dataService.WorkflowColumnExists(
                boardId,
                workflowColumnId,
                cancellationToken);

        if (!workflowColumnExists)
        {
            BadRequestCustomException exception = new(
                "The selected workflow column does not belong to this board.",
                "Invalid workflow column.");

            _logger.LogError(
                "Task movement failed because workflow column {WorkflowColumnId} does not belong to board {BoardId}.",
                exception,
                workflowColumnId,
                boardId);

            throw exception;
        }

        Guid previousWorkflowColumnId = task.WorkflowColumnId;

        if (previousWorkflowColumnId == workflowColumnId)
        {
            _logger.LogInformation(
                "Task {TaskId} is already in workflow column {WorkflowColumnId}. No movement was required.",
                taskId,
                workflowColumnId);

            return new MoveTaskResponseDto
            {
                TaskId = task.Id,
                WorkflowColumnId = task.WorkflowColumnId
            };
        }

        DateTime movedAt = DateTime.UtcNow;

        task.WorkflowColumnId = workflowColumnId;
        task.UpdatedBy = currentUserId;
        task.UpdatedAt = movedAt;

        await _dataService.SaveTask(
            task,
            cancellationToken);

        _logger.LogInformation(
            "Task {TaskId} moved from workflow column {PreviousWorkflowColumnId} to {WorkflowColumnId} in board {BoardId}.",
            task.Id,
            previousWorkflowColumnId,
            workflowColumnId,
            boardId);

        TaskMovedEventDto taskMovedEvent = new()
        {
            BoardId = boardId,
            TaskId = task.Id,
            PreviousWorkflowColumnId = previousWorkflowColumnId,
            WorkflowColumnId = workflowColumnId,
            MovedByUserId = currentUserId,
            MovedAt = movedAt
        };

        await SendRealtimeUpdate(taskMovedEvent);

        _logger.LogInformation(
            "Task movement completed for task {TaskId} in board {BoardId}.",
            task.Id,
            boardId);

        return new MoveTaskResponseDto
        {
            TaskId = task.Id,
            WorkflowColumnId = task.WorkflowColumnId
        };
    }

    /// <summary>Sends a task movement notification without failing the persisted movement when real-time delivery fails.</summary>
    /// <param name="taskMovedEvent">The task movement event to broadcast.</param>
    private async Task SendRealtimeUpdate(
        TaskMovedEventDto taskMovedEvent)
    {
        try
        {
            _logger.LogInformation(
                "Sending real-time task movement update for task {TaskId} in board {BoardId}.",
                taskMovedEvent.TaskId,
                taskMovedEvent.BoardId);

            await _realtimeService.SendTaskMoved(
                taskMovedEvent,
                CancellationToken.None);

            _logger.LogInformation(
                "Real-time task movement update sent for task {TaskId} in board {BoardId}.",
                taskMovedEvent.TaskId,
                taskMovedEvent.BoardId);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                "Task {TaskId} was saved, but the real-time update failed for board {BoardId}.",
                exception,
                taskMovedEvent.TaskId,
                taskMovedEvent.BoardId);
        }
    }
}
