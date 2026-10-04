using BoardTaskService.Application.ProjectModule.Contract.IService;
using FluentValidation;
using MediatR;
using Shared.Logging.Contracts;

namespace BoardTaskService.Application.ProjectModule.Command.Update;

/// <summary>
/// Represents a command to replace a user's project access mappings.
/// </summary>
public record UpdateUserProjectMappingCommand(
    Guid UserId,
    List<Guid> ProjectIds)
    : IRequest;

/// <summary>
/// Validates the replace user project mappings command.
/// </summary>
public class UpdateUserProjectMappingCommandValidator
    : AbstractValidator<UpdateUserProjectMappingCommand>
{
    public UpdateUserProjectMappingCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId cannot be empty.");

        RuleFor(x => x.ProjectIds)
            .NotNull()
            .WithMessage("ProjectIds cannot be null.");

        RuleForEach(x => x.ProjectIds)
            .NotEmpty()
            .WithMessage("ProjectId cannot be empty.");

        RuleFor(x => x.ProjectIds)
            .Must(projectIds => projectIds is not null && projectIds.Distinct().Count() == projectIds.Count)
            .WithMessage("Duplicate project IDs are not allowed.");
    }
}

/// <summary>
/// Handles replacement of a user's project access mappings.
/// </summary>
public class UpdateUserProjectMappingCommandHandler
    : IRequestHandler<UpdateUserProjectMappingCommand>
{
    private readonly IProjectService _projectService;
    private readonly ILoggerManager<UpdateUserProjectMappingCommandHandler> _logger;

    public UpdateUserProjectMappingCommandHandler(
        IProjectService projectService,
        ILoggerManager<UpdateUserProjectMappingCommandHandler> logger)
    {
        _projectService = projectService;
        _logger = logger;
    }

    public async Task Handle(
        UpdateUserProjectMappingCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Replacing project mappings for user {UserId}.",
            request.UserId);

        await _projectService.ReplaceUserProjectMappings(
            request.UserId,
            request.ProjectIds,
            cancellationToken);

        _logger.LogInformation(
            "Successfully replaced project mappings for user {UserId}.",
            request.UserId);
    }
}
