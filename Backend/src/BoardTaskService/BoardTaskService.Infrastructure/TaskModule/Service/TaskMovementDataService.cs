using BoardTaskService.Infrastructure.Common;
using BoardTaskService.Application.TaskModule.Contract.IService;
using BoardTaskService.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BoardTaskService.Infrastructure.TaskModule.Service;

/// <summary>
/// Represents the TaskMovementDataService component.
/// </summary>
public class TaskMovementDataService : ITaskMovementDataService
{
    private readonly IRepoWrapper _repoWrapper;

    public TaskMovementDataService(IRepoWrapper repoWrapper)
    {
        _repoWrapper = repoWrapper;
    }

    public Task<bool> HasBoardAccess(
        Guid boardId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        return _repoWrapper.BoardAccessRepository.AnyByConditionAsync(
            boardAccess =>
                boardAccess.IsActive &&
                boardAccess.BoardId == boardId &&
                boardAccess.Board.IsActive &&
                boardAccess.UserProjectMapping.IsActive &&
                boardAccess.UserProjectMapping.UserId == userId,
            cancellationToken);
    }

    public Task<BoardTask?> GetTask(
        Guid boardId,
        Guid taskId,
        CancellationToken cancellationToken)
    {
        return _repoWrapper.BoardTaskRepository
            .FindByCondition(task =>
                task.IsActive &&
                task.Id == taskId &&
                task.WorkflowColumn.IsActive &&
                task.WorkflowColumn.BoardId == boardId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<bool> WorkflowColumnExists(
        Guid boardId,
        Guid workflowColumnId,
        CancellationToken cancellationToken)
    {
        return _repoWrapper.WorkflowColumnRepository.AnyByConditionAsync(
            workflowColumn =>
                workflowColumn.IsActive &&
                workflowColumn.Id == workflowColumnId &&
                workflowColumn.BoardId == boardId,
            cancellationToken);
    }

    public async Task SaveTask(
        BoardTask task,
        CancellationToken cancellationToken)
    {
        _repoWrapper.BoardTaskRepository.Update(task);

        await _repoWrapper.SaveChangesAsync(
            cancellationToken);
    }
}
