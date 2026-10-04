using MediatR;
using MetadataService.Application.Query;
using Microsoft.AspNetCore.Mvc;
using Shared.Common.Dto;
using Shared.Logging.Contracts;
using Swashbuckle.AspNetCore.Annotations;

namespace MetadataService.API.Controller;
/// <summary>
/// This class handles requests related to metadata.
/// </summary>
[ApiController]
[Route("meta-data/")]
/// <summary>
/// Represents the MetaDataController component.
/// </summary>
public class MetaDataController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILoggerManager<MetaDataController> _logger;

    /// <summary>
    /// This is constructor of MetaData controller class
    /// </summary>
    /// <param name="mediator">IMediator object</param>
    /// <param name="logger">Logger</param>
    public MetaDataController(IMediator mediator, ILoggerManager<MetaDataController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a list of RefTerms associated with a given RefSetKey.
    /// </summary>
    /// <param name="key">The key of the RefSet.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A list of RefTermDto objects if found, otherwise NoContent.</returns>
    [SwaggerResponse(
            statusCode: StatusCodes.Status200OK,
            type: typeof(List<RefTermDto>),
            description: "Retrieved list of refterm"
        )]
    [SwaggerResponse(
            statusCode: StatusCodes.Status204NoContent,
            description: "No content"
        )]
    [SwaggerResponse(
            statusCode: StatusCodes.Status401Unauthorized,
            description: "Unauthorized"
        )]
    [SwaggerResponse(
            statusCode: StatusCodes.Status403Forbidden,
            description: "Forbidden"
        )]
    [SwaggerResponse(
            statusCode: StatusCodes.Status500InternalServerError,
            description: "Internal server error"
        )]
    [HttpGet("ref-set/{key}/ref-terms")]
    public async Task<IActionResult> GetRefTermsByRefSetKey(string key, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching the RefTerms for RefSetKey: {key}", key);
        GetRefTermByRefSetQuery query = new GetRefTermByRefSetQuery(key);
        List<RefTermDto> refTermList = await _mediator.Send(query, cancellationToken);
        _logger.LogDebug("Fetched the RefTerms for RefSetKey: {key} successfully.", key);
        return refTermList.Count > 0 ? Ok(refTermList) : NoContent();
    }

    /// <summary>
    /// Retrieves a RefTerm by its key.
    /// </summary>
    /// <param name="key">The key of the RefTerm.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The RefTermDto object if found.</returns>
    [SwaggerResponse(
            statusCode: StatusCodes.Status200OK,
            type: typeof(RefTermDto),
            description: "Retrieved refterm"
        )]
    [SwaggerResponse(
            statusCode: StatusCodes.Status401Unauthorized,
            description: "Unauthorized"
        )]
    [SwaggerResponse(
            statusCode: StatusCodes.Status403Forbidden,
            description: "Forbidden"
        )]
    [SwaggerResponse(
            statusCode: StatusCodes.Status500InternalServerError,
            description: "Internal server error"
        )]
    [HttpGet("ref-term-by-key/{key}")]
    public async Task<IActionResult> GetRefTermByRefTermKey(string key, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching the RefTermId for RefTermKey: {key}", key);
        GetRefTermByKeyQuery query = new GetRefTermByKeyQuery(key);
        RefTermDto result = await _mediator.Send(query, cancellationToken);
        _logger.LogDebug("Fetched the RefTermId for RefTermKey: {key} successfully.", key);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a RefTerm by its ID.
    /// </summary>
    /// <param name="id">The ID of the RefTerm.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The RefTermDto object if found.</returns>
    [SwaggerResponse(
            statusCode: StatusCodes.Status200OK,
            type: typeof(RefTermDto),
            description: "Retrieved refterm"
        )]
    [SwaggerResponse(
            statusCode: StatusCodes.Status401Unauthorized,
            description: "Unauthorized"
        )]
    [SwaggerResponse(
            statusCode: StatusCodes.Status403Forbidden,
            description: "Forbidden"
        )]
    [SwaggerResponse(
            statusCode: StatusCodes.Status500InternalServerError,
            description: "Internal server error"
        )]
    [HttpGet("ref-term/{id}")]
    public async Task<IActionResult> GetRefTermByRefTermId(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching the RefTerm for RefSetId: {id}", id);
        GetRefTermByIdQuery query = new GetRefTermByIdQuery(id);
        RefTermDto result = await _mediator.Send(query, cancellationToken);
        _logger.LogDebug("Fetched the RefTerm for RefSetId: {id} successfully.", id);
        return Ok(result);
    }
}
