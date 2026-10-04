using BoardTaskService.Application.BoardModule.Contract.IService;
using BoardTaskService.Application.BoardModule.Dto;
using FluentValidation;
using MediatR;
using Shared.Logging.Contracts;

namespace BoardTaskService.Application.BoardModule.Command.Update;

/// <summary>Represents a command to update the details of an existing board.</summary>
/// <param name="ProjectId">The unique identifier of the project containing the board.</param>
/// <param name="BoardId">The unique identifier of the board to update.</param>
/// <param name="Request">The updated board details.</param>
public record UpdateBoardCommand(
    Guid ProjectId,
    Guid BoardId,
    UpdateBoardRequest Request)
    : IRequest<BoardSummaryDto>;

/// <summary>Validates the project identifier, board identifier, and updated board details supplied in an <see cref="UpdateBoardCommand"/>.</summary>
public class UpdateBoardCommandValidator
    : AbstractValidator<UpdateBoardCommand>
{
    /// <summary>Initializes a new instance of the <see cref="UpdateBoardCommandValidator"/> class and configures the validation rules.</summary>
    public UpdateBoardCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage("ProjectId cannot be empty.");

        RuleFor(x => x.BoardId)
            .NotEmpty()
            .WithMessage("BoardId cannot be empty.");

        RuleFor(x => x.Request.BoardName)
            .NotEmpty()
            .WithMessage("Board name cannot be empty.")
            .MaximumLength(150)
            .WithMessage("Board name cannot exceed 150 characters.");
    }
}

/// <summary>Handles the <see cref="UpdateBoardCommand"/> by updating the specified board through the board service.</summary>
public class UpdateBoardCommandHandler
    : IRequestHandler<UpdateBoardCommand, BoardSummaryDto>
{
    private readonly IBoardService _boardService;
    private readonly ILoggerManager<UpdateBoardCommandHandler> _logger;

    /// <summary>Initializes a new instance of the <see cref="UpdateBoardCommandHandler"/> class.</summary>
    /// <param name="boardService">Service used to update the board.</param>
    /// <param name="logger">Logger used to record board update activity.</param>
    public UpdateBoardCommandHandler(
        IBoardService boardService,
        ILoggerManager<UpdateBoardCommandHandler> logger)
    {
        _boardService = boardService;
        _logger = logger;
    }

    /// <summary>Handles the command and returns the updated board details.</summary>
    /// <param name="request">The command containing the project, board, and update details.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>The updated board summary.</returns>
    public async Task<BoardSummaryDto> Handle(
        UpdateBoardCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Updating board {BoardId}.",
            request.BoardId);

        return await _boardService.UpdateBoard(
            request.ProjectId,
            request.BoardId,
            request.Request,
            cancellationToken);
    }
}