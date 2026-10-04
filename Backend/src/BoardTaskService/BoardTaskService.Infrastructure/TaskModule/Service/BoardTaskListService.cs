using BoardTaskService.Application.TaskModule.Contract.IService;
using BoardTaskService.Application.TaskModule.Dto;
using BoardTaskService.Domain.Models;
using BoardTaskService.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Shared.Common.contracts;
using Shared.Common.Dto;
using Shared.Exceptions.Infrastructure;
using Shared.Grpc.Common.Dto;
using Shared.Grpc.Contracts;
using Shared.Logging.Contracts;

namespace BoardTaskService.Infrastructure.TaskModule.Service;

/// <summary>Provides operations for retrieving tasks accessible to the current user within a board.</summary>
public class BoardTaskListService : IBoardTaskListService
{
    private const string PriorityRefSetKey = "PRIORITY";
    private const string TaskTypeRefSetKey = "TASK_TYPE";

    private readonly IRepoWrapper _repoWrapper;
    private readonly IUserContext _userContext;
    private readonly IIdentityGrpcHelperService _identityGrpcHelperService;
    private readonly IMetadataGrpcHelperService _metadataGrpcHelperService;
    private readonly ILoggerManager<BoardTaskListService> _logger;

    /// <summary>Initializes a new instance of the <see cref="BoardTaskListService"/> class.</summary>
    /// <param name="repoWrapper">Provides access to the required repositories.</param>
    /// <param name="userContext">Provides information about the current authenticated user.</param>
    /// <param name="identityGrpcHelperService">Provides access to user information from the Identity service.</param>
    /// <param name="metadataGrpcHelperService">Provides access to reference terms from the Metadata service.</param>
    /// <param name="logger">Logger used to record board task retrieval activity.</param>
    public BoardTaskListService(
        IRepoWrapper repoWrapper,
        IUserContext userContext,
        IIdentityGrpcHelperService identityGrpcHelperService,
        IMetadataGrpcHelperService metadataGrpcHelperService,
        ILoggerManager<BoardTaskListService> logger)
    {
        _repoWrapper = repoWrapper;
        _userContext = userContext;
        _identityGrpcHelperService = identityGrpcHelperService;
        _metadataGrpcHelperService = metadataGrpcHelperService;
        _logger = logger;
    }

