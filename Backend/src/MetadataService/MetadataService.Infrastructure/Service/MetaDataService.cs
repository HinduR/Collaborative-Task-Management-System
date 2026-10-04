using MetadataService.Application.Common;
using MetadataService.Application.Contract.IService;
using MetadataService.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Shared.Common.Dto;
using Shared.Exceptions.Infrastructure;
using Shared.Logging.Contracts;
using Shared.Redis.Contract;

namespace MetadataService.Infrastructure.Service;

/// <summary>
/// Metadata business service.
/// </summary>
public class MetaDataService : IMetaDataService
{
    private readonly IRepoWrapper _repoWrapper;
    private readonly IRedisCacheService _redisCacheService;
    private readonly ILoggerManager<MetaDataService> _logger;

    public MetaDataService(
        IRepoWrapper repoWrapper,
           IRedisCacheService redisCacheService,
        ILoggerManager<MetaDataService> logger)
    {
        _repoWrapper = repoWrapper;
        _redisCacheService = redisCacheService;
        _logger = logger;
    }

    /// <summary>
    /// Gets a reference term by RefTermId.
    /// </summary>
    public async Task<RefTermDto> GetRefTermByIdAsync(
        Guid refTermId,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Executing GetRefTermByIdAsync.");

        RefTerm? refTerm = await _repoWrapper.RefTermRepository
            .FindFirstByConditionAsync(
                r => r.Id == refTermId,
                cancellationToken);

        if (refTerm is null)
        {
            NotFoundCustomException exception = new NotFoundCustomException(
                $"RefTerm with RefTermId {refTermId} does not exist.",
                $"RefTerm with RefTermId {refTermId} does not exist.");

            _logger.LogError(
                "RefTerm not found for RefTermId: {RefTermId}",
                exception,
                refTermId);

            throw exception;
        }

        RefTermDto response = new RefTermDto
        {
            RefTermId = refTerm.Id,
            RefTermKey = refTerm.RefTermKey,
            Description = refTerm.Description
        };

        _logger.LogDebug(
            "RefTerm fetched successfully. RefTermId: {RefTermId}, RefTermKey: {RefTermKey}",
            response.RefTermId,
            response.RefTermKey);

        return response;
    }

    /// <summary>
    /// Gets a reference term by RefTermKey.
    /// </summary>
    public async Task<RefTermDto> GetRefTermByKeyAsync(
        string refTermKey,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Executing GetRefTermByKeyAsync.");


        RefTerm? refTerm = await _repoWrapper.RefTermRepository
            .FindFirstByConditionAsync(
                r => r.RefTermKey == refTermKey,
                cancellationToken);

        if (refTerm is null)
        {
            NotFoundCustomException exception = new NotFoundCustomException(
                $"RefTerm with RefTermKey {refTermKey} does not exist.",
                $"RefTerm with RefTermKey {refTermKey} does not exist.");

            _logger.LogError(
                "Refterm not found for refterm key: {RefTermKey}",
                exception,
                refTermKey);

            throw exception;
        }

        RefTermDto response = new RefTermDto
        {
            RefTermId = refTerm.Id,
            RefTermKey = refTerm.RefTermKey,
            Description = refTerm.Description
        };

        _logger.LogDebug(
            "Refterm fetched successfully. refterm id: {RefTermId}, refterm key: {RefTermKey}",
            response.RefTermId,
            response.RefTermKey);

        return response;
    }

    /// <summary>
    /// Gets all reference terms by RefSetKey.
    /// </summary>
    public async Task<List<RefTermDto>> GetRefTermsByRefSetKeyAsync(
        string refSetKey,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Executing GetRefTermsByRefSetKeyAsync.");


        string normalizedRefSetKey = refSetKey.Trim();

        RefSet? refSet = await _repoWrapper.RefSetRepository
            .FindByCondition(refSet =>
                refSet.IsActive &&
                refSet.RefSetKey != null &&
                EF.Functions.ILike(
                    refSet.RefSetKey.Trim(),
                    normalizedRefSetKey))
            .FirstOrDefaultAsync(cancellationToken);

        if (refSet is null)
        {
            NotFoundCustomException exception = new NotFoundCustomException(
                $"RefSet with RefSetKey {refSetKey} does not exist.",
                $"RefSet with RefSetKey {refSetKey} does not exist.");

            _logger.LogError(
                "Refset not found for refset key: {RefSetKey}",
                exception,
                refSetKey);

            throw exception;
        }

        List<SetRefTerm> setRefTermList = await _repoWrapper.SetRefTermRepository
            .FindByCondition(s => s.RefSetId == refSet.Id)
            .ToListAsync(cancellationToken);

        if (setRefTermList.Count == 0)
        {
            NotFoundCustomException exception = new NotFoundCustomException(
                $"No RefTerms mapped for RefSetKey {refSetKey}.",
                $"No RefTerms mapped for RefSetKey {refSetKey}.");

            _logger.LogError(
                "No set refterms found for refset id: {RefSetId}",
                exception,
                refSet.Id);

            throw exception;
        }

        List<Guid> refTermIdList = setRefTermList
            .Select(s => s.RefTermId)
            .ToList();

        List<RefTerm> refTermList = await _repoWrapper.RefTermRepository
            .FindByCondition(r => refTermIdList.Contains(r.Id))
            .ToListAsync(cancellationToken);

        List<RefTermDto> response = refTermList
            .Select(r => new RefTermDto
            {
                RefTermId = r.Id,
                RefTermKey = r.RefTermKey,
                Description = r.Description
            })
            .ToList();

        _logger.LogDebug(
            "Refterms fetched successfully for refset key: {RefSetKey}, Count: {Count}",
            refSetKey,
            response.Count);

        return response;
    }
}
