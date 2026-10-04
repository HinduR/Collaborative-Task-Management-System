

using IdentityService.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Shared.Common.contracts;
using Shared.Common.Model;
using Shared.Common.Util;

namespace IdentityService.Infrastructure.Persistence.ApplicationContext;

public sealed class IdentityDbContext : DbContext
{
    public IdentityDbContext(
        DbContextOptions<IdentityDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> User { get; set; }
    public DbSet<Role> Role { get; set; }
    public DbSet<Feature> Feature { get; set; }
    public DbSet<UserRoleMapping> UserRoleMapping { get; set; }
    public DbSet<RoleFeatureMapping> RoleFeatureMapping { get; set; }
    public DbSet<RefreshToken> RefreshToken { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("identity");

        foreach (IMutableEntityType entity in modelBuilder.Model.GetEntityTypes())
        {
            // Convert table names to snake_case
            entity.SetTableName(entity.GetTableName()!.ConvertToSnakeCase());

            var storeObjectIdentifier = StoreObjectIdentifier.Table(
                entity.GetTableName()!,
                entity.GetSchema()
            );

            foreach (IMutableProperty property in entity.GetProperties())
            {

                property.SetColumnName(
                    property.GetColumnName(storeObjectIdentifier)!.ConvertToSnakeCase()
                );

                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                {
                    property.SetColumnType("timestamp without time zone");
                }
            }

            // Convert key names to snake_case
            foreach (IMutableKey key in entity.GetKeys())
            {
                key.SetName(key.GetName()!.ConvertToSnakeCase());
            }

            // Convert foreign key names to snake_case
            foreach (IMutableForeignKey key in entity.GetForeignKeys())
            {
                key.SetConstraintName(key.GetConstraintName()!.ConvertToSnakeCase());
            }

            foreach (IMutableIndex index in entity.GetIndexes())
            {
                index.SetDatabaseName(index.GetDatabaseName()!.ConvertToSnakeCase());
            }
        }
    }

    private void ApplyAuditFields(Guid userId)
    {
        var currentTime = DateTime.Now;

        foreach (var entry in ChangeTracker.Entries<BaseModel>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = currentTime;
                entry.Entity.CreatedBy = userId;
                entry.Entity.UpdatedAt = currentTime;
                entry.Entity.UpdatedBy = userId;
                entry.Entity.IsActive = true;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = currentTime;
                entry.Entity.UpdatedBy = userId;

            }
        }
    }

    public async Task<int> SaveAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        ApplyAuditFields(userId);
        return await SaveChangesAsync(cancellationToken);
    }
}