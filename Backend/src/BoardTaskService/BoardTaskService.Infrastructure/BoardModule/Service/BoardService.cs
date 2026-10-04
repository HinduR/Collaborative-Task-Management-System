using BoardTaskService.Application.BoardModule.Contract.IService;
using BoardTaskService.Application.BoardModule.Dto;
using BoardTaskService.Infrastructure.Common;
using BoardTaskService.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Shared.Common.contracts;
using Shared.Exceptions.Infrastructure;
using Shared.Logging.Contracts;

namespace BoardTaskService.Infrastructure.BoardModule.Service;

/// <summary>Handles board and board-access business operations.</summary>
public class BoardService : IBoardService
{
    private readonly IRepoWrapper _repoWrapper;
    private readonly IUserContext _userContext;
    private readonly ILoggerManager<BoardService> _logger;

    /// <summary>Initializes a new instance of the <see cref="BoardService"/> class.</summary>
    /// <param name="repoWrapper">Provides access to the required repositories.</param>
    /// <param name="userContext">Provides information about the current authenticated user.</param>
    /// <param name="logger">Logger used to record board operations.</param>
    public BoardService(
        IRepoWrapper repoWrapper,
        IUserContext userContext,
        ILoggerManager<BoardService> logger)
    {
        _repoWrapper = repoWrapper;
        _userContext = userContext;
        _logger = logger;
    }

    /// <summary>Retrieves the boards in a project that are accessible to the current user.</summary>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>A list containing accessible boards and their permitted workflow columns.</returns>
    public async Task<List<BoardSummaryDto>> GetProjectBoards(
       Guid projectId,
       CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Executing GetProjectBoards.");

        Guid currentUserId = _userContext.GetUserId();
        Guid currentRoleId = _userContext.GetRoleId();

        _logger.LogDebug("Fetching accessible boards for project {ProjectId} requested by user {UserId}.", projectId, currentUserId);

        bool projectExists = await _repoWrapper.ProjectRepository
            .AnyByConditionAsync(
                project =>
                    project.IsActive &&
                    project.Id == projectId,
                cancellationToken);

        if (!projectExists)
        {
            _logger.LogError("Board retrieval failed because project {ProjectId} was not found.", null, projectId);

            throw new NotFoundCustomException(
                "The selected project does not exist.",
                "Project not found.");
        }

        UserProjectMapping? userProjectMapping =
            await _repoWrapper.UserProjectMappingRepository
                .FindFirstByConditionAsync(
                    mapping =>
                        mapping.IsActive &&
                        mapping.UserId == currentUserId &&
                        mapping.ProjectId == projectId,
                    cancellationToken);

        if (userProjectMapping is null)
        {
            _logger.LogError("Board retrieval denied because user {UserId} does not have access to project {ProjectId}.", null, currentUserId, projectId);

            throw new ForBiddenCustomException(
                "You do not have access to this project.",
                "Access denied.");
        }

        List<Guid> accessibleBoardIds = await _repoWrapper
            .BoardAccessRepository
            .FindByCondition(access =>
                access.IsActive &&
                access.UserProjectMappingId ==
                    userProjectMapping.Id)
            .Select(access => access.BoardId)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (accessibleBoardIds.Count == 0)
        {
            _logger.LogDebug("No accessible boards were found for user {UserId} in project {ProjectId}.", currentUserId, projectId);

            return [];
        }

        List<BoardSummaryDto> boardList = await _repoWrapper
            .BoardRepository
            .FindByCondition(board =>
                board.IsActive &&
                board.ProjectId == projectId &&
                accessibleBoardIds.Contains(board.Id))
            .OrderBy(board => board.Name)
            .Select(board => new BoardSummaryDto
            {
                Id = board.Id,
                BoardName = board.Name,
                OwnerUserId = board.CreatedBy,
                IsBoardOwner = board.CreatedBy == currentUserId,
                Columns = new List<WorkflowColumnDto>()
            })
            .ToListAsync(cancellationToken);

        if (boardList.Count == 0)
        {
            _logger.LogDebug("No active boards were found for user {UserId} in project {ProjectId}.", currentUserId, projectId);

            return [];
        }

        List<Guid> boardIds = boardList
            .Select(board => board.Id)
            .ToList();

        List<Guid> ownedBoardIds = boardList
            .Where(board => board.IsBoardOwner)
            .Select(board => board.Id)
            .ToList();

        List<BoardWorkflowColumnListItem> workflowColumnList = await _repoWrapper
            .WorkflowColumnRepository
            .FindByCondition(column =>
                column.IsActive &&
                boardIds.Contains(column.BoardId) &&

                (
                    ownedBoardIds.Contains(column.BoardId) ||

                    column.RoleId.Length == 0 ||

                    column.RoleId.Contains(currentRoleId)
                ))
            .OrderBy(column => column.SortOrder)
            .Select(column => new BoardWorkflowColumnListItem
            {
                BoardId = column.BoardId,

                Column = new WorkflowColumnDto
                {
                    Id = column.Id,
                    ColumnName = column.Name,
                    SortOrder = column.SortOrder,
                    RoleIds = column.RoleId.ToList()
                }
            })
            .ToListAsync(cancellationToken);

        Dictionary<Guid, List<WorkflowColumnDto>>
            columnsByBoardId = workflowColumnList
                .GroupBy(item => item.BoardId)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .Select(item => item.Column)
                        .ToList());

