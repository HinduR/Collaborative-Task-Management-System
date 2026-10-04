using BoardTaskService.Application.BoardModule.Contract.IService;
using BoardTaskService.Application.BoardModule.Dto;
using FluentValidation;
using MediatR;
using Shared.Logging.Contracts;

namespace BoardTaskService.Application.BoardModule.Command.Create;

/// <summary>
/// Represents a command to create a new board within a project.
/// </summary>
public record CreateBoardCommand(
    Guid ProjectId,
    CreateBoardRequest Request)
    : IRequest<CreateBoardResponseDto>;
/// <summary>
/// Validates the <see cref="CreateBoardCommand"/> before processing.
/// </summary>
public class CreateBoardCommandValidator
    : AbstractValidator<CreateBoardCommand>
{
    public CreateBoardCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage("ProjectId cannot be empty.");

        RuleFor(x => x.Request.Name)
            .NotEmpty()
            .WithMessage("Board name cannot be empty.")
            .MaximumLength(150)
            .WithMessage("Board name cannot exceed 150 characters.");
    }
}

/// <summary>
/// Handles the creation of a board by delegating to the board service.
/// </summary>
public class CreateBoardCommandHandler
    : IRequestHandler<CreateBoardCommand, CreateBoardResponseDto>
{
    private readonly IBoardService _boardService;
    private readonly ILoggerManager<CreateBoardCommandHandler> _logger;

    public CreateBoardCommandHandler(
        IBoardService boardService,
        ILoggerManager<CreateBoardCommandHandler> logger)
    {
        _boardService = boardService;
        _logger = logger;
    }

    /// <summary>
    /// Processes the board creation request.
    /// </summary>
    /// <param name="request">The create board command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response DTO containing the created board details.</returns>
    public async Task<CreateBoardResponseDto> Handle(
        CreateBoardCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Creating board in project {ProjectId}.",
            request.ProjectId);

        CreateBoardResponseDto result = await _boardService.CreateBoard(
            request.ProjectId,
            request.Request,
            cancellationToken);

        _logger.LogInformation("Created board {BoardId} in project {ProjectId}.",
            result.Id,
            request.ProjectId);

        return result;
    }
}