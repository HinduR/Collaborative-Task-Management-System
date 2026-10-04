using BoardTaskService.Domain.Models;
using Shared.Common.contracts;

namespace BoardTaskService.Application.BoardModule.Contract.IRepository;

/// <summary>
/// Represents the IBoardRepository component.
/// </summary>
public interface IBoardRepository : IRepositoryBase<Board>
{
}
