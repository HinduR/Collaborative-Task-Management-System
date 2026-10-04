using MetadataService.Application.Contract.IRepository;
using MetadataService.Domain.Models;
using MetadataService.Infrastructure.Persistence.ApplicationContext;
using Shared.Common.Repository;

namespace MetadataService.Infrastructure.Repository;

/// <summary>
/// Repository implementation for managing <see cref="RefTerm"/> entities.
/// </summary>
public class RefTermRepository
    : RepositoryBase<RefTerm, MetadataDbContext>,
      IRefTermRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RefTermRepository"/> class.
    /// </summary>
    /// <param name="appDbContext">Application database context.</param>
    public RefTermRepository(MetadataDbContext appDbContext)
        : base(appDbContext)
    {
    }
}
