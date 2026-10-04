using BoardTaskService.Application.TaskModule.Contract.IService;
using BoardTaskService.Application.TaskModule.Dto;
using FluentValidation;
using MediatR;
using Shared.Logging.Contracts;

namespace BoardTaskService.Application.TaskModule.Command.Move;

/// <summary>Represents a command to move a task to another workflow column.</summary>
/// <param name="BoardId">The unique identifier of the board containing the task.</param>
/// <param name="TaskId">The unique identifier of the task to move.</param>
/// <param name="Request">The destination workflow column information.</param>
public record MoveTaskCommand(
    Guid BoardId,
    Guid TaskId,
    MoveTaskRequestDto Request)
    : IRequest<MoveTaskResponseDto>;

/// <summary>Validates the data supplied in a <see cref="MoveTaskCommand"/>.</summary>
public class MoveTaskCommandValidator
    : AbstractValidator<MoveTaskCommand>
{
    /// <summary>Initializes a new instance of the <see cref="MoveTaskCommandValidator"/> class and configures the validation rules.</summary>
    public MoveTaskCommandValidator()
    {
        RuleFor(command => command.BoardId)
            .NotEmpty()
            .WithMessage(
                "The board identifier is required.");

        RuleFor(command => command.TaskId)
            .NotEmpty()
            .WithMessage(
                "The task identifier is required.");

        RuleFor(command => command.Request)
            .NotNull()
            .WithMessage(
                "The task movement request is required.");

        When(command => command.Request is not null, () =>
        {
            RuleFor(command =>
                    command.Request.WorkflowColumnId)
                .NotEmpty()
                .WithMessage(
                    "The destination workflow column identifier is required.");
        });
    }
}

/// <summary>Handles the <see cref="MoveTaskCommand"/> by delegating task movement to the task movement service.</summary>
public class MoveTaskCommandHandler
    : IRequestHandler<MoveTaskCommand, MoveTaskResponseDto>
{
    private readonly ITaskMovementService _taskMovementService;
    private readonly ILoggerManager<MoveTaskCommandHandler> _logger;

    /// <summary>Initializes a new instance of the <see cref="MoveTaskCommandHandler"/> class.</summary>
    /// <param name="taskMovementService">Service used to move tasks between workflow columns.</param>
    /// <param name="logger">Logger used to record task movement activity.</param>
    public MoveTaskCommandHandler(
        ITaskMovementService taskMovementService,
        ILoggerManager<MoveTaskCommandHandler> logger)
    {
        _taskMovementService = taskMovementService;
        _logger = logger;
    }

    /// <summary>Handles the command and moves the specified task to the destination workflow column.</summary>
    /// <param name="request">The command containing the board, task, and destination workflow column information.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>The task movement result.</returns>
    public async Task<MoveTaskResponseDto> Handle(
        MoveTaskCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Moving task {TaskId} on board {BoardId} to workflow column {WorkflowColumnId}.",
            request.TaskId,
            request.BoardId,
            request.Request.WorkflowColumnId);

        MoveTaskResponseDto response =
            await _taskMovementService.MoveTask(
                request.BoardId,
                request.TaskId,
                request.Request.WorkflowColumnId,
                cancellationToken);

        _logger.LogInformation(
            "Task {TaskId} moved successfully to workflow column {WorkflowColumnId}.",
            request.TaskId,
            response.WorkflowColumnId);

        return response;
    }
}