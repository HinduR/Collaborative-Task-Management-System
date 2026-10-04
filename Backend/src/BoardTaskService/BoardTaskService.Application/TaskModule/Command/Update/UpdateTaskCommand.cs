using BoardTaskService.Application.TaskModule.Contract.IService;
using BoardTaskService.Application.TaskModule.Dto;
using FluentValidation;
using MediatR;
using Shared.Logging.Contracts;

namespace BoardTaskService.Application.TaskModule.Command.Update;

/// <summary>Represents a command to update an existing task.</summary>
/// <param name="BoardId">The unique identifier of the board containing the task.</param>
/// <param name="TaskId">The unique identifier of the task to update.</param>
/// <param name="Request">The updated task data.</param>
public record UpdateTaskCommand(
    Guid BoardId,
    Guid TaskId,
    TaskRequestDto Request) : IRequest<TaskDetailsDto>;

/// <summary>Validates the data supplied in an <see cref="UpdateTaskCommand"/>.</summary>
public class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
{
    /// <summary>Initializes a new instance of the <see cref="UpdateTaskCommandValidator"/> class and configures the validation rules.</summary>
    public UpdateTaskCommandValidator()
    {
        RuleFor(x => x.BoardId).NotEmpty();
        RuleFor(x => x.TaskId).NotEmpty();

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

        RuleFor(x => x.Request.WorkflowColumnId).NotEmpty();
    }
}

/// <summary>Handles the <see cref="UpdateTaskCommand"/> by updating the specified task through the task service.</summary>
public class UpdateTaskCommandHandler
    : IRequestHandler<UpdateTaskCommand, TaskDetailsDto>
{
    private readonly ITaskService _taskService;
    private readonly ILoggerManager<UpdateTaskCommandHandler> _logger;

    /// <summary>Initializes a new instance of the <see cref="UpdateTaskCommandHandler"/> class.</summary>
    /// <param name="taskService">Service used to update the task.</param>
    /// <param name="logger">Logger used to record task update activity.</param>
    public UpdateTaskCommandHandler(
        ITaskService taskService,
        ILoggerManager<UpdateTaskCommandHandler> logger)
    {
        _taskService = taskService;
        _logger = logger;
    }

    /// <summary>Handles the command and updates the specified task.</summary>
    /// <param name="request">The command containing the task identifier and updated task data.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>The updated task details.</returns>
    public async Task<TaskDetailsDto> Handle(
        UpdateTaskCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Updating task {TaskId} in board {BoardId}.",
            request.TaskId,
            request.BoardId);

        return await _taskService.UpdateTask(
            request.BoardId,
            request.TaskId,
            request.Request,
            cancellationToken);
    }
}