using BoardTaskService.Application.BoardModule.Contract.IService;
using FluentValidation;
using MediatR;
using Shared.Logging.Contracts;

namespace BoardTaskService.Application.BoardModule.Command.Delete;

/// <summary>
/// Represents a command to delete a board from a project.
/// </summary>
public record DeleteBoardCommand(
    Guid ProjectId,
    Guid BoardId,
    bool ConfirmDelete)
    : IRequest;

/// <summary>
/// Validates the <see cref="DeleteBoardCommand"/> before processing.
/// </summary>
public class DeleteBoardCommandValidator
    : AbstractValidator<DeleteBoardCommand>
{
    public DeleteBoardCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage("ProjectId cannot be empty.");

        RuleFor(x => x.BoardId)
            .NotEmpty()
            .WithMessage("BoardId cannot be empty.");
    }
}


/// <summary>
/// Handles the deletion of a board by delegating to the board service.
/// </summary>
public class DeleteBoardCommandHandler
    : IRequestHandler<DeleteBoardCommand>
{
    private readonly IBoardService _boardService;
    private readonly ILoggerManager<DeleteBoardCommandHandler> _logger;

    public DeleteBoardCommandHandler(
        IBoardService boardService,
        ILoggerManager<DeleteBoardCommandHandler> logger)
    {
        _boardService = boardService;
        _logger = logger;
    }

    /// <summary>
    /// Processes the board deletion request.
    /// </summary>
    /// <param name="request">The delete board command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task Handle(
        DeleteBoardCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Deleting board {BoardId}. Confirmation: {ConfirmDelete}.",
            request.BoardId,
            request.ConfirmDelete);

        await _boardService.DeleteBoard(
            request.ProjectId,
            request.BoardId,
            request.ConfirmDelete,
            cancellationToken);
    }
}