using BoardTaskService.Application.TaskModule.Contract.IHelperService;
using BoardTaskService.Application.TaskModule.Contract.IService;
using BoardTaskService.Application.TaskModule.Dto;
using FluentValidation;
using MediatR;
using Shared.Logging.Contracts;

namespace BoardTaskService.Application.TaskModule.Command.Comment.Update;

/// <summary>Represents a command to update an existing task comment.</summary>
/// <param name="BoardId">The unique identifier of the board containing the task.</param>
/// <param name="TaskId">The unique identifier of the task containing the comment.</param>
/// <param name="CommentId">The unique identifier of the comment to update.</param>
/// <param name="Request">The updated comment data.</param>
public record UpdateTaskCommentCommand(
    Guid BoardId,
    Guid TaskId,
    Guid CommentId,
    CommentRequestDto Request) : IRequest<TaskCommentDto>;

/// <summary>Validates the data supplied in an <see cref="UpdateTaskCommentCommand"/>.</summary>
public class UpdateTaskCommentCommandValidator
    : AbstractValidator<UpdateTaskCommentCommand>
{
    /// <summary>Initializes a new instance of the <see cref="UpdateTaskCommentCommandValidator"/> class and configures the validation rules.</summary>
    public UpdateTaskCommentCommandValidator()
    {
        RuleFor(x => x.BoardId)
            .NotEmpty()
            .WithMessage("Board id is required.");

        RuleFor(x => x.TaskId)
            .NotEmpty()
            .WithMessage("Task id is required.");

        RuleFor(x => x.CommentId)
            .NotEmpty()
            .WithMessage("Comment id is required.");

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

/// <summary>Handles the <see cref="UpdateTaskCommentCommand"/> by updating the specified comment through the comment helper service.</summary>
public class UpdateTaskCommentCommandHandler
    : IRequestHandler<UpdateTaskCommentCommand, TaskCommentDto>
{
    private readonly ITaskCommentHelperService _commentHelperService;
    private readonly ILoggerManager<UpdateTaskCommentCommandHandler> _logger;

    /// <summary>Initializes a new instance of the <see cref="UpdateTaskCommentCommandHandler"/> class.</summary>
    /// <param name="commentHelperService">Helper service used to validate and update task comments.</param>
    /// <param name="logger">Logger used to record comment update activity.</param>
    public UpdateTaskCommentCommandHandler(
        ITaskCommentHelperService commentHelperService,
        ILoggerManager<UpdateTaskCommentCommandHandler> logger)
    {
        _commentHelperService = commentHelperService;
        _logger = logger;
    }

    /// <summary>Handles the command and updates the specified task comment.</summary>
    /// <param name="request">The command containing the comment identifier and updated comment data.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>The updated task comment.</returns>
    public async Task<TaskCommentDto> Handle(
        UpdateTaskCommentCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Updating comment {CommentId} for task {TaskId}.",
            request.CommentId,
            request.TaskId);

        return await _commentHelperService.UpdateComment(
            request.BoardId,
            request.TaskId,
            request.CommentId,
            request.Request,
            cancellationToken);
    }
}