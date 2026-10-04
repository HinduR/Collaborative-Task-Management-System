using IdentityService.Application.AuthenticationModule.Contract.IRepository;
using IdentityService.Application.Common;
using IdentityService.Application.RoleModule.Contract.IRepository;
using IdentityService.Application.UserModule.Contract.IRepository;
using IdentityService.Infrastructure.AuthenticationModule.Repository;
using IdentityService.Infrastructure.Persistence.ApplicationContext;
using IdentityService.Infrastructure.RoleModule.Repository;
using IdentityService.Infrastructure.UserModule.Repository;
using Shared.Common.contracts;

namespace IdentityService.Infrastructure.Common;

public class RepoWrapper : IRepoWrapper
{
    private readonly IdentityDbContext _context;
    private readonly IUserContext _userContext;

    private IUserRepository? _userRepository;
    private IRoleRepository? _roleRepository;
    private IFeatureRepository? _featureRepository;
    private IUserRoleMappingRepository? _userRoleMappingRepository;
    private IRoleFeatureMappingRepository? _roleFeatureMappingRepository;
    private IRefreshTokenRepository? _refreshTokenRepository;


    public RepoWrapper(
        IdentityDbContext context,
        IUserContext userContext)
    {
        _context = context;
        _userContext = userContext;
    }

    public IUserRepository UserRepository
    {
        get
        {
            _userRepository ??= new UserRepository(_context);

            return _userRepository;
        }
    }

    public IRoleRepository RoleRepository
    {
        get
        {
            _roleRepository ??= new RoleRepository(_context);

            return _roleRepository;
        }
    }

    public IFeatureRepository FeatureRepository
    {
        get
        {
            _featureRepository ??= new FeatureRepository(_context);

            return _featureRepository;
        }
    }

    public IUserRoleMappingRepository UserRoleMappingRepository
    {
        get
        {
            _userRoleMappingRepository ??=
                new UserRoleMappingRepository(_context);

            return _userRoleMappingRepository;
        }
    }

    public IRoleFeatureMappingRepository RoleFeatureMappingRepository
    {
        get
        {
            _roleFeatureMappingRepository ??=
                new RoleFeatureMappingRepository(_context);

            return _roleFeatureMappingRepository;
        }
    }

    public IRefreshTokenRepository RefreshTokenRepository
    {
        get
        {
            _refreshTokenRepository ??=
                new RefreshTokenRepository(_context);

            return _refreshTokenRepository;
        }
    }
    public Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _context.SaveAsync(
            _userContext.GetUserId(),
            cancellationToken);
    }
}