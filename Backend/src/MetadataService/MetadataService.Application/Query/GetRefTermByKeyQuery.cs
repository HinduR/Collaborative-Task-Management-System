using MediatR;
using Shared.Logging.Contracts;
using Shared.Exceptions.Infrastructure;
using FluentValidation;
using Shared.Common.Dto;
using MetadataService.Application.Contract.IService;



namespace MetadataService.Application.Query;


/// <summary>
/// Query to get a RefTerm by the RefTermKey.
/// </summary>
/// <param name="RefTermKey">The Id of the RefTerm.</param>
/// <returns>RefTermDto object contains RefTerm.</returns>
public record GetRefTermByKeyQuery(string RefTermKey) : IRequest<RefTermDto>;


/// <summary>
/// Validator for the GetRefTermIdByKeyQuery.
/// </summary>
public class GetRefTermByKeyQueryValidator : AbstractValidator<GetRefTermByKeyQuery>
{

    public GetRefTermByKeyQueryValidator()
    {
        RuleFor(x => x.RefTermKey)
            .NotEmpty().WithMessage("Ref term key is cannot be empty.");
    }
}

/// <summary>
/// Handles the GetRefTermIdByKeyQuery and returns the corresponding Id"/>.
/// </summary>
public class GetRefTermByKeyQueryHandler : IRequestHandler<GetRefTermByKeyQuery, RefTermDto>
{
    private readonly IMetaDataService _refTermService;
    private readonly ILoggerManager<GetRefTermByKeyQueryHandler> _logger;


    /// <summary>
    /// Initializes a new instance of the GetRefTermIdByKeyQueryHandler class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="refTermService">The RefSetService instance</param>
    public GetRefTermByKeyQueryHandler(ILoggerManager<GetRefTermByKeyQueryHandler> logger, IMetaDataService refTermService)
    {
        _refTermService = refTermService;
        _logger = logger;

    }

    /// <summary>
    /// Handles the query to retrieve RefTermDto by the RefTermKey.
    /// </summary>
    /// <param name="request">The query request containing the RefTermKey.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>Id of the RefTermKey.</returns>
    /// <exception cref="NotFoundCustomException">Thrown when no RefTerm is found for the RefTermKey.</exception>
    public async Task<RefTermDto> Handle(GetRefTermByKeyQuery request, CancellationToken cancellationToken)
    {

        _logger.LogInformation("Fetching refterm id for the given ref-term_key: {RefTermKey}", request.RefTermKey);

        RefTermDto refTerm = await _refTermService.GetRefTermByKeyAsync(request.RefTermKey!, cancellationToken);

        _logger.LogInformation("Fetched refterm for the given ref-term_key: {RefTermKey}", request.RefTermKey);
        return refTerm;

    }

}

