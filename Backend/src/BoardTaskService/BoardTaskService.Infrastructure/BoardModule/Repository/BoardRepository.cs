using BoardTaskService.Application.BoardModule.Contract.IRepository;
using BoardTaskService.Domain.Models;
using BoardTaskService.Infrastructure.Persistence.ApplicationContext;
using Shared.Common.Repository;

namespace BoardTaskService.Infrastructure.BoardModule.Repository;

/// <summary>
/// Repository for board entities.
/// </summary>
public class BoardRepository(
    BoardTaskDbContext repositoryContext)
    : RepositoryBase<Board, BoardTaskDbContext>(repositoryContext),
      IBoardRepository
{
}