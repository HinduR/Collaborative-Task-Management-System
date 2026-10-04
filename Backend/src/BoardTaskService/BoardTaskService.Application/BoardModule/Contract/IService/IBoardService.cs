using BoardTaskService.Application.BoardModule.Dto;

namespace BoardTaskService.Application.BoardModule.Contract.IService;

/// <summary>Defines operations for retrieving, creating, updating, and deleting boards.</summary>
public interface IBoardService
{
    /// <summary>Retrieves the boards in a project that are accessible to the current user.</summary>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>A list containing accessible board summaries.</returns>
    Task<List<BoardSummaryDto>> GetProjectBoards(
        Guid projectId,
        CancellationToken cancellationToken);

    /// <summary>Creates a board in the specified project.</summary>
    /// <param name="projectId">The unique identifier of the project in which the board will be created.</param>
    /// <param name="request">The board creation request.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>The details of the newly created board.</returns>
    Task<CreateBoardResponseDto> CreateBoard(
        Guid projectId,
        CreateBoardRequest request,
        CancellationToken cancellationToken);

    /// <summary>Updates an existing board in the specified project.</summary>
    /// <param name="projectId">The unique identifier of the project containing the board.</param>
    /// <param name="boardId">The unique identifier of the board to update.</param>
    /// <param name="request">The updated board data.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>The updated board summary.</returns>
    Task<BoardSummaryDto> UpdateBoard(
        Guid projectId,
        Guid boardId,
        UpdateBoardRequest request,
        CancellationToken cancellationToken);

    /// <summary>Deletes a board and its related records from the specified project.</summary>
    /// <param name="projectId">The unique identifier of the project containing the board.</param>
    /// <param name="boardId">The unique identifier of the board to delete.</param>
    /// <param name="confirmDelete">Indicates whether deletion is confirmed when the board contains tasks.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    Task DeleteBoard(
        Guid projectId,
        Guid boardId,
        bool confirmDelete,
        CancellationToken cancellationToken);
}
