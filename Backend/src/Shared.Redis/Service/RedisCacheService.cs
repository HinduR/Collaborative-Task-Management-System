using System.Text.Json;
using System.Security.Cryptography;
using System.Text;
using Shared.Logging.Contracts;
using Shared.Redis.Contract;
using StackExchange.Redis;

namespace Shared.Redis.Service
{
    /// <summary>
    /// Provides methods for interacting with a Redis cache.
    /// </summary>
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly IDatabase _database;
        private readonly ILoggerManager<RedisCacheService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisCacheService"/> class.
        /// </summary>
        /// <param name="connectionMultiplexer">The Redis connection multiplexer.</param>
        public RedisCacheService(IConnectionMultiplexer connectionMultiplexer, ILoggerManager<RedisCacheService> logger)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _database = _connectionMultiplexer.GetDatabase();
            _logger = logger;
        }

        /// <summary>
        /// Stores a value in the Redis cache with an optional expiry.
        /// </summary>
        /// <typeparam name="T">The type of the value to store.</typeparam>
        /// <param name="key">The key under which the value will be stored.</param>
        /// <param name="value">The value to store in the cache.</param>
        /// <param name="expiry">The optional expiry time for the cached value.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            try
            {
                var jsonData = JsonSerializer.Serialize(value);
                await _database.StringSetAsync(key, jsonData, expiry);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error setting key: {key} in Redis.", ex, key);
            }
        }

        // <summary>
        /// Deletes a given key from the Redis cache.
        /// </summary>
        /// <param name="key">The key of the cached value to retrieve.</param>
        public async Task DeleteAsync(string key)
        {
            _logger.LogDebug(
                "Executing DeleteAsync.");

            try
            {
                await _database.KeyDeleteAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error setting key: {key} in Redis.", ex, key);
            }
        }

        /// <summary>
        /// Retrieves a value from the Redis cache.
        /// </summary>
        /// <typeparam name="T">The type of the value to retrielve.</typeparam>
        /// <param name="key">The key of the cached value to retrieve.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. 
        /// The task result contains the value from the cache if found; otherwise, <c>null</c>.
        /// </returns>
        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var jsonData = await _database.StringGetAsync(key);
                if (jsonData.IsNullOrEmpty)
                {
                    return default;
                }
                return JsonSerializer.Deserialize<T>(jsonData!);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving key: {key} in Redis.", ex, key);
                return default;
            }
        }

        /// <summary>
        /// Stores a byte array in the Redis cache with an optional expiry.
        /// </summary>
        /// <param name="key">The key under which the value will be stored.</param>
        /// <param name="value">The byte array to store in the cache.</param>
        /// <param name="expiry">The optional expiry time for the cached value.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task SetBytesAsync(string key, byte[] value, TimeSpan? expiry = null)
        {
            _logger.LogDebug(
                "Executing SetBytesAsync.");

            try
            {
                await _database.StringSetAsync(key, value, expiry);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error setting key: {key} in Redis.", ex, key);
            }
        }

        /// <summary>
        /// Retrieves a byte array from the Redis cache.
        /// </summary>
        /// <param name="key">The key of the cached value to retrieve.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. 
        /// The task result contains the byte array from the cache if found; otherwise, <c>null</c>.
        /// </returns>
        public async Task<byte[]?> GetBytesAsync(string key)
        {
            _logger.LogDebug(
                "Executing GetBytesAsync.");

            try
            {
                var data = await _database.StringGetAsync(key);
                if (data.IsNullOrEmpty)
                {
                    return null;
                }
                return data!;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error getting key: {key} in Redis.", ex, key);
                return null;
            }
        }

        /// <summary>
        /// Computes a deterministic, hashed representation of the specified key.
        /// </summary>
        /// <param name="key">The key to hash.</param>
        /// <returns>A lowercase hexadecimal SHA-256 hash of the key.</returns>
        public string HashKey(string key)
        {
            _logger.LogDebug(
                "Executing HashKey.");

            return ComputeHashedKey(key);
        }

        /// <summary>
        /// Computes a SHA-256 hash of the specified key and returns it as a lowercase hexadecimal string.
        /// </summary>
        /// <param name="key">The key to hash.</param>
        /// <returns>A lowercase hexadecimal SHA-256 hash of the key.</returns>
        public static string ComputeHashedKey(string key)
        {
            byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(key));
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

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
        public async Task<string?> TryAcquireLockAsync(string key, TimeSpan expiry)
        {
            _logger.LogDebug(
                "Executing TryAcquireLockAsync.");

            try
            {
                string token = Guid.NewGuid().ToString();

                bool isAcquired = await _database.StringSetAsync(
                    key,
                    token,
                    expiry,
                    when: When.NotExists);

                return isAcquired ? token : null;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error acquiring lock for key: {key}", ex, key);
                return null;
            }
        }

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
        public async Task<bool> ReleaseLockAsync(string key, string token)
        {
            _logger.LogDebug(
                "Executing ReleaseLockAsync.");

            const string unlockScript = @"
        if redis.call('get', KEYS[1]) == ARGV[1] then
            return redis.call('del', KEYS[1])
        else
            return 0
        end";

            try
            {
                int result = (int)await _database.ScriptEvaluateAsync(
                    unlockScript,
                    [key],
                    [token]);

                return result == 1;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error releasing lock for key: {key}", ex, key);
                return false;
            }
        }

        // <summary>
        /// Deletes a given key from the Redis cache.
        /// </summary>
        /// <param name="key">List of keys of the cached value to retrieve.</param>
        public async Task DeleteAsync(List<string> key)
        {
            _logger.LogDebug(
                "Executing DeleteAsync.");

            try
            {
                RedisKey[] redisKeys = key.Select(k => (RedisKey)k).ToArray();
                await _database.KeyDeleteAsync(redisKeys);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error setting key: {key} in Redis.", ex, key);
            }
        }

    }
}
