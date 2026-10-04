using BoardTaskService.Application.BoardModule.Contract.IRepository;
using BoardTaskService.Application.ProjectModule.Contract.IRepository;
using BoardTaskService.Application.TaskModule.Contract.IRepository;

namespace BoardTaskService.Infrastructure.Common;

/// <summary>
/// Provides access to repositories used within the BoardTask infrastructure layer.
/// </summary>
public interface IRepoWrapper
{
    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);

    IProjectRepository ProjectRepository { get; }

    IUserProjectMappingRepository UserProjectMappingRepository { get; }

    IBoardRepository BoardRepository { get; }

    IBoardAccessRepository BoardAccessRepository { get; }

    IWorkflowColumnRepository WorkflowColumnRepository { get; }

    IBoardTaskRepository BoardTaskRepository { get; }

    ITaskCommentRepository TaskCommentRepository { get; }
}
