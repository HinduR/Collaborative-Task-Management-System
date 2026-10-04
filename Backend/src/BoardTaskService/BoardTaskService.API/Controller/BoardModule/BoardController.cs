using Microsoft.AspNetCore.Mvc;
using MediatR;
using Shared.Logging.Contracts;
using Swashbuckle.AspNetCore.Annotations;
using BoardTaskService.Application.BoardModule.Dto;
using BoardTaskService.Application.BoardModule.Query.List;
using BoardTaskService.Application.BoardModule.Command.Update;
using BoardTaskService.Application.BoardModule.Command.Create;
using BoardTaskService.Application.BoardModule.Command.Delete;
using Shared.Authorisation.Domain.Attribute;

namespace BoardTaskService.API.Controller.BoardModule;

/// <summary>
/// This class handles requests related to Calculation.
/// </summary>
[ApiController]
[Route("board-task/")]
public class BoardController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILoggerManager<BoardController> _logger;
    public BoardController(
        IMediator mediator,
        ILoggerManager<BoardController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }


    /// <summary>
    /// Retrieves the list of users who have access to the specified board.
    /// </summary>
    /// <param name="boardId">The unique identifier of the board.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>A list of users with access to the board.</returns>

    [HttpGet("boards/{board-id}/access")]
    [CustomAuthorize("TEAM_MEMBER_ACCESS")]
    [SwaggerResponse(StatusCodes.Status200OK, "Board access users returned successfully.", typeof(List<BoardAccessUserDto>))]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "You do not have access to this board.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "The selected board does not exist.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error.")]
    public async Task<IActionResult> GetBoardAccess(
        [FromRoute(Name = "board-id")] Guid boardId,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Received request to fetch board access for board {BoardId}.", boardId);

        List<BoardAccessUserDto> result = await _mediator.Send(
            new GetBoardAccessQuery(boardId),
            cancellationToken);
        _logger.LogDebug("Board access fetch completed for board {BoardId}. {Count} users returned.", boardId, result.Count);

        return Ok(result);
    }



    /// <summary>
    /// Replaces the full set of users who have access to the specified board.
    /// </summary>
    /// <param name="boardId">The unique identifier of the board.</param>
    /// <param name="request">The desired list of user project mapping identifiers to grant access.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>

    [HttpPut("boards/{board-id}/access")]
    [CustomAuthorize("TEAM_MEMBER_ACCESS")]
    [SwaggerResponse(StatusCodes.Status200OK, "Board access updated successfully.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid board access data.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Only the board owner can update board access.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "The selected board does not exist.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error.")]
    public async Task<IActionResult> ReplaceBoardAccess(
        [FromRoute(Name = "board-id")] Guid boardId,
        [FromBody] UpdateBoardAccessRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Received request to replace board access for board {BoardId}.", boardId);

        await _mediator.Send(
            new UpdateBoardAccessCommand(
                boardId,
                request.UserProjectMappingIdList),
            cancellationToken);
        _logger.LogDebug("Board access replacement completed for board {BoardId}.", boardId);

        return Ok();
    }

    /// <summary>
    /// Retrieves the list of boards belonging to the specified project.
    /// </summary>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>A list of boards within the project.</returns>

    [HttpGet("projects/{project-id}/boards")]
    [CustomAuthorize("TEAM_MEMBER_ACCESS")]
    [SwaggerResponse(StatusCodes.Status200OK, "Accessible boards returned successfully.", typeof(List<BoardSummaryDto>))]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "You do not have access to this project.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "The selected project does not exist.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error.")]
    public async Task<IActionResult> GetProjectBoards(
        [FromRoute(Name = "project-id")] Guid projectId,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Received request to fetch boards for project {ProjectId}.", projectId);

        List<BoardSummaryDto> result = await _mediator.Send(
            new GetProjectBoardsQuery(projectId),
            cancellationToken);
        _logger.LogDebug("Board list fetch completed for project {ProjectId}. {Count} boards returned.", projectId, result.Count);

        return Ok(result);
    }

    /// <summary>
    /// Creates a new board within the specified project.
    /// </summary>
    /// <param name="projectId">The unique identifier of the project the board will belong to.</param>
    /// <param name="request">The board creation details.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>The newly created board.</returns>
    [HttpPost("projects/{project-id}/boards")]
    [CustomAuthorize("TEAM_MEMBER_ACCESS")]
    [SwaggerResponse(StatusCodes.Status201Created, "Board created successfully.", typeof(CreateBoardResponseDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid board creation data or duplicate board name.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "You do not have access to this project.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "The selected project does not exist.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error.")]
    public async Task<IActionResult> CreateBoard(
        [FromRoute(Name = "project-id")] Guid projectId,
        [FromBody] CreateBoardRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Received request to create a board for project {ProjectId}.", projectId);

        CreateBoardResponseDto result = await _mediator.Send(
            new CreateBoardCommand(projectId, request),
            cancellationToken);

        _logger.LogDebug("Board creation completed for project {ProjectId}. Board {BoardId} created.", projectId, result.Id);

        return Created("", result);
    }

    /// <summary>
    /// Updates the details of an existing board.
    /// </summary>
    /// <param name="projectId">The unique identifier of the project the board belongs to.</param>
    /// <param name="boardId">The unique identifier of the board to update.</param>
    /// <param name="request">The updated board details.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>The updated board.</returns>
    [HttpPatch("projects/{project-id}/boards/{board-id}")]
    [CustomAuthorize("TEAM_MEMBER_ACCESS")]
    [SwaggerResponse(StatusCodes.Status200OK, "Board updated successfully.", typeof(BoardSummaryDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid board update data.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Only the board owner can update this board.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "The selected project or board does not exist.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error.")]
    public async Task<IActionResult> UpdateBoard(
        [FromRoute(Name = "project-id")] Guid projectId,
        [FromRoute(Name = "board-id")] Guid boardId,
        [FromBody] UpdateBoardRequest request,
        CancellationToken cancellationToken)
    {

        _logger.LogDebug("Received request to update board {BoardId} for project {ProjectId}.", boardId, projectId);

        BoardSummaryDto result = await _mediator.Send(
            new UpdateBoardCommand(projectId, boardId, request),
            cancellationToken);
        _logger.LogDebug("Board update completed for board {BoardId}.", boardId);

        return Ok(result);
    }

    /// <summary>
    /// Deletes the specified board.
    /// </summary>
    /// <param name="projectId">The unique identifier of the project the board belongs to.</param>
    /// <param name="boardId">The unique identifier of the board to delete.</param>
    /// <param name="confirmDelete">A confirmation flag required to proceed with deletion.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    [HttpDelete("projects/{project-id}/boards/{board-id}")]
    [CustomAuthorize("TEAM_MEMBER_ACCESS")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Board deleted successfully.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Deletion was not confirmed.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Only the board owner can delete this board.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "The selected project or board does not exist.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error.")]
    public async Task<IActionResult> DeleteBoard(
        [FromRoute(Name = "project-id")] Guid projectId,
        [FromRoute(Name = "board-id")] Guid boardId,
        [FromQuery] bool confirmDelete,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Received request to delete board {BoardId} for project {ProjectId}.", boardId, projectId);

        await _mediator.Send(
            new DeleteBoardCommand(projectId, boardId, confirmDelete),
            cancellationToken);
        _logger.LogDebug("Board deletion completed for board {BoardId}.", boardId);

        return NoContent();
    }

    /// <summary>
    /// Replaces the full set of workflow columns for a board, applying creates, renames, reorders, and soft-deletes based on the supplied column list.
    /// </summary>
    /// <param name="boardId">The unique identifier of the board whose workflow columns are being updated.</param>
    /// <param name="request">The complete desired state of the board's workflow columns.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>The saved list of workflow columns.</returns>
    [HttpPut("boards/{board-id}/workflow-columns")]
    [CustomAuthorize("TEAM_MEMBER_ACCESS")]
    [SwaggerResponse(StatusCodes.Status200OK, "Workflow columns saved successfully.", typeof(List<WorkflowColumnDto>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid workflow column data.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Only the board owner can update workflow columns.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "The selected board does not exist.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error.")]
    public async Task<IActionResult> UpdateWorkflowColumns(
        [FromRoute(Name = "board-id")] Guid boardId,
        [FromBody] UpdateWorkflowColumnsRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Received request to update workflow columns for board {BoardId}.", boardId);

        List<WorkflowColumnDto> result = await _mediator.Send(
            new UpdateWorkflowColumnCommand(boardId, request),
            cancellationToken);
        _logger.LogDebug("Workflow columns update completed for board {BoardId}. {Count} columns returned.", boardId, result.Count);

        return Ok(result);
    }
}