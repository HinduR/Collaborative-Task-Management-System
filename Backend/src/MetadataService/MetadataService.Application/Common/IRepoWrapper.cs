using MetadataService.Application.Contract.IRepository;

namespace MetadataService.Application.Common;

public interface IRepoWrapper
{
  public IRefSetRepository RefSetRepository { get; }

  public IRefTermRepository RefTermRepository { get; }

  public ISetRefTermRepository SetRefTermRepository { get; }
  /// <summary>
  /// Asynchronously saves changes to the database.
  /// </summary>
  /// <param name="cancellationToken">A token to cancel the operation.</param>
  /// <returns>The number of state entries written to the database.</returns>
  Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
