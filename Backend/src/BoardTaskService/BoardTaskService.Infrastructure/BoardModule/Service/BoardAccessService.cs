using BoardTaskService.Application.BoardModule.Contract.IService;
using BoardTaskService.Application.BoardModule.Dto;
using BoardTaskService.Infrastructure.Common;
using BoardTaskService.Application.ProjectModule.Contract.IRepository;
using BoardTaskService.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Shared.Common.contracts;
using Shared.Exceptions.Infrastructure;
using Shared.Logging.Contracts;

namespace BoardTaskService.Infrastructure.BoardModule.Service;

/// <summary>Provides data access and authorization operations for managing user access to boards.</summary>
public class BoardAccessService : IBoardAccessService
{
    private readonly IRepoWrapper _repoWrapper;
    private readonly IUserContext _userContext;
    private readonly ILoggerManager<BoardAccessService> _logger;

    /// <summary>Initializes a new instance of the <see cref="BoardAccessService"/> class.</summary>
    /// <param name="repoWrapper">Provides access to the required repositories.</param>
    /// <param name="userContext">Provides information about the current authenticated user.</param>
    /// <param name="logger">Logger used to record board access activity.</param>
    public BoardAccessService(
        IRepoWrapper repoWrapper,
        IUserContext userContext,
        ILoggerManager<BoardAccessService> logger)
    {
        _repoWrapper = repoWrapper;
        _userContext = userContext;
        _logger = logger;
    }

    /// <summary>Retrieves the active users who have access to the specified board.</summary>
    /// <param name="boardId">The unique identifier of the board.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>A list containing users who currently have access to the board.</returns>
    public async Task<List<BoardAccessUserDto>> GetBoardAccess(
        Guid boardId,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Executing GetBoardAccess.");

        Guid currentUserId = _userContext.GetUserId();

        _logger.LogInformation("Fetching board access for board {BoardId} requested by user {UserId}.", boardId, currentUserId);

        Board? board = await _repoWrapper.BoardRepository
            .FindFirstByConditionAsync(
                board => board.IsActive && board.Id == boardId,
                cancellationToken);

        if (board == null)
        {
            _logger.LogError("Board access retrieval failed because board {BoardId} was not found.", null, boardId);

            throw new NotFoundCustomException(
                "The selected board does not exist.",
                "Board not found.");
        }

        UserProjectMapping? currentUserProjectMapping = await _repoWrapper
            .UserProjectMappingRepository
            .FindFirstByConditionAsync(
                mapping =>
                    mapping.IsActive &&
                    mapping.UserId == _userContext.GetUserId() &&
                    mapping.ProjectId == board.ProjectId,
                cancellationToken);

        if (currentUserProjectMapping == null)
        {
            _logger.LogError("Board access retrieval denied because user {UserId} does not have access to project {ProjectId}.", null, currentUserId, board.ProjectId);

            throw new ForBiddenCustomException(
                "You do not have access to this board.",
                "Access denied.");
        }

        bool hasBoardAccess = await _repoWrapper.BoardAccessRepository
            .AnyByConditionAsync(
                access =>
                    access.IsActive &&
                    access.BoardId == boardId &&
                    access.UserProjectMappingId == currentUserProjectMapping.Id,
                cancellationToken);

        if (!hasBoardAccess)
        {
            _logger.LogError("Board access retrieval denied because user {UserId} does not have access to board {BoardId}.", null, currentUserId, boardId);

            throw new ForBiddenCustomException(
                "You do not have access to this board.",
                "Access denied.");
        }

        List<BoardAccess> boardAccesses = await _repoWrapper
            .BoardAccessRepository
            .FindByCondition(access =>
                access.IsActive &&
                access.BoardId == boardId)
            .ToListAsync(cancellationToken);

        List<Guid> mappingIds = boardAccesses
            .Select(access => access.UserProjectMappingId)
            .ToList();

        List<UserProjectMapping> mappings = await _repoWrapper
            .UserProjectMappingRepository
            .FindByCondition(mapping =>
                mapping.IsActive &&
                mappingIds.Contains(mapping.Id))
            .ToListAsync(cancellationToken);

        List<BoardAccessUserDto> result = boardAccesses
            .Join(
                mappings,
                access => access.UserProjectMappingId,
                mapping => mapping.Id,
                (access, mapping) => new BoardAccessUserDto
                {
                    UserProjectMappingId = mapping.Id,
                    UserId = mapping.UserId
                })
            .ToList();

        _logger.LogInformation("Fetched {AccessCount} active board access records for board {BoardId}.", result.Count, boardId);

        return result;
    }

