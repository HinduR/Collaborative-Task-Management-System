using BoardTaskService.Application.TaskModule.Contract.IHelperService;
using BoardTaskService.Application.TaskModule.Contract.IService;
using BoardTaskService.Application.TaskModule.Dto;
using FluentValidation;
using MediatR;
using Shared.Logging.Contracts;

namespace BoardTaskService.Application.TaskModule.Command.Comment.Create;

/// <summary>Represents a command to create a comment for a task.</summary>
/// <param name="BoardId">The unique identifier of the board containing the task.</param>
/// <param name="TaskId">The unique identifier of the task where the comment will be added.</param>
/// <param name="Request">The comment creation details.</param>
public record CreateTaskCommentCommand(
    Guid BoardId,
    Guid TaskId,
    CommentRequestDto Request) : IRequest<TaskCommentDto>;

/// <summary>Validates the board identifier, task identifier, and comment details supplied in a <see cref="CreateTaskCommentCommand"/>.</summary>
public class CreateTaskCommentCommandValidator
    : AbstractValidator<CreateTaskCommentCommand>
{
    /// <summary>Initializes a new instance of the <see cref="CreateTaskCommentCommandValidator"/> class and configures the validation rules.</summary>
    public CreateTaskCommentCommandValidator()
    {
        RuleFor(x => x.BoardId)
            .NotEmpty()
            .WithMessage("Board id is required.");

        RuleFor(x => x.TaskId)
            .NotEmpty()
            .WithMessage("Task id is required.");

        RuleFor(x => x.Request)
            .NotNull()
            .WithMessage("Comment request is required.");

        When(x => x.Request is not null, () =>
        {
            RuleFor(x => x.Request.Comment)
                .NotEmpty()
                .WithMessage("Comment is required.")
                .MaximumLength(1000)
                .WithMessage("Comment cannot exceed 1000 characters.");
        });
    }
}

/// <summary>Handles the <see cref="CreateTaskCommentCommand"/> by creating a comment through the task comment helper service.</summary>
public class CreateTaskCommentCommandHandler
    : IRequestHandler<CreateTaskCommentCommand, TaskCommentDto>
{
    private readonly ITaskCommentHelperService _commentHelperService;
    private readonly ILoggerManager<CreateTaskCommentCommandHandler> _logger;

    /// <summary>Initializes a new instance of the <see cref="CreateTaskCommentCommandHandler"/> class.</summary>
    /// <param name="commentHelperService">Helper service used to validate and create the task comment.</param>
    /// <param name="logger">Logger used to record task comment creation activity.</param>
    public CreateTaskCommentCommandHandler(
        ITaskCommentHelperService commentHelperService,
        ILoggerManager<CreateTaskCommentCommandHandler> logger)
    {
        _commentHelperService = commentHelperService;
        _logger = logger;
    }

    /// <summary>Handles the command and returns the newly created task comment.</summary>
    /// <param name="request">The command containing the board, task, and comment details.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>The newly created task comment.</returns>
    public async Task<TaskCommentDto> Handle(
        CreateTaskCommentCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Creating comment for task {TaskId} in board {BoardId}.",
            request.TaskId,
            request.BoardId);

        return await _commentHelperService.CreateComment(
            request.BoardId,
            request.TaskId,
            request.Request,
            cancellationToken);
    }
}