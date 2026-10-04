using BoardTaskService.Application.TaskModule.Contract.IRepository;
using BoardTaskService.Domain.Models;
using BoardTaskService.Infrastructure.Persistence.ApplicationContext;
using Shared.Common.Repository;

namespace BoardTaskService.Infrastructure.TaskModule.Repository;

/// <summary>
/// Repository for board task entities.
/// </summary>
public class BoardTaskRepository(
    BoardTaskDbContext repositoryContext)
    : RepositoryBase<BoardTask, BoardTaskDbContext>(repositoryContext),
      IBoardTaskRepository
{
}
