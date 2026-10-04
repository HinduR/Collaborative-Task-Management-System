using MediatR;
using Shared.Logging.Contracts;
using Shared.Exceptions.Infrastructure;
using FluentValidation;
using Shared.Common.Dto;
using MetadataService.Application.Contract.IService;


namespace MetadataService.Application.Query;

/// <summary>
/// Query to get a RefTerm by the RefTermId.
/// </summary>
/// <param name="RefTermId">The Id of the RefTerm.</param>
/// <returns>RefTermDto object contains RefTerm.</returns>
public record GetRefTermByIdQuery(Guid RefTermId) : IRequest<RefTermDto>;


/// <summary>
/// Validator for the GetRefTermByIdQuery.
/// </summary>
public class GetRefTermByIdQueryValidator : AbstractValidator<GetRefTermByIdQuery>
{

    public GetRefTermByIdQueryValidator()
    {
        RuleFor(x => x.RefTermId)
            .NotEmpty().WithMessage("Ref term id is cannot be empty.");
    }
}

/// <summary>
/// Handles the GetRefTermByIdQuery and returns the corresponding List of RefTermDto"/>.
/// </summary>
public class GetRefTermByIdQueryHandler : IRequestHandler<GetRefTermByIdQuery, RefTermDto>
{
    private readonly IMetaDataService _refTermService;
    private readonly ILoggerManager<GetRefTermByIdQueryHandler> _logger;



    /// <summary>
    /// Initializes a new instance of the GetRefTermByIdQueryHandler class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="refTermService">The RefSetService instance</param>
    public GetRefTermByIdQueryHandler(ILoggerManager<GetRefTermByIdQueryHandler> logger, IMetaDataService refTermService)
    {
        _refTermService = refTermService;
        _logger = logger;
    }

    /// <summary>
    /// Handles the query to retrieve RefTerms by the RefSetKey.
    /// </summary>
    /// <param name="request">The query request containing the RefTermId.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A List of RefTermDto.</returns>
    /// <exception cref="NotFoundCustomException">Thrown when no RefTerm is found for the RefTermId.</exception>
    public async Task<RefTermDto> Handle(GetRefTermByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching refterm for the given ref-term-id: {RefTermId}", request.RefTermId);

        RefTermDto? refTermDto = await _refTermService.GetRefTermByIdAsync(request.RefTermId!, cancellationToken);
        
         _logger.LogInformation("Fetched refterm for the given ref-term-id: {RefTermId}", request.RefTermId);

        return refTermDto;
    }
}

