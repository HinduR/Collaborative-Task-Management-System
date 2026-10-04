using BoardTaskService.Application.TaskModule.Contract.IService;
using BoardTaskService.Application.TaskModule.Dto;
using FluentValidation;
using MediatR;
using Shared.Logging.Contracts;

namespace BoardTaskService.Application.TaskModule.Query.Get;

/// <summary>
/// Represents the GetTaskByIdQuery component.
/// </summary>
public record GetTaskByIdQuery(
    Guid BoardId,
    Guid TaskId) : IRequest<TaskDetailsDto>;

/// <summary>
/// Represents the GetTaskByIdQueryValidator component.
/// </summary>
public class GetTaskByIdQueryValidator : AbstractValidator<GetTaskByIdQuery>
{
    public GetTaskByIdQueryValidator()
    {
        RuleFor(x => x.BoardId).NotEmpty();
        RuleFor(x => x.TaskId).NotEmpty();
    }
}

/// <summary>
/// Represents the GetTaskByIdQueryHandler component.
/// </summary>
public class GetTaskByIdQueryHandler
    : IRequestHandler<GetTaskByIdQuery, TaskDetailsDto>
{
    private readonly ITaskService _taskService;
    private readonly ILoggerManager<GetTaskByIdQueryHandler> _logger;

    public GetTaskByIdQueryHandler(
        ITaskService taskService,
        ILoggerManager<GetTaskByIdQueryHandler> logger)
    {
        _taskService = taskService;
        _logger = logger;
    }

    public async Task<TaskDetailsDto> Handle(
        GetTaskByIdQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Getting task {TaskId} from board {BoardId}.",
            request.TaskId,
            request.BoardId);

        return await _taskService.GetTaskById(
            request.BoardId,
            request.TaskId,
            cancellationToken);
    }
}
