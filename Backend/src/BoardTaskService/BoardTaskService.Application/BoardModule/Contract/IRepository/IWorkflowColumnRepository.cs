using BoardTaskService.Domain.Models;
using Shared.Common.contracts;

/// <summary>
/// Defines repository operations for accessing and managing
/// <see cref="WorkflowColumn"/> entities.
/// </summary>
namespace BoardTaskService.Application.BoardModule.Contract.IRepository;
/// <summary>
/// Represents the IWorkflowColumnRepository component.
/// </summary>
public interface IWorkflowColumnRepository
    : IRepositoryBase<WorkflowColumn>
{
}
