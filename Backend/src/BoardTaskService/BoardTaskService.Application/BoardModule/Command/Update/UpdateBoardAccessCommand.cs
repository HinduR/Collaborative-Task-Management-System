using BoardTaskService.Application.BoardModule.Contract.IService;
using FluentValidation;
using MediatR;
using Shared.Logging.Contracts;

namespace BoardTaskService.Application.BoardModule.Command.Update;

/// <summary>Represents a command to replace the user access mappings for a board.</summary>
/// <param name="BoardId">The unique identifier of the board whose access mappings will be replaced.</param>
/// <param name="UserProjectMappingIds">The complete list of user-project mapping identifiers that should have access to the board.</param>
public record UpdateBoardAccessCommand(
    Guid BoardId,
    List<Guid> UserProjectMappingIds)
    : IRequest;

/// <summary>Validates the board identifier and user-project mapping identifiers supplied to an <see cref="UpdateBoardAccessCommand"/>.</summary>
public class UpdateBoardAccessCommandValidator
    : AbstractValidator<UpdateBoardAccessCommand>
{
    /// <summary>Initializes a new instance of the <see cref="UpdateBoardAccessCommandValidator"/> class and configures the validation rules.</summary>
    public UpdateBoardAccessCommandValidator()
    {
        RuleFor(x => x.BoardId)
            .NotEmpty()
            .WithMessage("BoardId cannot be empty.");

        RuleFor(x => x.UserProjectMappingIds)
            .NotNull()
            .WithMessage("UserProjectMappingIds cannot be null.");

        RuleForEach(x => x.UserProjectMappingIds)
            .NotEmpty()
            .WithMessage("UserProjectMappingId cannot be empty.");

        RuleFor(x => x.UserProjectMappingIds)
            .Must(ids => ids is not null && ids.Distinct().Count() == ids.Count)
            .WithMessage("Duplicate user-project mapping IDs are not allowed.");
    }
}

/// <summary>Handles the <see cref="UpdateBoardAccessCommand"/> by replacing the board's existing access mappings.</summary>
public class UpdateBoardAccessCommandHandler
    : IRequestHandler<UpdateBoardAccessCommand>
{
    private readonly IBoardAccessService _boardAccessService;
    private readonly ILoggerManager<UpdateBoardAccessCommandHandler> _logger;

    /// <summary>Initializes a new instance of the <see cref="UpdateBoardAccessCommandHandler"/> class.</summary>
    /// <param name="boardAccessService">Service used to replace the board access mappings.</param>
    /// <param name="logger">Logger used to record board access update activity.</param>
    public UpdateBoardAccessCommandHandler(
        IBoardAccessService boardAccessService,
        ILoggerManager<UpdateBoardAccessCommandHandler> logger)
    {
        _boardAccessService = boardAccessService;
        _logger = logger;
    }

    /// <summary>Handles the command and replaces the complete set of user access mappings for the specified board.</summary>
    /// <param name="request">The command containing the board identifier and requested user-project mapping identifiers.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    public async Task Handle(
        UpdateBoardAccessCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Replacing board access for board {BoardId}.",
            request.BoardId);

        await _boardAccessService.ReplaceBoardAccess(
            request.BoardId,
            request.UserProjectMappingIds,
            cancellationToken);

        _logger.LogInformation(
            "Board access replaced for board {BoardId}.",
            request.BoardId);
    }
}
