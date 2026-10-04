using Shared.Common.Dto;

namespace MetadataService.Application.Contract.IService;

/// <summary>
/// Service contract for metadata operations.
/// </summary>
public interface IMetaDataService
{
    /// <summary>
    /// Retrieves a reference term by identifier.
    /// </summary>
    /// <param name="refTermId">Reference term identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Reference term details.</returns>
    Task<RefTermDto> GetRefTermByIdAsync(
        Guid refTermId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a reference term by key.
    /// </summary>
    /// <param name="refTermKey">Reference term key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Reference term details.</returns>
    Task<RefTermDto> GetRefTermByKeyAsync(
        string refTermKey,
        CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves all reference terms for a reference set key.
    /// </summary>
    /// <param name="refSetKey">Reference set key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of reference terms.</returns>
    Task<List<RefTermDto>> GetRefTermsByRefSetKeyAsync(
        string refSetKey,
        CancellationToken cancellationToken);
}