using BoardTaskService.Domain.Models;
using Shared.Common.contracts;

namespace BoardTaskService.Application.BoardModule.Contract.IRepository;

/// <summary>Defines repository operations for <see cref="BoardAccess"/> entities.</summary>
public interface IBoardAccessRepository : IRepositoryBase<BoardAccess>
{
}
