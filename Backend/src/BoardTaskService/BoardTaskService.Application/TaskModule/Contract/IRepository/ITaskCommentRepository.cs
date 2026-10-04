using BoardTaskService.Domain.Models;
using Shared.Common.contracts;

namespace BoardTaskService.Application.TaskModule.Contract.IRepository;

/// <summary>Defines repository operations for <see cref="TaskComment"/> entities.</summary>
public interface ITaskCommentRepository : IRepositoryBase<TaskComment>
{
}
