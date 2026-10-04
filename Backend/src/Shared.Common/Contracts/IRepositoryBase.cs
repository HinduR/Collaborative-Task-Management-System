using System.Linq.Expressions;

namespace Shared.Common.contracts
{
    /// <summary>
    /// Interface <c>IRepositoryBase</c> used to define the methods related for repository base.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="RContext"></typeparam>
    /// <typeparam name="WContext"></typeparam>
    public interface IRepositoryBase<T>
    {
        /// <summary>
        /// The Create function in C# is used to create a new entity.
        /// </summary>
        /// <param name="entity">The "T" in the method signature `void Create(T entity);` represents a
        /// generic type parameter. This means that the method can work with any data type specified by
        /// the caller when the method is invoked. The actual type will be determined at compile time
        /// based on how the method is used.</param>
        void Create(T entity);

        /// <summary>
        /// Creates a range of entities in the repository.
        /// </summary>
        /// <param name="entityList">The list of entities to create.</param>
        void CreateRange(List<T> entityList);

        /// <summary>
        /// Creates a range of entities in the repository asynchronously.
        /// </summary>
        /// <param name="entityList">The list of entities to create.</param>
        Task CreateRangeAsync(List<T> entityList, CancellationToken cancellationToken = default);


        /// <summary>
        /// Creates a new entity in the repository asynchronously.
        /// </summary>
        /// <param name="entity">The entity to create.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        Task CreateAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// The Update function in C# is used to update a new entity.
        /// </summary>
        /// <param name="entity"></param>
        void Update(T entity);

        /// <summary>
        /// Updates a range of existing entities.
        /// </summary>
        /// <param name="entityList">The list of entities to update.</param>
        void UpdateRange(List<T> entityList);

        /// <summary>
        /// Deletes the specified entity from the repository.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        void Delete(T entity);

        /// <summary>
        /// Finds the entities in the repository that matches the specified condition.
        /// </summary>
        /// <param name="entity">The entity to fetch.</param>
        IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression);

        /// <summary>
        /// Finds the first entity in the repository that matches the specified condition.
        /// </summary>
        /// <param name="entity">The entity to fetch.</param>
        T? FindFirstByCondition(Expression<Func<T, bool>> expression);

        /// <summary>
        /// Saves the changes in the repository asynchronously.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The save status.</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Determines whether any entities in the repository match the specified condition.
        /// </summary>
        /// <param name="expression">The condition to match.</param>
        /// <returns><c>true</c> if any entities match the condition; otherwise, <c>false</c>.</returns>
        bool AnyByCondition(Expression<Func<T, bool>> expression);

        /// <summary>
        /// Asynchronously determines whether any entities in the repository match the specified condition.
        /// </summary>
        /// <param name="expression">The condition to match.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns><c>true</c> if any entities match the condition; otherwise, <c>false</c>.</returns>
        Task<bool> AnyByConditionAsync(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously finds the first entity in the repository that matches the specified condition.
        /// </summary>
        /// <param name="expression">The condition to match.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The first matching entity, or null if no match is found.</returns>
        Task<T?> FindFirstByConditionAsync(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default);
        /// <summary>
        /// Deletes a range of existing entities.
        /// </summary>
        /// <param name="entityList">The list of entities to delete.</param>
        void DeleteRange(List<T> entityList);

        /// <summary>
        /// Asynchronously finds the first entity in the repository that matches the specified condition.
        /// </summary>
        /// <param name="expression">The condition to match.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The first matching entity, or null if no match is found.</returns>
        Task<T?> FindFirstByConditionAsNoTrackingAsync(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default);
    }
}