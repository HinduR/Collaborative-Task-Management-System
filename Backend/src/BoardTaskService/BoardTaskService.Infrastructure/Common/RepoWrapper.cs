using BoardTaskService.Application.BoardModule.Contract.IRepository;
using BoardTaskService.Application.ProjectModule.Contract.IRepository;
using BoardTaskService.Application.TaskModule.Contract.IRepository;
using BoardTaskService.Infrastructure.BoardModule.Repository;
using BoardTaskService.Infrastructure.Persistence.ApplicationContext;
using BoardTaskService.Infrastructure.ProjectModule.Repository;
using BoardTaskService.Infrastructure.TaskModule.Repository;
using Shared.Common.contracts;

namespace BoardTaskService.Infrastructure.Common;

public class RepoWrapper : IRepoWrapper
{
    private readonly BoardTaskDbContext _context;
    private readonly IUserContext _userContext;

    private IProjectRepository? _projectRepository;
    private IUserProjectMappingRepository? _userProjectMappingRepository;
    private IBoardRepository? _boardRepository;
    private IBoardAccessRepository? _boardAccessRepository;
    private IWorkflowColumnRepository? _workflowColumnRepository;
    private IBoardTaskRepository? _boardTaskRepository;
    private ITaskCommentRepository? _taskCommentRepository;

    public RepoWrapper(BoardTaskDbContext context, IUserContext userContext)
    {
        _context = context;
        _userContext = userContext;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveAsync(_userContext.GetUserId(), cancellationToken);
    }

    public IProjectRepository ProjectRepository
    {
        get
        {
            if (_projectRepository == null)
            {
                _projectRepository = new ProjectRepository(_context);
            }

            return _projectRepository;
        }
    }

    public IUserProjectMappingRepository UserProjectMappingRepository
    {
        get
        {
            if (_userProjectMappingRepository == null)
            {
                _userProjectMappingRepository =
                    new UserProjectMappingRepository(_context);
            }

            return _userProjectMappingRepository;
        }
    }

    public IBoardRepository BoardRepository
    {
        get
        {
            if (_boardRepository == null)
            {
                _boardRepository = new BoardRepository(_context);
            }

            return _boardRepository;
        }
    }

    public IBoardAccessRepository BoardAccessRepository
    {
        get
        {
            if (_boardAccessRepository == null)
            {
                _boardAccessRepository = new BoardAccessRepository(_context);
            }

            return _boardAccessRepository;
        }
    }

    public IWorkflowColumnRepository WorkflowColumnRepository
    {
        get
        {
            if (_workflowColumnRepository == null)
            {
                _workflowColumnRepository =
                    new WorkflowColumnRepository(_context);
            }

            return _workflowColumnRepository;
        }
    }

    public IBoardTaskRepository BoardTaskRepository
    {
        get
        {
            if (_boardTaskRepository == null)
            {
                _boardTaskRepository = new BoardTaskRepository(_context);
            }

            return _boardTaskRepository;
        }
    }

    public ITaskCommentRepository TaskCommentRepository
    {
        get
        {
            if (_taskCommentRepository == null)
            {
                _taskCommentRepository = new TaskCommentRepository(_context);
            }
            return _taskCommentRepository;
        }
    }
}
