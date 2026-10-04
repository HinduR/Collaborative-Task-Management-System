    using IdentityService.Application.AuthenticationModule.Contract.IRepository;
using IdentityService.Application.RoleModule.Contract.IRepository;
using IdentityService.Application.UserModule.Contract.IRepository;

    namespace IdentityService.Application.Common;

    public interface IRepoWrapper
    {
        IUserRepository UserRepository { get; }

        IRoleRepository RoleRepository { get; }

        IFeatureRepository FeatureRepository { get; }

        IUserRoleMappingRepository UserRoleMappingRepository { get; }

        IRoleFeatureMappingRepository RoleFeatureMappingRepository { get; }
        IRefreshTokenRepository RefreshTokenRepository { get; }

        Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default);
    }