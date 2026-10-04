using BoardTaskService.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Shared.Common.contracts;
using Shared.Common.Model;
using Shared.Common.Util;

namespace BoardTaskService.Infrastructure.Persistence.ApplicationContext;

public sealed class BoardTaskDbContext : DbContext
{
    public BoardTaskDbContext(
        DbContextOptions<BoardTaskDbContext> options)
        : base(options)
    {
    }

    public DbSet<Project> Project { get; set; }

    public DbSet<UserProjectMapping> UserProjectMapping { get; set; }

    public DbSet<Board> Board { get; set; }

    public DbSet<BoardAccess> BoardAccess { get; set; }

    public DbSet<WorkflowColumn> WorkflowColumn { get; set; }

    public DbSet<BoardTask> BoardTask { get; set; }

    public DbSet<TaskComment> TaskComment { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("boardtask");
        modelBuilder.Entity<Board>().HasIndex(board => new { board.ProjectId, board.Name })
                    .IsUnique();

        modelBuilder.Entity<WorkflowColumn>().HasIndex(column => new { column.BoardId, column.Name })
            .IsUnique();

        modelBuilder.Entity<BoardTask>().HasIndex(task => task.WorkflowColumnId);

        modelBuilder.Entity<TaskComment>().HasIndex(comment => comment.TaskId);
        foreach (IMutableEntityType entity in modelBuilder.Model.GetEntityTypes())
        {
            // Convert table names to snake_case
            entity.SetTableName(entity.GetTableName()!.ConvertToSnakeCase());

            StoreObjectIdentifier storeObjectIdentifier =
                StoreObjectIdentifier.Table(
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
        DateTime currentTime = DateTime.Now;

        foreach (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<BaseModel> entry in ChangeTracker.Entries<BaseModel>())
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