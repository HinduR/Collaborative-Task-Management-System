using MetadataService.Application.Contract.IRepository;
using MetadataService.Domain.Models;
using MetadataService.Infrastructure.Persistence.ApplicationContext;
using Shared.Common.Repository;

namespace MetadataService.Infrastructure.Repository;

/// <summary>
/// Repository implementation for managing <see cref="RefSet"/> entities.
/// </summary>
public class RefSetRepository
    : RepositoryBase<RefSet, MetadataDbContext>,
      IRefSetRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RefSetRepository"/> class.
    /// </summary>
    /// <param name="appDbContext">Application database context.</param>
    public RefSetRepository(MetadataDbContext appDbContext)
        : base(appDbContext)
    {
    }
}
