using BoardTaskService.Application.TaskModule.Contract.IRepository;
using BoardTaskService.Domain.Models;
using BoardTaskService.Infrastructure.Persistence.ApplicationContext;
using Shared.Common.Repository;

namespace BoardTaskService.Infrastructure.TaskModule.Repository;

/// <summary>
/// Repository for task comment entities.
/// </summary>
public class TaskCommentRepository(
    BoardTaskDbContext repositoryContext)
    : RepositoryBase<TaskComment, BoardTaskDbContext>(repositoryContext),
      ITaskCommentRepository
{
}
