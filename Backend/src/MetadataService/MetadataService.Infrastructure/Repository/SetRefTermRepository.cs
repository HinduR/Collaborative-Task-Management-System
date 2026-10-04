using MetadataService.Application.Contract.IRepository;
using MetadataService.Domain.Models;
using MetadataService.Infrastructure.Persistence.ApplicationContext;
using Shared.Common.Repository;

namespace MetadataService.Infrastructure.Repository;

/// <summary>
/// Repository implementation for managing <see cref="SetRefTerm"/> entities.
/// </summary>
public class SetRefTermRepository
    : RepositoryBase<SetRefTerm, MetadataDbContext>,
      ISetRefTermRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetRefTermRepository"/> class.
    /// </summary>
    /// <param name="appDbContext">Application database context.</param>
    public SetRefTermRepository(MetadataDbContext appDbContext)
        : base(appDbContext)
    {
    }
}