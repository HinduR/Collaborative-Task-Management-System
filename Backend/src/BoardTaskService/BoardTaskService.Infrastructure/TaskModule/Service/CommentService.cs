using BoardTaskService.Infrastructure.Common;
using BoardTaskService.Application.TaskModule.Contract.IService;
using BoardTaskService.Domain.Models;
using Shared.Logging.Contracts;

namespace BoardTaskService.Infrastructure.TaskModule.Service;

/// <summary>
/// Handles task-comment data access.
/// Business validation and orchestration remain in the Application helper service.
/// </summary>
public class CommentService : ICommentService
{
    private readonly IRepoWrapper _repoWrapper;
    private readonly ILoggerManager<CommentService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommentService"/> class.
    /// </summary>
    public CommentService(
        IRepoWrapper repoWrapper,
        ILoggerManager<CommentService> logger)
    {
        _repoWrapper = repoWrapper;
        _logger = logger;
    }

    /// <summary>
    /// Determines whether an active task belongs to the specified active board.
    /// </summary>
    public async Task<bool> TaskExistsInBoardAsync(
        Guid boardId,
        Guid taskId,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Checking whether task {TaskId} belongs to board {BoardId}.",
            taskId,
            boardId);

        BoardTask? task = await _repoWrapper.BoardTaskRepository
            .FindFirstByConditionAsync(
                task =>
                    task.IsActive &&
                    task.Id == taskId,
                cancellationToken);

        if (task is null)
        {
            _logger.LogDebug(
                "Active task {TaskId} was not found.",
                taskId);

            return false;
        }

        bool columnExists = await _repoWrapper.WorkflowColumnRepository
            .AnyByConditionAsync(
                column =>
                    column.IsActive &&
                    column.Id == task.WorkflowColumnId &&
                    column.BoardId == boardId,
                cancellationToken);

        _logger.LogDebug(
            "Task {TaskId} board validation returned {Result}.",
            taskId,
            columnExists);

        return columnExists;
    }

    /// <summary>
    /// Retrieves an active comment belonging to the specified task.
    /// </summary>
    public async Task<TaskComment?> GetActiveCommentAsync(
        Guid taskId,
        Guid commentId,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Fetching active comment {CommentId} for task {TaskId}.",
            commentId,
            taskId);

        TaskComment? comment = await _repoWrapper.TaskCommentRepository
            .FindFirstByConditionAsync(
                comment =>
                    comment.IsActive &&
                    comment.Id == commentId &&
                    comment.TaskId == taskId,
                cancellationToken);

        _logger.LogDebug(
            "Active comment fetch for {CommentId} returned {Result}.",
            commentId,
            comment is null ? "no result" : "a result");

        return comment;
    }

    /// <summary>
    /// Creates a task comment and saves the changes.
    /// </summary>
    public async Task CreateCommentAsync(
        TaskComment comment,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Creating comment for task {TaskId}.",
            comment.TaskId);

        await _repoWrapper.TaskCommentRepository.CreateAsync(
            comment,
            cancellationToken);

        await _repoWrapper.SaveChangesAsync(cancellationToken);

        _logger.LogDebug(
            "Comment {CommentId} created successfully for task {TaskId}.",
            comment.Id,
            comment.TaskId);
    }

    /// <summary>
    /// Updates an existing task comment and saves the changes.
    /// </summary>
    public async Task UpdateCommentAsync(
        TaskComment comment,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Updating comment {CommentId}.",
            comment.Id);

        _repoWrapper.TaskCommentRepository.Update(comment);

        await _repoWrapper.SaveChangesAsync(cancellationToken);

        _logger.LogDebug(
            "Comment {CommentId} updated successfully.",
            comment.Id);
    }

    /// <summary>
    /// Soft-deletes a task comment and saves the changes.
    /// </summary>
    public async Task DeleteCommentAsync(
        TaskComment comment,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Soft-deleting comment {CommentId}.",
            comment.Id);

        comment.IsActive = false;

        _repoWrapper.TaskCommentRepository.Update(comment);

        await _repoWrapper.SaveChangesAsync(cancellationToken);

        _logger.LogDebug(
            "Comment {CommentId} soft-deleted successfully.",
            comment.Id);
    }
}
