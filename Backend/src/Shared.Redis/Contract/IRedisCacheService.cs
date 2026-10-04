namespace Shared.Redis.Contract
{
    /// <summary>
    /// Represents the IRedisCacheService component.
    /// </summary>
    public interface IRedisCacheService
    {
        /// <summary>
        /// Stores a value in the Redis cache with an optional expiry.
        /// </summary>
        /// <typeparam name="T">The type of the value to store.</typeparam>
        /// <param name="key">The key under which the value will be stored.</param>
        /// <param name="value">The value to store in the cache.</param>
        /// <param name="expiry">The optional expiry time for the cached value.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);

        /// <summary>
        /// Retrieves a value from the Redis cache.
        /// </summary>
        /// <typeparam name="T">The type of the value to retrieve.</typeparam>
        /// <param name="key">The key of the cached value to retrieve.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. 
        /// The task result contains the value from the cache if found; otherwise, <c>null</c>.
        /// </returns>
        Task<T?> GetAsync<T>(string key);

        /// <summary>
        /// Stores a byte array in the Redis cache with an optional expiry.
        /// </summary>
        /// <param name="key">The key under which the value will be stored.</param>
        /// <param name="value">The byte array to store in the cache.</param>
        /// <param name="expiry">The optional expiry time for the cached value.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task SetBytesAsync(string key, byte[] value, TimeSpan? expiry = null);

        /// <summary>
        /// Retrieves a byte array from the Redis cache.
        /// </summary>
        /// <param name="key">The key of the cached value to retrieve.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. 
        /// The task result contains the byte array from the cache if found; otherwise, <c>null</c>.
        /// </returns>
        Task<byte[]?> GetBytesAsync(string key);

        /// <summary>
        /// Hashes a Redis key without changing generic cache behavior.
        /// </summary>
        /// <param name="key">The raw Redis key.</param>
        /// <returns>The hashed Redis key.</returns>
        string HashKey(string key);
        // <summary>
        /// Deletes a given key from the Redis cache.
        /// </summary>
        /// <param name="key">The key of the cached value to retrieve.</param>
        Task DeleteAsync(string key);
        /// <summary>
        /// Attempts to acquire a distributed lock by setting a key in Redis with a unique token,
        /// only if the key does not already exist.
        /// </summary>
        /// <param name="key">The Redis key used to represent the lock.</param>
        /// <param name="expiry">The time after which the lock will automatically expire.</param>
        /// <returns>
        /// A unique token string if the lock was successfully acquired; otherwise, <c>null</c>.
        /// This token should be passed to <see cref="ReleaseLockAsync"/> to safely release the lock.
        /// </returns>
        Task<string?> TryAcquireLockAsync(string key, TimeSpan expiry);
        /// <summary>
        /// Releases a previously acquired distributed lock by deleting the Redis key
        /// only if its current value matches the provided token. This ensures that
        /// only the owner of the lock can release it.
        /// </summary>
        /// <param name="key">The Redis key representing the lock.</param>
        /// <param name="token">The unique token that was returned when the lock was acquired.</param>
        /// <returns>
        /// <c>true</c> if the lock was successfully released (key deleted); otherwise, <c>false</c>.
        /// </returns>
        Task<bool> ReleaseLockAsync(string key, string token);

        // <summary>
        /// Deletes a given key from the Redis cache.
        /// </summary>
        /// <param name="key">List of keys of the cached value to retrieve.</param>
        Task DeleteAsync(List<string> key);
    }
}