    /// <summary>Retrieves the active tasks from the specified board when the current user has project and board access.</summary>
    /// <param name="boardId">The unique identifier of the board whose tasks are being retrieved.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>A list containing the accessible board task summaries.</returns>
    public async Task<List<TaskSummaryDto>> GetBoardTasksAsync(
        Guid boardId,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Executing GetBoardTasksAsync.");

        Guid currentUserId = _userContext.GetUserId();

        _logger.LogInformation(
            "Fetching tasks for board {BoardId} requested by user {UserId}.",
            boardId,
            currentUserId);

        Board? board = await _repoWrapper.BoardRepository
            .FindFirstByConditionAsync(
                item =>
                    item.IsActive &&
                    item.Id == boardId,
                cancellationToken);

        if (board is null)
        {
            _logger.LogError(
                "Task retrieval failed because board {BoardId} was not found.",
                null,
                boardId);

            throw new NotFoundCustomException(
                "The selected board does not exist.",
                "Board not found.");
        }

        UserProjectMapping? userProjectMapping =
            await _repoWrapper.UserProjectMappingRepository
                .FindFirstByConditionAsync(
                    mapping =>
                        mapping.IsActive &&
                        mapping.UserId == currentUserId &&
                        mapping.ProjectId == board.ProjectId,
                    cancellationToken);

        if (userProjectMapping is null)
        {
            _logger.LogError(
                "Task retrieval denied because user {UserId} does not have access to project {ProjectId}.",
                null,
                currentUserId,
                board.ProjectId);

            throw new ForBiddenCustomException(
                "You do not have access to this project.",
                "Access denied.");
        }

        bool hasBoardAccess = await _repoWrapper.BoardAccessRepository
            .AnyByConditionAsync(
                access =>
                    access.IsActive &&
                    access.BoardId == boardId &&
                    access.UserProjectMappingId ==
                        userProjectMapping.Id,
                cancellationToken);

        if (!hasBoardAccess)
        {
            _logger.LogError(
                "Task retrieval denied because user {UserId} does not have access to board {BoardId}.",
                null,
                currentUserId,
                boardId);

            throw new ForBiddenCustomException(
                "You do not have access to this board.",
                "Board access denied.");
        }

        List<BoardTaskListItem> taskList = await _repoWrapper.BoardTaskRepository
            .FindByCondition(task =>
                task.IsActive &&
                task.WorkflowColumn.IsActive &&
                task.WorkflowColumn.BoardId == boardId)
            .Select(task => new BoardTaskListItem
            {
                Id = task.Id,
                WorkflowColumnId = task.WorkflowColumnId,
                Title = task.Title,
                Description = task.Description,
                PriorityId = task.PriorityId,
                TaskTypeId = task.TaskTypeId,
                CreatedBy = task.CreatedBy,

                AssigneeUserId =
                    task.AssigneeBoardAccessId == null
                        ? (Guid?)null
                        : task.AssigneeBoardAccess!
                            .UserProjectMapping.UserId
            })
            .ToListAsync(cancellationToken);

        if (taskList.Count == 0)
        {
            _logger.LogInformation(
                "No active tasks were found for board {BoardId}.",
                boardId);

            return [];
        }

        _logger.LogInformation(
            "Retrieved {TaskCount} active task records for board {BoardId}. Fetching related user and metadata details.",
            taskList.Count,
            boardId);

        List<Guid> assigneeUserIds = taskList
            .Where(task => task.AssigneeUserId.HasValue)
            .Select(task => task.AssigneeUserId!.Value)
            .Distinct()
            .ToList();

        Task<List<UserDto>> userListTask =
            _identityGrpcHelperService.GetUserListAsync(
                assigneeUserIds,
                cancellationToken);

        Task<List<RefTermDto>> priorityListTask =
            _metadataGrpcHelperService
                .GetRefTermListByRefSetKeyAsync(
                    PriorityRefSetKey,
                    cancellationToken);

        Task<List<RefTermDto>> taskTypeListTask =
            _metadataGrpcHelperService
                .GetRefTermListByRefSetKeyAsync(
                    TaskTypeRefSetKey,
                    cancellationToken);

        await Task.WhenAll(
            userListTask,
            priorityListTask,
            taskTypeListTask);

        List<UserDto> userList = await userListTask;
        List<RefTermDto> priorityList =
            await priorityListTask;
        List<RefTermDto> taskTypeList =
            await taskTypeListTask;

        Dictionary<Guid, string> userNameById =
            userList.ToDictionary(
                user => user.Id,
                user => user.DisplayName);

        Dictionary<Guid, string> priorityNameById =
            priorityList.ToDictionary(
                priority => priority.RefTermId,
                priority => priority.RefTermKey);

        Dictionary<Guid, string> taskTypeNameById =
            taskTypeList.ToDictionary(
                taskType => taskType.RefTermId,
                taskType => taskType.RefTermKey);

        List<TaskSummaryDto> result = taskList
            .Select(task => new TaskSummaryDto
            {
                Id = task.Id,
                WorkflowColumnId =
                    task.WorkflowColumnId,
                Title = task.Title,
                Description = task.Description,

                PriorityName =
                    priorityNameById.GetValueOrDefault(
                        task.PriorityId,
                        string.Empty),

                TaskTypeName =
                    taskTypeNameById.GetValueOrDefault(
                        task.TaskTypeId,
                        string.Empty),

                AssigneeUserId =
                    task.AssigneeUserId,

                AssigneeName =
                    task.AssigneeUserId.HasValue
                        ? userNameById.GetValueOrDefault(
                            task.AssigneeUserId.Value)
                        : null,

                IsTaskOwner =
                    task.CreatedBy == currentUserId
            })
            .ToList();

        _logger.LogInformation(
            "{TaskCount} tasks returned for board {BoardId}.",
            result.Count,
            boardId);

        return result;
    }

    /// <summary>
    /// Represents the flattened task data needed to build task summaries.
    /// </summary>
    private sealed class BoardTaskListItem
    {
        /// <summary>Gets or sets the task identifier.</summary>
        public Guid Id { get; set; }

        /// <summary>Gets or sets the workflow column identifier.</summary>
        public Guid WorkflowColumnId { get; set; }

        /// <summary>Gets or sets the task title.</summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>Gets or sets the task description.</summary>
        public string? Description { get; set; }

        /// <summary>Gets or sets the priority reference-term identifier.</summary>
        public Guid PriorityId { get; set; }

        /// <summary>Gets or sets the task-type reference-term identifier.</summary>
        public Guid TaskTypeId { get; set; }

        /// <summary>Gets or sets the user that created the task.</summary>
        public Guid CreatedBy { get; set; }

        /// <summary>Gets or sets the assigned user identifier, when assigned.</summary>
        public Guid? AssigneeUserId { get; set; }
    }
}
