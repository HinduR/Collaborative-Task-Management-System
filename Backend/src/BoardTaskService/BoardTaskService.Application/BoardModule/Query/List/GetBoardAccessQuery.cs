using BoardTaskService.Application.BoardModule.Contract.IService;
using BoardTaskService.Application.BoardModule.Dto;
using FluentValidation;
using MediatR;
using Shared.Logging.Contracts;

namespace BoardTaskService.Application.BoardModule.Query.List;

/// <summary>Represents a query to retrieve the users who currently have access to a board.</summary>
/// <param name="BoardId">The unique identifier of the board whose access list should be retrieved.</param>
public record GetBoardAccessQuery(Guid BoardId)
    : IRequest<List<BoardAccessUserDto>>;

/// <summary>Validates the board identifier supplied in a <see cref="GetBoardAccessQuery"/>.</summary>
public class GetBoardAccessQueryValidator
    : AbstractValidator<GetBoardAccessQuery>
{
    /// <summary>Initializes a new instance of the <see cref="GetBoardAccessQueryValidator"/> class and configures the validation rules.</summary>
    public GetBoardAccessQueryValidator()
    {
        RuleFor(x => x.BoardId)
            .NotEmpty()
            .WithMessage("BoardId cannot be empty.");
    }
}

/// <summary>Handles the <see cref="GetBoardAccessQuery"/> by retrieving the users who currently have access to the specified board.</summary>
public class GetBoardAccessQueryHandler
    : IRequestHandler<GetBoardAccessQuery, List<BoardAccessUserDto>>
{
    private readonly IBoardAccessService _boardAccessService;
    private readonly ILoggerManager<GetBoardAccessQueryHandler> _logger;

    /// <summary>Initializes a new instance of the <see cref="GetBoardAccessQueryHandler"/> class.</summary>
    /// <param name="boardAccessService">Service used to retrieve board access information.</param>
    /// <param name="logger">Logger used to record board access retrieval activity.</param>
    public GetBoardAccessQueryHandler(
        IBoardAccessService boardAccessService,
        ILoggerManager<GetBoardAccessQueryHandler> logger)
    {
        _boardAccessService = boardAccessService;
        _logger = logger;
    }

    /// <summary>Handles the query and returns the users who currently have access to the specified board.</summary>
    /// <param name="request">The query containing the board identifier.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>A list containing users with active access to the board.</returns>
    public async Task<List<BoardAccessUserDto>> Handle(
        GetBoardAccessQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Fetching users with access to board {BoardId}.",
            request.BoardId);

        List<BoardAccessUserDto> result = await _boardAccessService
            .GetBoardAccess(request.BoardId, cancellationToken);

        _logger.LogInformation(
            "Fetched {Count} users with access to board {BoardId}.",
            result.Count,
            request.BoardId);

        return result;
    }
}
