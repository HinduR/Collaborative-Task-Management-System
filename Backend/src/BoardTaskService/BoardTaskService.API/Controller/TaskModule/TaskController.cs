using BoardTaskService.Application.TaskModule.Command;
using BoardTaskService.Application.TaskModule.Command.Comment.Create;
using BoardTaskService.Application.TaskModule.Command.Comment.Delete;
using BoardTaskService.Application.TaskModule.Command.Comment.Update;
using BoardTaskService.Application.TaskModule.Command.Create;
using BoardTaskService.Application.TaskModule.Command.Delete;
using BoardTaskService.Application.TaskModule.Command.Move;
using BoardTaskService.Application.TaskModule.Command.Update;
using BoardTaskService.Application.TaskModule.Dto;
using BoardTaskService.Application.TaskModule.Query.Get;
using BoardTaskService.Application.TaskModule.Query.TaskQuery;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Authorisation.Domain.Attribute;
using Shared.Logging.Contracts;
using Swashbuckle.AspNetCore.Annotations;

namespace BoardTaskService.API.Controller.TaskModule.v1;

/// <summary>Handles requests related to board tasks and task comments.</summary>
[ApiController]
[Route("board-task/boards/{boardId}/tasks")]
public class TaskController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILoggerManager<TaskController> _logger;

    /// <summary>Initializes a new instance of the <see cref="TaskController"/> class.</summary>
    /// <param name="mediator">Mediator used to dispatch task commands and queries.</param>
    /// <param name="logger">Logger used to record task-related API activity.</param>
    public TaskController(
        IMediator mediator,
        ILoggerManager<TaskController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>Retrieves the list of tasks belonging to the specified board.</summary>
    /// <param name="boardId">The unique identifier of the board.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>A list of tasks belonging to the board.</returns>
    [HttpGet]
    [CustomAuthorize("TEAM_MEMBER_ACCESS")]
    [SwaggerResponse(StatusCodes.Status200OK, "Board tasks returned successfully.", typeof(List<TaskSummaryDto>))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Authentication is required.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "You do not have access to this board.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "The selected board does not exist.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error.")]
    public async Task<IActionResult> GetBoardTasks(
        Guid boardId,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Received request to fetch tasks for board {BoardId}.", boardId);

        List<TaskSummaryDto> response = await _mediator.Send(
            new GetBoardTasksQuery(boardId),
            cancellationToken);

        _logger.LogDebug("Task list fetch completed for board {BoardId}. {Count} tasks returned.", boardId, response.Count);

        return Ok(response);
    }

    /// <summary>Creates a new task on the specified board.</summary>
    /// <param name="boardId">The unique identifier of the board where the task will be created.</param>
    /// <param name="request">The task creation details.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>The newly created task.</returns>
    [HttpPost]
    [CustomAuthorize("TEAM_MEMBER_ACCESS")]
    [SwaggerResponse(StatusCodes.Status201Created, "Task created successfully.", typeof(TaskSummaryDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid task data.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Authentication is required.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "You do not have access to this board.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "The selected board does not exist.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error.")]
    public async Task<IActionResult> CreateTask(
        Guid boardId,
        [FromBody] TaskRequestDto request,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Received request to create a task on board {BoardId}.", boardId);

        TaskSummaryDto response = await _mediator.Send(
            new CreateTaskCommand(boardId, request),
            cancellationToken);

        _logger.LogDebug("Task creation completed for board {BoardId}. Task {TaskId} created.", boardId, response.Id);

        return CreatedAtAction(
            nameof(GetTaskById),
            new { boardId, taskId = response.Id },
            response);
    }

    /// <summary>Retrieves the details of a single task.</summary>
    /// <param name="boardId">The unique identifier of the board.</param>
    /// <param name="taskId">The unique identifier of the task.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>The requested task details.</returns>
    [HttpGet("{taskId}")]
    [CustomAuthorize("TEAM_MEMBER_ACCESS")]
    [SwaggerResponse(StatusCodes.Status200OK, "Task details returned successfully.", typeof(TaskDetailsDto))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Authentication is required.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "You do not have access to this task.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "The selected board or task does not exist.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error.")]
    public async Task<IActionResult> GetTaskById(
        Guid boardId,
        Guid taskId,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Received request to fetch task {TaskId} on board {BoardId}.", taskId, boardId);

        TaskDetailsDto response = await _mediator.Send(
            new GetTaskByIdQuery(boardId, taskId),
            cancellationToken);

        _logger.LogDebug("Task fetch completed for task {TaskId}.", taskId);

        return Ok(response);
    }

    /// <summary>Updates the details of an existing task.</summary>
    /// <param name="boardId">The unique identifier of the board.</param>
    /// <param name="taskId">The unique identifier of the task to update.</param>
    /// <param name="request">The updated task details.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>The updated task details.</returns>
    [HttpPatch("{taskId}")]
    [CustomAuthorize("TEAM_MEMBER_ACCESS")]
    [SwaggerResponse(StatusCodes.Status200OK, "Task updated successfully.", typeof(TaskDetailsDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid task data.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Authentication is required.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "You do not have access to this task.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "The selected board or task does not exist.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error.")]
    public async Task<IActionResult> UpdateTask(
        Guid boardId,
        Guid taskId,
        [FromBody] TaskRequestDto request,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Received request to update task {TaskId} on board {BoardId}.", taskId, boardId);

        TaskDetailsDto response = await _mediator.Send(
            new UpdateTaskCommand(boardId, taskId, request),
            cancellationToken);

        _logger.LogDebug("Task update completed for task {TaskId}.", taskId);

        return Ok(response);
    }

    /// <summary>Moves a task to another workflow column.</summary>
    /// <param name="boardId">The unique identifier of the board.</param>
    /// <param name="taskId">The unique identifier of the task.</param>
    /// <param name="request">The destination workflow column information.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>The updated task movement details.</returns>
    [HttpPatch("{taskId}/move")]
    [CustomAuthorize("TEAM_MEMBER_ACCESS")]
    [SwaggerResponse(StatusCodes.Status200OK, "Task moved successfully.", typeof(MoveTaskResponseDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid target workflow column.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Authentication is required.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "You do not have access to this task.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "The selected board, task, or workflow column does not exist.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error.")]
    public async Task<IActionResult> MoveTask(
        Guid boardId,
        Guid taskId,
        [FromBody] MoveTaskRequestDto request,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
        "Received request to move task {TaskId} on board {BoardId} to workflow column {WorkflowColumnId}.",
        taskId,
        boardId,
        request.WorkflowColumnId);

        MoveTaskResponseDto response = await _mediator.Send(
            new MoveTaskCommand(
                boardId,
                taskId,
                request),
            cancellationToken);

        _logger.LogDebug(
            "Task move completed for task {TaskId} on board {BoardId}.",
            taskId,
            boardId);

        return Ok(response);
    }

    /// <summary>Deletes the specified task.</summary>
    /// <param name="boardId">The unique identifier of the board.</param>
    /// <param name="taskId">The unique identifier of the task to delete.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>A 204 No Content response when the task is deleted successfully.</returns>
    [HttpDelete("{taskId:guid}")]
    [CustomAuthorize("TEAM_MEMBER_ACCESS")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Task deleted successfully.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Authentication is required.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "You do not have access to this task.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "The selected board or task does not exist.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error.")]
    public async Task<IActionResult> DeleteTask(
        Guid boardId,
        Guid taskId,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Received request to delete task {TaskId} on board {BoardId}.", taskId, boardId);

        await _mediator.Send(
            new DeleteTaskCommand(boardId, taskId),
            cancellationToken);

        _logger.LogDebug("Task deletion completed for task {TaskId}.", taskId);

        return NoContent();
    }

    /// <summary>Adds a comment to the specified task.</summary>
    /// <param name="boardId">The unique identifier of the board.</param>
    /// <param name="taskId">The unique identifier of the task.</param>
    /// <param name="request">The comment content.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>The newly created comment.</returns>
    [HttpPost("{taskId}/comments")]
    [CustomAuthorize("TEAM_MEMBER_ACCESS")]
    [SwaggerResponse(StatusCodes.Status201Created, "Comment created successfully.", typeof(TaskCommentDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Comment validation failed.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Authentication is required.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "The user does not have access to this task.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "The selected board or task does not exist.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error.")]
    public async Task<IActionResult> CreateTaskComment(
        Guid boardId,
        Guid taskId,
        [FromBody] CommentRequestDto request,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Received request to add a comment to task {TaskId} on board {BoardId}.", taskId, boardId);

        TaskCommentDto response = await _mediator.Send(
            new CreateTaskCommentCommand(
                boardId,
                taskId,
                request),
            cancellationToken);

        _logger.LogDebug("Comment creation completed for task {TaskId}. Comment {CommentId} created.", taskId, response.Id);

        return StatusCode(
            StatusCodes.Status201Created,
            response);
    }

    /// <summary>Updates an existing comment on a task.</summary>
    /// <param name="boardId">The unique identifier of the board.</param>
    /// <param name="taskId">The unique identifier of the task.</param>
    /// <param name="commentId">The unique identifier of the comment to update.</param>
    /// <param name="request">The updated comment content.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>The updated comment.</returns>
    [HttpPatch("{taskId}/comments/{commentId}")]
    [CustomAuthorize("TEAM_MEMBER_ACCESS")]
    [SwaggerResponse(StatusCodes.Status200OK, "Comment updated successfully.", typeof(TaskCommentDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Comment validation failed.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Authentication is required.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Only the comment owner can update this comment.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "The selected board, task, or comment does not exist.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error.")]
    public async Task<IActionResult> UpdateTaskComment(
        Guid boardId,
        Guid taskId,
        Guid commentId,
        [FromBody] CommentRequestDto request,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Received request to update comment {CommentId} on task {TaskId}.", commentId, taskId);

        TaskCommentDto response = await _mediator.Send(
            new UpdateTaskCommentCommand(
                boardId,
                taskId,
                commentId,
                request),
            cancellationToken);

        _logger.LogDebug("Comment update completed for comment {CommentId}.", commentId);

        return Ok(response);
    }

    /// <summary>Deletes a comment from a task.</summary>
    /// <param name="boardId">The unique identifier of the board.</param>
    /// <param name="taskId">The unique identifier of the task.</param>
    /// <param name="commentId">The unique identifier of the comment to delete.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>A 204 No Content response when the comment is deleted successfully.</returns>
    [HttpDelete("{taskId}/comments/{commentId}")]
    [CustomAuthorize("TEAM_MEMBER_ACCESS")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Comment deleted successfully.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Authentication is required.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Only the comment owner can delete this comment.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "The selected board, task, or comment does not exist.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error.")]
    public async Task<IActionResult> DeleteTaskComment(
        Guid boardId,
        Guid taskId,
        Guid commentId,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Received request to delete comment {CommentId} on task {TaskId}.", commentId, taskId);

        await _mediator.Send(
            new DeleteTaskCommentCommand(
                boardId,
                taskId,
                commentId),
            cancellationToken);

        _logger.LogDebug("Comment deletion completed for comment {CommentId}.", commentId);

        return NoContent();
    }

    /// <summary>Queries accessible tasks using the specified filter conditions.</summary>
    /// <param name="request">The task query filter criteria and pagination information.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>The paginated tasks matching the query conditions.</returns>
    [HttpPost("~/board-task/tasks/query")]
    [CustomAuthorize("TEAM_MEMBER_ACCESS")]
    [SwaggerResponse(StatusCodes.Status200OK, "Matching tasks returned successfully.", typeof(TaskQueryResponseDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Task query validation failed.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Authentication is required.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "The user is not authorized to query tasks.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error.")]
    public async Task<IActionResult> QueryTasks(
        [FromBody] TaskQueryRequestDto request,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Received request to query tasks.");

        TaskQueryResponseDto response = await _mediator.Send(
            new GetTaskQuery(request),
            cancellationToken);

        _logger.LogDebug("Task query completed. {Count} matching tasks returned.", response.Items.Count());

        return Ok(response);
    }
}