        foreach (BoardSummaryDto board in boardList)
        {
            board.Columns = columnsByBoardId.GetValueOrDefault(
                board.Id,
                []);
        }

        _logger.LogDebug("Fetched {BoardCount} accessible boards with {ColumnCount} permitted workflow columns for project {ProjectId}.", boardList.Count, workflowColumnList.Count, projectId);

        return boardList;
    }

    /// <summary>Creates a board in the specified project and grants access to its owner.</summary>
    /// <param name="projectId">The unique identifier of the project in which the board will be created.</param>
    /// <param name="request">The board creation request.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>The details of the newly created board.</returns>
    public async Task<CreateBoardResponseDto> CreateBoard(
        Guid projectId,
        CreateBoardRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Executing CreateBoard.");

        Guid currentUserId = _userContext.GetUserId();

        _logger.LogDebug("Creating a board in project {ProjectId} requested by user {UserId}.", projectId, currentUserId);

        bool projectExists = await _repoWrapper.ProjectRepository
            .AnyByConditionAsync(
                project => project.IsActive && project.Id == projectId,
                cancellationToken);

        if (!projectExists)
        {
            _logger.LogError("Board creation failed because project {ProjectId} was not found.", null, projectId);

            throw new NotFoundCustomException(
                "The selected project does not exist.",
                "Project not found.");
        }

        UserProjectMapping? ownerProjectMapping = await _repoWrapper
            .UserProjectMappingRepository
            .FindFirstByConditionAsync(
                mapping =>
                    mapping.IsActive &&
                    mapping.UserId == currentUserId &&
                    mapping.ProjectId == projectId,
                cancellationToken);

        if (ownerProjectMapping == null)
        {
            _logger.LogError("Board creation denied because user {UserId} is not assigned to project {ProjectId}.", null, currentUserId, projectId);

            throw new ForBiddenCustomException(
                "You must be assigned to the project before creating a board.",
                "Project access required.");
        }

        string boardName = request.Name.Trim();

        bool duplicateBoardExists = await _repoWrapper.BoardRepository
            .AnyByConditionAsync(
                board =>
                    board.IsActive &&
                    board.ProjectId == projectId &&
                    board.Name.ToLower() == boardName.ToLower(),
                cancellationToken);

        if (duplicateBoardExists)
        {
            _logger.LogError("Board creation failed because a board named {BoardName} already exists in project {ProjectId}.", null, boardName, projectId);

            throw new BadRequestCustomException(
                "A board with this name already exists in the selected project.",
                "Duplicate board name.");
        }

        Guid boardId = Guid.NewGuid();

        Board board = new Board
        {
            Id = boardId,
            ProjectId = projectId,
            Name = boardName,
            IsActive = true
        };

        BoardAccess ownerBoardAccess = new BoardAccess
        {
            Id = Guid.NewGuid(),
            BoardId = boardId,
            UserProjectMappingId = ownerProjectMapping.Id,
            IsActive = true
        };

        _repoWrapper.BoardRepository.Create(board);

        _repoWrapper.BoardAccessRepository.Create(ownerBoardAccess);

        await _repoWrapper.SaveChangesAsync(cancellationToken);

        _logger.LogDebug(
            "Board {BoardId} created in project {ProjectId} by user {UserId}.",
            boardId,
            projectId,
            currentUserId);

        return new CreateBoardResponseDto
        {
            Id = boardId,
            Name = boardName,
            OwnerUserId = currentUserId,
        };
    }

    /// <summary>Updates the name of an existing board when requested by the Board Owner.</summary>
    /// <param name="projectId">The unique identifier of the project containing the board.</param>
    /// <param name="boardId">The unique identifier of the board to update.</param>
    /// <param name="request">The updated board data.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>The updated board summary.</returns>
    public async Task<BoardSummaryDto> UpdateBoard(
        Guid projectId,
        Guid boardId,
        UpdateBoardRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Executing UpdateBoard.");

        Guid currentUserId = _userContext.GetUserId();

        _logger.LogDebug("Updating board {BoardId} in project {ProjectId} requested by user {UserId}.", boardId, projectId, currentUserId);

        Board? board = await _repoWrapper.BoardRepository
            .FindFirstByConditionAsync(
                item =>
                    item.IsActive &&
                    item.Id == boardId &&
                    item.ProjectId == projectId,
                cancellationToken);

        if (board == null)
        {
            _logger.LogError("Board update failed because board {BoardId} was not found in project {ProjectId}.", null, boardId, projectId);

            throw new NotFoundCustomException(
                "The selected board does not exist in this project.",
                "Board not found.");
        }

        if (board.CreatedBy != _userContext.GetUserId())
        {
            _logger.LogError("Board update denied because user {UserId} is not the owner of board {BoardId}.", null, currentUserId, boardId);

            throw new ForBiddenCustomException(
                "Only the Board Owner can update this board.",
                "Access denied.");
        }

        string boardName = request.BoardName.Trim();

        bool duplicateBoardExists = await _repoWrapper.BoardRepository
            .AnyByConditionAsync(
                item =>
                    item.IsActive &&
                    item.Id != boardId &&
                    item.ProjectId == projectId &&
                    item.Name.ToLower() == boardName.ToLower(),
                cancellationToken);

        if (duplicateBoardExists)
        {
            _logger.LogError("Board update failed because a board named {BoardName} already exists in project {ProjectId}.", null, boardName, projectId);

            throw new BadRequestCustomException(
                "A board with this name already exists in the selected project.",
                "Duplicate board name.");
        }

        board.Name = boardName;

        await _repoWrapper.SaveChangesAsync(cancellationToken);

        _logger.LogDebug(
            "Board {BoardId} updated by user {UserId}.",
            boardId,
            _userContext.GetUserId());

        return new BoardSummaryDto
        {
            Id = board.Id,
            BoardName = board.Name,
            OwnerUserId = board.CreatedBy
        };
    }

    /// <summary>Soft-deletes a board and its related workflow columns, tasks, comments, and board access records.</summary>
    /// <param name="projectId">The unique identifier of the project containing the board.</param>
    /// <param name="boardId">The unique identifier of the board to delete.</param>
    /// <param name="confirmDelete">Indicates whether deletion has been confirmed when the board contains tasks.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    public async Task DeleteBoard(
        Guid projectId,
        Guid boardId,
        bool confirmDelete,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Executing DeleteBoard.");

        Guid currentUserId = _userContext.GetUserId();

        _logger.LogDebug("Deleting board {BoardId} from project {ProjectId} requested by user {UserId}.", boardId, projectId, currentUserId);

        Board? board = await _repoWrapper.BoardRepository
            .FindFirstByConditionAsync(
                item =>
                    item.IsActive &&
                    item.Id == boardId &&
                    item.ProjectId == projectId,
                cancellationToken);

        if (board == null)
        {
            _logger.LogError("Board deletion failed because board {BoardId} was not found in project {ProjectId}.", null, boardId, projectId);

            throw new NotFoundCustomException(
                "The selected board does not exist in this project.",
                "Board not found.");
        }

        if (board.CreatedBy != _userContext.GetUserId())
        {
            _logger.LogError("Board deletion denied because user {UserId} is not the owner of board {BoardId}.", null, currentUserId, boardId);

            throw new ForBiddenCustomException(
                "Only the Board Owner can delete this board.",
                "Access denied.");
        }

        List<WorkflowColumn> workflowColumns = await _repoWrapper
            .WorkflowColumnRepository
            .FindByCondition(column =>
                column.IsActive &&
                column.BoardId == boardId)
            .ToListAsync(cancellationToken);

        List<Guid> workflowColumnIds = workflowColumns
            .Select(column => column.Id)
            .ToList();

        List<BoardTask> tasks = await _repoWrapper.BoardTaskRepository
            .FindByCondition(task =>
                task.IsActive &&
                workflowColumnIds.Contains(task.WorkflowColumnId))
            .ToListAsync(cancellationToken);

        if (tasks.Any() && !confirmDelete)
        {
            _logger.LogError("Board deletion requires confirmation because board {BoardId} contains {TaskCount} active tasks.", null, boardId, tasks.Count);

            throw new BadRequestCustomException(
                "This board contains tasks. Confirm deletion before deleting it.",
                "Board deletion confirmation required.");
        }

        List<Guid> taskIds = tasks
            .Select(task => task.Id)
            .ToList();

        List<TaskComment> comments = await _repoWrapper.TaskCommentRepository
            .FindByCondition(comment =>
                comment.IsActive &&
                taskIds.Contains(comment.TaskId))
            .ToListAsync(cancellationToken);

        List<BoardAccess> boardAccesses = await _repoWrapper
            .BoardAccessRepository
            .FindByCondition(access =>
                access.IsActive &&
                access.BoardId == boardId)
            .ToListAsync(cancellationToken);

        board.IsActive = false;

        foreach (WorkflowColumn workflowColumn in workflowColumns)
        {
            workflowColumn.IsActive = false;
        }

        foreach (BoardTask task in tasks)
        {
            task.IsActive = false;
        }

        foreach (TaskComment comment in comments)
        {
            comment.IsActive = false;
        }

        foreach (BoardAccess boardAccess in boardAccesses)
        {
            boardAccess.IsActive = false;
        }

        _repoWrapper.BoardRepository.Update(board);

        if (workflowColumns.Any())
        {
            _repoWrapper.WorkflowColumnRepository
                .UpdateRange(workflowColumns);
        }

        if (tasks.Any())
        {
            _repoWrapper.BoardTaskRepository.UpdateRange(tasks);
        }

        if (comments.Any())
        {
            _repoWrapper.TaskCommentRepository.UpdateRange(comments);
        }

        if (boardAccesses.Any())
        {
            _repoWrapper.BoardAccessRepository.UpdateRange(boardAccesses);
        }

        await _repoWrapper.SaveChangesAsync(cancellationToken);

        _logger.LogDebug(
            "Board {BoardId} deleted by user {UserId}. Workflow columns: {ColumnCount}, tasks: {TaskCount}, comments: {CommentCount}, access records: {AccessCount}.",
            boardId,
            _userContext.GetUserId(),
            workflowColumns.Count,
            tasks.Count,
            comments.Count,
            boardAccesses.Count);
    }

    /// <summary>
    /// Represents a permitted workflow column grouped by board.
    /// </summary>
    private sealed class BoardWorkflowColumnListItem
    {
        /// <summary>Gets or sets the board identifier.</summary>
        public Guid BoardId { get; set; }

        /// <summary>Gets or sets the workflow column returned to clients.</summary>
        public WorkflowColumnDto Column { get; set; } = new();
    }
}
