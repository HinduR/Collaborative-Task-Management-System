using BoardTaskService.Application.BoardModule.Dto;

namespace BoardTaskService.Application.BoardModule.Contract.IService;

/// <summary>Defines operations for retrieving and replacing user access to boards.</summary>
public interface IBoardAccessService
{
    /// <summary>Retrieves the active users who have access to the specified board.</summary>
    /// <param name="boardId">The unique identifier of the board.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>A list containing users who currently have access to the board.</returns>
    Task<List<BoardAccessUserDto>> GetBoardAccess(
        Guid boardId,
        CancellationToken cancellationToken);

    /// <summary>Replaces the users who have access to a board while retaining access for the Board Owner.</summary>
    /// <param name="boardId">The unique identifier of the board whose access is being updated.</param>
    /// <param name="userProjectMappingIds">The user-project mapping identifiers that should have access to the board.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    Task ReplaceBoardAccess(
        Guid boardId,
        List<Guid> userProjectMappingIds,
        CancellationToken cancellationToken);
}
