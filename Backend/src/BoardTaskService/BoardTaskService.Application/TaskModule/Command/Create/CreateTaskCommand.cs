using BoardTaskService.Application.TaskModule.Contract.IService;
using BoardTaskService.Application.TaskModule.Dto;
using FluentValidation;
using MediatR;
using Shared.Logging.Contracts;

namespace BoardTaskService.Application.TaskModule.Command.Create;

/// <summary>Represents a command to create a task within a board.</summary>
/// <param name="BoardId">The unique identifier of the board where the task will be created.</param>
/// <param name="Request">The task creation details.</param>
public record CreateTaskCommand(
    Guid BoardId,
    TaskRequestDto Request) : IRequest<TaskSummaryDto>;

/// <summary>Validates the board identifier and task details supplied in a <see cref="CreateTaskCommand"/>.</summary>
public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    /// <summary>Initializes a new instance of the <see cref="CreateTaskCommandValidator"/> class and configures the validation rules.</summary>
    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.BoardId)
            .NotEmpty()
            .WithMessage("Board id is required.");

        RuleFor(x => x.Request.Title)
            .NotEmpty()
            .WithMessage("Task title is required.")
            .MaximumLength(200)
            .WithMessage("Task title cannot exceed 200 characters.");

        RuleFor(x => x.Request.PriorityRefTermKey)
            .NotEmpty()
            .WithMessage("Priority is required.")
            .MaximumLength(100);

        RuleFor(x => x.Request.TaskTypeRefTermKey)
            .NotEmpty()
            .WithMessage("Task type is required.")
            .MaximumLength(100);
    }
}

/// <summary>Handles the <see cref="CreateTaskCommand"/> by creating a task through the task service.</summary>
public class CreateTaskCommandHandler
    : IRequestHandler<CreateTaskCommand, TaskSummaryDto>
{
    private readonly ITaskService _taskService;
    private readonly ILoggerManager<CreateTaskCommandHandler> _logger;

    /// <summary>Initializes a new instance of the <see cref="CreateTaskCommandHandler"/> class.</summary>
    /// <param name="taskService">Service used to create the task.</param>
    /// <param name="logger">Logger used to record task creation activity.</param>
    public CreateTaskCommandHandler(
        ITaskService taskService,
        ILoggerManager<CreateTaskCommandHandler> logger)
    {
        _taskService = taskService;
        _logger = logger;
    }

    /// <summary>Handles the command and returns the newly created task.</summary>
    /// <param name="request">The command containing the board identifier and task details.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>The summary of the newly created task.</returns>
    public async Task<TaskSummaryDto> Handle(
        CreateTaskCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Creating task in board {BoardId}.",
            request.BoardId);

        return await _taskService.CreateTask(
            request.BoardId,
            request.Request,
            cancellationToken);
    }
}