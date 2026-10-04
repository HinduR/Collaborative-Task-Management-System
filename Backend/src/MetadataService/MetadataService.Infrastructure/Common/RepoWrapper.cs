using MetadataService.Application.Common;
using MetadataService.Application.Contract.IRepository;
using MetadataService.Infrastructure.Persistence.ApplicationContext;
using MetadataService.Infrastructure.Repository;
using Microsoft.Extensions.Configuration;
using Shared.Common.contracts;

namespace MetadataService.Infrastructure.Common;

public class RepoWrapper : IRepoWrapper
{
    private readonly MetadataDbContext _context;
    private readonly IUserContext _userContext;

    private IRefTermRepository? _refTermRepository;

    private IRefSetRepository? _refSetRepository;

    private ISetRefTermRepository? _setRefTermRepository;

    public RepoWrapper(MetadataDbContext context, IUserContext userContext)
    {
        _context = context;
        _userContext = userContext;
    }
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveAsync(_userContext.GetUserId());
    }
    public IRefTermRepository RefTermRepository =>
_refTermRepository ??=
    new RefTermRepository(_context);

    public IRefSetRepository RefSetRepository =>
    _refSetRepository ??=
        new RefSetRepository(_context);

    public ISetRefTermRepository SetRefTermRepository =>
    _setRefTermRepository ??=
        new SetRefTermRepository(_context);
}
