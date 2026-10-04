using BoardTaskService.Application.TaskModule.Contract.IService;
using BoardTaskService.Application.TaskModule.Dto;
using FluentValidation;
using MediatR;

namespace BoardTaskService.Application.TaskModule.Query.Get;

/// <summary>
/// Represents the GetBoardTasksQuery component.
/// </summary>
public record GetBoardTasksQuery(Guid BoardId)
    : IRequest<List<TaskSummaryDto>>;

/// <summary>
/// Represents the GetBoardTasksQueryValidator component.
/// </summary>
public class GetBoardTasksQueryValidator
    : AbstractValidator<GetBoardTasksQuery>
{
    public GetBoardTasksQueryValidator()
    {

        RuleFor(query => query.BoardId)
            .NotEmpty()
            .WithMessage("BoardId cannot be empty.");
    }
}
/// <summary>
/// Represents the GetBoardTasksQueryHandler component.
/// </summary>
public class GetBoardTasksQueryHandler
    : IRequestHandler<
        GetBoardTasksQuery,
        List<TaskSummaryDto>>
{
    private readonly IBoardTaskListService _taskListService;

    public GetBoardTasksQueryHandler(
        IBoardTaskListService taskListService)
    {
        _taskListService = taskListService;
    }

    public Task<List<TaskSummaryDto>> Handle(
        GetBoardTasksQuery request,
        CancellationToken cancellationToken)
    {
        return _taskListService.GetBoardTasksAsync(
            request.BoardId,
            cancellationToken);
    }
}
