using BoardTaskService.Application.ProjectModule.Contract.IRepository;
using BoardTaskService.Domain.Models;
using BoardTaskService.Infrastructure.Persistence.ApplicationContext;
using Shared.Common.Repository;

namespace BoardTaskService.Infrastructure.ProjectModule.Repository;

/// <summary>
/// Repository for user-project mapping entities.
/// </summary>
public class UserProjectMappingRepository(
    BoardTaskDbContext repositoryContext)
    : RepositoryBase<UserProjectMapping, BoardTaskDbContext>(repositoryContext),
      IUserProjectMappingRepository
{
}