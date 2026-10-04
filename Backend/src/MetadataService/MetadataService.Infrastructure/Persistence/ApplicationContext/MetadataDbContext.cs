
using MetadataService.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Shared.Common.Model;
using Shared.Common.Util;

namespace MetadataService.Infrastructure.Persistence.ApplicationContext;

public sealed class MetadataDbContext : DbContext
{
    public MetadataDbContext(
        DbContextOptions<MetadataDbContext> options)
        : base(options)
    {
    }

    public DbSet<RefSet> RefSet { get; set; }

    public DbSet<RefTerm> RefTerm { get; set; }

    public DbSet<SetRefTerm> SetRefTerm { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("metadata");

        foreach (IMutableEntityType entity in modelBuilder.Model.GetEntityTypes())
        {
            entity.SetTableName(
                entity.GetTableName()!.ConvertToSnakeCase());

            StoreObjectIdentifier storeObjectIdentifier =
                StoreObjectIdentifier.Table(
                    entity.GetTableName()!,
                    entity.GetSchema());

            foreach (IMutableProperty property in entity.GetProperties())
            {
                property.SetColumnName(
                    property.GetColumnName(storeObjectIdentifier)!
                        .ConvertToSnakeCase());

                if (property.ClrType == typeof(DateTime) ||
                    property.ClrType == typeof(DateTime?))
                {
                    property.SetColumnType("timestamp without time zone");
                }
            }

            foreach (IMutableKey key in entity.GetKeys())
            {
                key.SetName(key.GetName()!.ConvertToSnakeCase());
            }

            foreach (IMutableForeignKey foreignKey in entity.GetForeignKeys())
            {
                foreignKey.SetConstraintName(
                    foreignKey.GetConstraintName()!
                        .ConvertToSnakeCase());
            }

            foreach (IMutableIndex index in entity.GetIndexes())
            {
                index.SetDatabaseName(
                    index.GetDatabaseName()!
                        .ConvertToSnakeCase());
            }
        }
    }

    private void ApplyAuditFields(Guid userId)
    {
        DateTime currentTime = DateTime.Now;

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

    public async Task<int> SaveAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        ApplyAuditFields(userId);
        return await SaveChangesAsync(cancellationToken);
    }
}