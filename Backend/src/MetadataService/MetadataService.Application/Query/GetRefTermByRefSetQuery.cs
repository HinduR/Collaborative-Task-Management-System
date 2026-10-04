using MediatR;
using Shared.Logging.Contracts;
using Shared.Exceptions.Infrastructure;
using FluentValidation;
using Shared.Common.Dto;
using MetadataService.Application.Contract.IService;

namespace MetadataService.Application.Query;

/// <summary>
/// Query to get a RefTerms by the RefSetKey.
/// </summary>
/// <param name="RefSetKey">The key of the RefSet.</param>
/// <returns>A List of RefTermDto objects containing RefTerms.</returns>
public record GetRefTermByRefSetQuery(string RefSetKey) : IRequest<List<RefTermDto>>;


/// <summary>
/// Validator for the GetRefTermByRefSetQuery.
/// </summary>
public class GetRefTermByRefSetQueryValidator : AbstractValidator<GetRefTermByRefSetQuery>
{

    public GetRefTermByRefSetQueryValidator()
    {
        RuleFor(x => x.RefSetKey)
            .NotEmpty().WithMessage("RefSetKey is cannot be empty.");
    }
}

/// <summary>
/// Handles the GetRefTermByRefSetQuery and returns the corresponding List of RefTermDto"/>.
/// </summary>
public class GetRefTermByRefSetQueryHandler : IRequestHandler<GetRefTermByRefSetQuery, List<RefTermDto>>
{
    private readonly IMetaDataService _refSetService;
    private readonly ILoggerManager<GetRefTermByRefSetQueryHandler> _logger;
  

    /// <summary>
    /// Initializes a new instance of the GetRefTermByRefSetQueryHandler class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="refSetService">The RefSetService instance</param>
    public GetRefTermByRefSetQueryHandler(ILoggerManager<GetRefTermByRefSetQueryHandler> logger, IMetaDataService refSetService)
    {
        _refSetService = refSetService;
        _logger = logger;
      
    }

    /// <summary>
    /// Handles the query to retrieve RefTerms by the RefSetKey.
    /// </summary>
    /// <param name="request">The query request containing the RefSetKey.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A List of RefTermDto.</returns>
    /// <exception cref="NotFoundCustomException">Thrown when no RefSet is found for the RefSetKey.</exception>
    /// <exception cref="NoContentCustomException">Thrown when no RefTerms are found for the provided RefSetKey.</exception>
    public async Task<List<RefTermDto>> Handle(GetRefTermByRefSetQuery request, CancellationToken cancellationToken)
    {

        _logger.LogInformation("Fetching ref terms for the given ref-set-key: {RefSetKey}", request.RefSetKey);

        List<RefTermDto> result = await _refSetService.GetRefTermsByRefSetKeyAsync(request.RefSetKey!,cancellationToken);

        _logger.LogInformation("Fetched refterms for the given ref-set-key: {RefSetKey} successfully.", request.RefSetKey);

        return result;
    }

}

