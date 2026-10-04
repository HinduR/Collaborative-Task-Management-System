using BoardTaskService.Application.BoardModule.Contract.IService;
using BoardTaskService.Application.BoardModule.Dto;
using FluentValidation;
using MediatR;
using Shared.Logging.Contracts;

namespace BoardTaskService.Application.BoardModule.Query.List;

/// <summary>Represents a query to retrieve the boards accessible to the current user within a project.</summary>
/// <param name="ProjectId">The unique identifier of the project whose boards should be retrieved.</param>
public record GetProjectBoardsQuery(Guid ProjectId)
    : IRequest<List<BoardSummaryDto>>;

/// <summary>Validates the project identifier supplied in a <see cref="GetProjectBoardsQuery"/>.</summary>
public class GetProjectBoardsQueryValidator
    : AbstractValidator<GetProjectBoardsQuery>
{
    /// <summary>Initializes a new instance of the <see cref="GetProjectBoardsQueryValidator"/> class and configures the validation rules.</summary>
    public GetProjectBoardsQueryValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage("ProjectId cannot be empty.");
    }
}

/// <summary>Handles the <see cref="GetProjectBoardsQuery"/> by retrieving the boards accessible to the current user.</summary>
public class GetProjectBoardsQueryHandler
    : IRequestHandler<GetProjectBoardsQuery, List<BoardSummaryDto>>
{
    private readonly IBoardService _boardService;
    private readonly ILoggerManager<GetProjectBoardsQueryHandler> _logger;

    /// <summary>Initializes a new instance of the <see cref="GetProjectBoardsQueryHandler"/> class.</summary>
    /// <param name="boardService">Service used to retrieve accessible boards.</param>
    /// <param name="logger">Logger used to record board retrieval activity.</param>
    public GetProjectBoardsQueryHandler(
        IBoardService boardService,
        ILoggerManager<GetProjectBoardsQueryHandler> logger)
    {
        _boardService = boardService;
        _logger = logger;
    }

    /// <summary>Handles the query and returns the boards accessible to the current user within the specified project.</summary>
    /// <param name="request">The query containing the project identifier.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>A list containing the accessible board summaries.</returns>
    public async Task<List<BoardSummaryDto>> Handle(
        GetProjectBoardsQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Fetching accessible boards for project {ProjectId}.",
            request.ProjectId);

        List<BoardSummaryDto> result = await _boardService
            .GetProjectBoards(request.ProjectId, cancellationToken);

        _logger.LogInformation(
            "Fetched {Count} accessible boards for project {ProjectId}.",
            result.Count,
            request.ProjectId);

        return result;
    }
}
