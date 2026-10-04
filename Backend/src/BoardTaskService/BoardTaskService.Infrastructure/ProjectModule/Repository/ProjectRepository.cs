 using BoardTaskService.Application.ProjectModule.Contract.IRepository;
using BoardTaskService.Domain.Models;
using BoardTaskService.Infrastructure.Persistence.ApplicationContext;
using Shared.Common.Repository;

namespace BoardTaskService.Infrastructure.ProjectModule.Repository;

/// <summary>
/// Repository for project entities.
/// </summary>
public class ProjectRepository(
    BoardTaskDbContext repositoryContext)
    : RepositoryBase<Project, BoardTaskDbContext>(repositoryContext),
      IProjectRepository
{
}