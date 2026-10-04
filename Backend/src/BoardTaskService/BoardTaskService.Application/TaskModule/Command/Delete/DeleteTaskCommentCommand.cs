using BoardTaskService.Application.TaskModule.Contract.IHelperService;
using BoardTaskService.Application.TaskModule.Contract.IService;
using FluentValidation;
using MediatR;
using Shared.Logging.Contracts;

namespace BoardTaskService.Application.TaskModule.Command.Comment.Delete;

/// <summary>Represents a command to delete a comment from a task.</summary>
/// <param name="BoardId">The unique identifier of the board containing the task.</param>
/// <param name="TaskId">The unique identifier of the task containing the comment.</param>
/// <param name="CommentId">The unique identifier of the comment to delete.</param>
public record DeleteTaskCommentCommand(
    Guid BoardId,
    Guid TaskId,
    Guid CommentId) : IRequest;

/// <summary>Validates the identifiers supplied in a <see cref="DeleteTaskCommentCommand"/>.</summary>
public class DeleteTaskCommentCommandValidator
    : AbstractValidator<DeleteTaskCommentCommand>
{
    /// <summary>Initializes a new instance of the <see cref="DeleteTaskCommentCommandValidator"/> class and configures the validation rules.</summary>
    public DeleteTaskCommentCommandValidator()
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
    }
}

/// <summary>Handles the <see cref="DeleteTaskCommentCommand"/> by deleting the specified comment through the comment helper service.</summary>
public class DeleteTaskCommentCommandHandler
    : IRequestHandler<DeleteTaskCommentCommand>
{
    private readonly ITaskCommentHelperService _commentHelperService;
    private readonly ILoggerManager<DeleteTaskCommentCommandHandler> _logger;

    /// <summary>Initializes a new instance of the <see cref="DeleteTaskCommentCommandHandler"/> class.</summary>
    /// <param name="commentHelperService">Helper service used to validate and delete task comments.</param>
    /// <param name="logger">Logger used to record comment deletion activity.</param>
    public DeleteTaskCommentCommandHandler(
        ITaskCommentHelperService commentHelperService,
        ILoggerManager<DeleteTaskCommentCommandHandler> logger)
    {
        _commentHelperService = commentHelperService;
        _logger = logger;
    }

    /// <summary>Handles the command and deletes the specified task comment.</summary>
    /// <param name="request">The command containing the board, task, and comment identifiers.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    public async Task Handle(
        DeleteTaskCommentCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Deleting comment {CommentId} from task {TaskId}.",
            request.CommentId,
            request.TaskId);

        await _commentHelperService.DeleteComment(
            request.BoardId,
            request.TaskId,
            request.CommentId,
            cancellationToken);
    }
}