    /// <summary>Replaces the users who have access to a board while always retaining access for the Board Owner.</summary>
    /// <param name="boardId">The unique identifier of the board whose access is being updated.</param>
    /// <param name="userProjectMappingIds">The user-project mapping identifiers that should retain or receive board access.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    public async Task ReplaceBoardAccess(
        Guid boardId,
        List<Guid> userProjectMappingIds,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Executing ReplaceBoardAccess.");

        Guid currentUserId = _userContext.GetUserId();

        _logger.LogInformation("Replacing board access for board {BoardId} requested by user {UserId}.", boardId, currentUserId);

        Board? board = await _repoWrapper.BoardRepository
            .FindFirstByConditionAsync(
                board => board.IsActive && board.Id == boardId,
                cancellationToken);

        if (board == null)
        {
            _logger.LogError("Board access update failed because board {BoardId} was not found.", null, boardId);

            throw new NotFoundCustomException(
                "The selected board does not exist.",
                "Board not found.");
        }

        if (board.CreatedBy != _userContext.GetUserId())
        {
            _logger.LogError("Board access update denied because user {UserId} is not the owner of board {BoardId}.", null, currentUserId, boardId);

            throw new ForBiddenCustomException(
                "Only the Board Owner can update board access.",
                "Access denied.");
        }

        UserProjectMapping? ownerProjectMapping = await _repoWrapper
            .UserProjectMappingRepository
            .FindFirstByConditionAsync(
                mapping =>
                    mapping.IsActive &&
                    mapping.UserId == board.CreatedBy &&
                    mapping.ProjectId == board.ProjectId,
                cancellationToken);

        if (ownerProjectMapping == null)
        {
            _logger.LogError("Board access update failed because owner {OwnerUserId} is not actively mapped to project {ProjectId}.", null, board.CreatedBy, board.ProjectId);

            throw new BadRequestCustomException(
                "The Board Owner is not actively mapped to this project.",
                "Invalid board owner mapping.");
        }

        List<Guid> requestedMappingIds = userProjectMappingIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        // Owner access is always retained.
        requestedMappingIds.Add(ownerProjectMapping.Id);

        requestedMappingIds = requestedMappingIds
            .Distinct()
            .ToList();

        List<UserProjectMapping> validMappings = await _repoWrapper
            .UserProjectMappingRepository
            .FindByCondition(mapping =>
                mapping.IsActive &&
                mapping.ProjectId == board.ProjectId &&
                requestedMappingIds.Contains(mapping.Id))
            .ToListAsync(cancellationToken);

        if (validMappings.Count != requestedMappingIds.Count)
        {
            _logger.LogError("Board access update failed because one or more requested mappings are invalid for project {ProjectId}.", null, board.ProjectId);

            throw new BadRequestCustomException(
                "All selected users must be active members of the board's project.",
                "Invalid project user mapping.");
        }

        List<BoardAccess> existingAccesses = await _repoWrapper
            .BoardAccessRepository
            .FindByCondition(access => access.BoardId == boardId)
            .ToListAsync(cancellationToken);

        foreach (BoardAccess access in existingAccesses)
        {
            access.IsActive = requestedMappingIds.Contains(
                access.UserProjectMappingId);
        }

        List<Guid> existingMappingIds = existingAccesses
            .Select(access => access.UserProjectMappingId)
            .ToList();

        List<BoardAccess> newAccesses = requestedMappingIds
            .Where(mappingId => !existingMappingIds.Contains(mappingId))
            .Select(mappingId => new BoardAccess
            {
                Id = Guid.NewGuid(),
                BoardId = boardId,
                UserProjectMappingId = mappingId,
                IsActive = true
            })
            .ToList();

        if (existingAccesses.Any())
        {
            _repoWrapper.BoardAccessRepository.UpdateRange(existingAccesses);
        }

        if (newAccesses.Any())
        {
            await _repoWrapper.BoardAccessRepository.CreateRangeAsync(newAccesses, cancellationToken);
        }

        await _repoWrapper.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Board access updated for board {BoardId} by user {UserId}. Active mappings: {ActiveMappingCount}, new mappings: {NewMappingCount}.",
            boardId,
            _userContext.GetUserId(),
            requestedMappingIds.Count,
            newAccesses.Count);
    }
}
