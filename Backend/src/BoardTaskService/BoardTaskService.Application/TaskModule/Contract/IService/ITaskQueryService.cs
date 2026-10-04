using System.Linq.Expressions;
using BoardTaskService.Application.TaskModule.Dto;
using BoardTaskService.Domain.Models;

namespace BoardTaskService.Application.TaskModule.Contract.IService;

/// <summary>Defines operations for querying accessible tasks using dynamic filter conditions and pagination.</summary>
public interface ITaskQueryService
{
    /// <summary>Queries tasks accessible to the specified user using the supplied filter expression.</summary>
    /// <param name="currentUserId">The unique identifier of the current authenticated user.</param>
    /// <param name="conditionExpression">The expression containing the task filter conditions.</param>
    /// <param name="pageNumber">The one-based page number to retrieve.</param>
    /// <param name="pageSize">The maximum number of tasks to return in one page.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>The matching tasks and total result count.</returns>
    Task<TaskQueryResponseDto> QueryTasksAsync(
        Guid currentUserId,
        Expression<Func<BoardTask, bool>> conditionExpression,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);
}
