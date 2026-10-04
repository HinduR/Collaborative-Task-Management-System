using System.Linq.Expressions;
using BoardTaskService.Application.TaskModule.Contract.IService;
using BoardTaskService.Application.TaskModule.Dto;
using BoardTaskService.Application.TaskModule.HelperService;
using BoardTaskService.Domain.Models;
using FluentValidation;
using MediatR;
using Shared.Common.contracts;
using Shared.Logging.Contracts;

namespace BoardTaskService.Application.TaskModule.Query.TaskQuery;

/// <summary>
/// Represents the GetTaskQuery component.
/// </summary>
public record GetTaskQuery(
    TaskQueryRequestDto Request)
    : IRequest<TaskQueryResponseDto>;

/// <summary>
/// Represents the GetTaskQueryValidator component.
/// </summary>
public class GetTaskQueryValidator
    : AbstractValidator<GetTaskQuery>
{
    public GetTaskQueryValidator()
    {
        RuleFor(x => x.Request)
            .NotNull()
            .WithMessage("Task query request is required.");

        When(x => x.Request is not null, () =>
        {
            RuleFor(x => x.Request.PageNumber)
                .GreaterThan(0)
                .WithMessage("Page number must be greater than zero.");

            RuleFor(x => x.Request.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage("Page size must be between 1 and 100.");
        });
    }
}

/// <summary>
/// Represents the GetTaskQueryHandler component.
/// </summary>
public class GetTaskQueryHandler
    : IRequestHandler<GetTaskQuery, TaskQueryResponseDto>
{
    private readonly ITaskQueryService _taskQueryService;
    private readonly IUserContext _userContext;
    private readonly ILoggerManager<GetTaskQueryHandler> _logger;

    public GetTaskQueryHandler(
        ITaskQueryService taskQueryService,
        IUserContext userContext,
        ILoggerManager<GetTaskQueryHandler> logger)
    {
        _taskQueryService = taskQueryService;
        _userContext = userContext;
        _logger = logger;
    }

    public async Task<TaskQueryResponseDto> Handle(
        GetTaskQuery request,
        CancellationToken cancellationToken)
    {
        Guid currentUserId = _userContext.GetUserId();

        Expression<Func<BoardTask, bool>> conditionExpression =
            TaskQueryExpressionBuilder.Build(
                request.Request.Conditions);

        _logger.LogInformation(
            "Running task query with {ConditionCount} conditions. " +
            "PageNumber: {PageNumber}, PageSize: {PageSize}, UserId: {UserId}.",
            request.Request.Conditions.Count,
            request.Request.PageNumber,
            request.Request.PageSize,
            currentUserId);

        return await _taskQueryService.QueryTasksAsync(
            currentUserId,
            conditionExpression,
            request.Request.PageNumber,
            request.Request.PageSize,
            cancellationToken);
    }
}
