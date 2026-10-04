using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Shared.Common.contracts;

namespace Shared.Common.Repository;

/// <summary>
/// Provides a base repository with common data access methods.
/// </summary>
/// <typeparam name="T">The type of the entity.</typeparam>
/// <typeparam name="Context">The type of the database context.</typeparam>
public abstract class RepositoryBase<T, Context> : IRepositoryBase<T>
    where T : class where Context : DbContext
{
    protected Context RepositoryContext { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryBase{T, Context}"/> class.
    /// </summary>
    /// <param name="repositoryContext">The repository context for data access.</param>
    protected RepositoryBase(Context repositoryContext)
    {
        RepositoryContext = repositoryContext;
    }

    /// <summary>
    /// Creates a new entity in the repository.
    /// </summary>
    /// <param name="entity">The entity to create.</param>
    public void Create(T entity)
    {
        _ = RepositoryContext.Set<T>().Add(entity);
    }

    /// <summary>
    /// Creates a new entity in the repository asynchronously.
    /// </summary>
    /// <param name="entity">The entity to create.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task CreateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _ = await RepositoryContext.Set<T>().AddAsync(entity, cancellationToken);
    }

    /// <summary>
    /// Creates a range of entities in the repository asynchronously.
    /// </summary>
    /// <param name="entityList">The list of entities to create.</param>
    public async Task CreateRangeAsync(List<T> entityList, CancellationToken cancellationToken = default)
    {
        await RepositoryContext.Set<T>().AddRangeAsync(entityList, cancellationToken);
    }

    /// <summary>
    /// Creates a range of new entities in the repository.
    /// </summary>
    /// <param name="entities">The list of entities to create.</param>
    public void CreateRange(List<T> entityList)
    {
        RepositoryContext.Set<T>().AddRange(entityList);
    }

    /// <summary>
    /// Updates an existing entity in the repository.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    public void Update(T entity)
    {
        _ = RepositoryContext.Set<T>().Update(entity);
    }

    public void UpdateRange(List<T> entityList)
    {
        RepositoryContext.Set<T>().UpdateRange(entityList);
    }

    /// <summary>
    /// Deletes the specified entity from the repository.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    public void Delete(T entity)
    {
        RepositoryContext.Set<T>().Remove(entity);
    }

    /// <summary>
    /// Finds entities in the repository that match the specified condition.
    /// </summary>
    /// <param name="expression">The condition to match.</param>
    /// <returns>An <see cref="IQueryable{T}"/> of matching entities.</returns>
    public IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression)
    {
        return RepositoryContext.Set<T>().Where(expression).AsNoTracking();
    }

    /// <summary>
    /// Finds the first entity in the repository that matches the specified condition.
    /// </summary>
    /// <param name="expression">The condition to match.</param>
    /// <returns>The first matching entity, or null if no match is found.</returns>
    public T? FindFirstByCondition(Expression<Func<T, bool>> expression)
    {
        return RepositoryContext.Set<T>().Where(expression).FirstOrDefault();
    }

    /// <summary>
    /// Saves changes to the database.
    /// </summary>
    /// <returns>The number of rows affected.</returns>
    public int SaveChanges()
    {
        return RepositoryContext.SaveChanges();
    }

    /// <summary>
    /// Saves changes to the database asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, with the number of rows affected.</returns>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await RepositoryContext.SaveChangesAsync(cancellationToken);
    }
    /// <summary>
    /// Determines whether any entities in the repository match the specified condition.
    /// </summary>
    /// <param name="expression">The condition to match.</param>
    /// <returns><c>true</c> if any entities match the condition; otherwise, <c>false</c>.</returns>
    public bool AnyByCondition(Expression<Func<T, bool>> expression)
    {
        return RepositoryContext.Set<T>().Any(expression);
    }
    /// <summary>
    /// Asynchronously determines whether any entities in the repository match the specified condition.
    /// </summary>
    /// <param name="expression">The condition to match.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>true</c> if any entities match the condition; otherwise, <c>false</c>.</returns>
    public async Task<bool> AnyByConditionAsync(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default)
    {
        return await RepositoryContext.Set<T>().AnyAsync(expression, cancellationToken);
    }
    /// <summary>
    /// Asynchronously finds the first entity in the repository that matches the specified condition.
    /// </summary>
    /// <param name="expression">The condition to match.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The first matching entity, or null if no match is found.</returns>
    public async Task<T?> FindFirstByConditionAsync(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default)
    {
        return await RepositoryContext.Set<T>().Where(expression).FirstOrDefaultAsync(cancellationToken);
    }
    /// <summary>
    /// Deletes a range of existing entities.
    /// </summary>
    /// <param name="entityList">The list of entities to delete.</param>
    public void DeleteRange(List<T> entityList)
    {
        RepositoryContext.Set<T>().RemoveRange(entityList);
    }

    /// <summary>
    /// Asynchronously finds the first entity in the repository that matches the specified condition.
    /// </summary>
    /// <param name="expression">The condition to match.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The first matching entity, or null if no match is found.</returns>
    public async Task<T?> FindFirstByConditionAsNoTrackingAsync(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default)
    {
        return await RepositoryContext.Set<T>().Where(expression).AsNoTracking().FirstOrDefaultAsync(cancellationToken);
    }
}