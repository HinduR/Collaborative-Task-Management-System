using BoardTaskService.Application.BoardModule.Contract.IRepository;
using BoardTaskService.Domain.Models;
using BoardTaskService.Infrastructure.Persistence.ApplicationContext;
using Shared.Common.Repository;

namespace BoardTaskService.Infrastructure.BoardModule.Repository;

/// <summary>
/// Repository for workflow-column entities.
/// </summary>
public class WorkflowColumnRepository(
    BoardTaskDbContext repositoryContext)
    : RepositoryBase<WorkflowColumn, BoardTaskDbContext>(repositoryContext),
      IWorkflowColumnRepository
{
}