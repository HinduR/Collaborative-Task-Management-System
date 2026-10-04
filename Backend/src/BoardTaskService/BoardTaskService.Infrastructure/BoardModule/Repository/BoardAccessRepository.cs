using BoardTaskService.Application.BoardModule.Contract.IRepository;
using BoardTaskService.Domain.Models;
using BoardTaskService.Infrastructure.Persistence.ApplicationContext;
using Shared.Common.Repository;

namespace BoardTaskService.Infrastructure.BoardModule.Repository;

/// <summary>
/// Repository for board-access entities.
/// </summary>
public class BoardAccessRepository(
    BoardTaskDbContext repositoryContext)
    : RepositoryBase<BoardAccess, BoardTaskDbContext>(repositoryContext),
      IBoardAccessRepository
{
}