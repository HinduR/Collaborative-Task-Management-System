using BoardTaskService.Domain.Models;
using Shared.Common.contracts;

namespace BoardTaskService.Application.ProjectModule.Contract.IRepository;
/// <summary>
/// Defines repository operations for <see cref="Project"/> entities using the shared generic repository functionality.
/// </summary>
public interface IProjectRepository : IRepositoryBase<Project>
{
}
