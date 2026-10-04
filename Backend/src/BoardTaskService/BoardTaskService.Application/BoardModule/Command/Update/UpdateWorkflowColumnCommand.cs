using BoardTaskService.Application.BoardModule.Contract.IService;
using BoardTaskService.Application.BoardModule.Dto;
using FluentValidation;
using MediatR;
using Shared.Logging.Contracts;

namespace BoardTaskService.Application.BoardModule.Command.Update;

///<summary>
/// Command to replace the full set of workflow columns for a board.
/// </summary>
/// <param name="BoardId">The unique identifier of the board whose workflow columns are being updated.</param>
/// <param name="Request">The complete desired state of the board's workflow columns.</param>
public record UpdateWorkflowColumnCommand(
    Guid BoardId,
    UpdateWorkflowColumnsRequest Request)
    : IRequest<List<WorkflowColumnDto>>;

/// <summary>
/// Validates the shape of an <see cref="UpdateWorkflowColumnCommand"/> before it reaches the handler.
/// Referential checks (board existence, ownership, column-to-board membership) are performed
/// separately in <see cref="IWorkflowColumnService"/>, since they require database access.
/// </summary>
public class UpdateWorkflowColumnCommandValidator
    : AbstractValidator<UpdateWorkflowColumnCommand>
{
    public UpdateWorkflowColumnCommandValidator()
    {
        RuleFor(x => x.BoardId)
            .NotEmpty()
            .WithMessage("BoardId cannot be empty.");

        RuleFor(x => x.Request.WorkflowColumnList)
            .NotNull()
            .WithMessage("Workflow columns cannot be null.")
            .NotEmpty()
            .WithMessage("At least one workflow column is required.")
            .Must(columns => columns.Count <= 5)
            .WithMessage("A board can have a maximum of five workflow columns.");

        RuleForEach(x => x.Request.WorkflowColumnList)
            .ChildRules(column =>
            {
                column.RuleFor(x => x.ColumnName)
                    .NotEmpty()
                    .WithMessage("Workflow column name cannot be empty.")
                    .MaximumLength(100)
                    .WithMessage(
                        "Workflow column name cannot exceed 100 characters.");
            });

        RuleFor(x => x.Request.WorkflowColumnList)
            .Must(columns => columns is not null && columns
                .Select(column => column.ColumnName.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count() == columns.Count)
            .WithMessage("Duplicate workflow column names are not allowed.");

        RuleFor(x => x.Request.WorkflowColumnList)
            .Must(columns => columns is not null && columns
                .Where(column => column.Id.HasValue)
                .Select(column => column.Id!.Value)
                .Distinct()
                .Count() == columns.Count(column => column.Id.HasValue))
            .WithMessage("Duplicate workflow column IDs are not allowed.");
    }
}

/// <summary>
/// Represents the UpdateWorkflowColumnCommandHandler component.
/// </summary>
public class UpdateWorkflowColumnCommandHandler
    : IRequestHandler<UpdateWorkflowColumnCommand, List<WorkflowColumnDto>>
{
    private readonly IWorkflowColumnHelperService _workflowColumnHelperService;
    private readonly ILoggerManager<UpdateWorkflowColumnCommandHandler> _logger;

    public UpdateWorkflowColumnCommandHandler(
        IWorkflowColumnHelperService workflowColumnHelperService,
        ILoggerManager<UpdateWorkflowColumnCommandHandler> logger)
    {
        _workflowColumnHelperService = workflowColumnHelperService;
        _logger = logger;
    }

    /// <summary>
    /// Saves the workflow columns for the specified board, applying creates, updates,
    /// and soft-deletes based on the supplied column list.
    /// </summary>
    /// <param name="request">The command containing the board ID and desired workflow column state.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>The saved list of workflow columns.</returns>
    public async Task<List<WorkflowColumnDto>> Handle(
           UpdateWorkflowColumnCommand request,
            CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Saving workflow columns for board {BoardId}.",
            request.BoardId);

        List<WorkflowColumnDto> result = await _workflowColumnHelperService
     .SaveWorkflowColumnsAsync(
         request.BoardId,
         request.Request,
         cancellationToken);

        _logger.LogInformation(
            "Workflow columns saved successfully for board {BoardId}. {Count} columns returned.",
            request.BoardId,
            result.Count);

        return result;

    }
}
