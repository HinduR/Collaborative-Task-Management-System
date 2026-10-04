using BoardTaskService.Application.TaskModule.Contract.IService;
using FluentValidation;
using MediatR;
using Shared.Logging.Contracts;

namespace BoardTaskService.Application.TaskModule.Command.Delete;

/// <summary>Represents a command to delete a task from a board.</summary>
/// <param name="BoardId">The unique identifier of the board containing the task.</param>
/// <param name="TaskId">The unique identifier of the task to delete.</param>
public record DeleteTaskCommand(
    Guid BoardId,
    Guid TaskId) : IRequest;

/// <summary>Validates the board and task identifiers supplied in a <see cref="DeleteTaskCommand"/>.</summary>
public class DeleteTaskCommandValidator : AbstractValidator<DeleteTaskCommand>
{
    /// <summary>Initializes a new instance of the <see cref="DeleteTaskCommandValidator"/> class and configures the validation rules.</summary>
    public DeleteTaskCommandValidator()
    {
        RuleFor(x => x.BoardId).NotEmpty();
        RuleFor(x => x.TaskId).NotEmpty();
    }
}

/// <summary>Handles the <see cref="DeleteTaskCommand"/> by deleting the specified task through the task service.</summary>
public class DeleteTaskCommandHandler
    : IRequestHandler<DeleteTaskCommand>
{
    private readonly ITaskService _taskService;
    private readonly ILoggerManager<DeleteTaskCommandHandler> _logger;

    /// <summary>Initializes a new instance of the <see cref="DeleteTaskCommandHandler"/> class.</summary>
    /// <param name="taskService">Service used to delete the task.</param>
    /// <param name="logger">Logger used to record task deletion activity.</param>
    public DeleteTaskCommandHandler(
        ITaskService taskService,
        ILoggerManager<DeleteTaskCommandHandler> logger)
    {
        _taskService = taskService;
        _logger = logger;
    }

    /// <summary>Handles the command and deletes the specified task.</summary>
    /// <param name="request">The command containing the board and task identifiers.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    public async Task Handle(
        DeleteTaskCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Deleting task {TaskId} from board {BoardId}.",
            request.TaskId,
            request.BoardId);

        await _taskService.DeleteTask(
            request.BoardId,
            request.TaskId,
            cancellationToken);
    }
}