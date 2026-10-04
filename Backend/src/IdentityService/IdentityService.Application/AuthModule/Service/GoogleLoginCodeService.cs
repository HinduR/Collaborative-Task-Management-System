using System.Collections.Concurrent;
using System.Security.Cryptography;
using IdentityService.Application.AuthenticationModule.Contract.IService;
using IdentityService.Application.AuthenticationModule.Dto;
using Microsoft.Extensions.Caching.Memory;
using Shared.Cryptography.Application.Cryptography.Contract;

namespace IdentityService.Application.AuthenticationModule.Service;

/// <summary>
/// Provides in-memory creation and consumption of short-lived Google login codes.
/// </summary>
public class GoogleLoginCodeService : IGoogleLoginCodeService
{
    private static readonly TimeSpan CodeLifetime =
        TimeSpan.FromMinutes(1);

    private readonly IMemoryCache _memoryCache;
    private readonly IAuthenticity _authenticity;
    private readonly ConcurrentDictionary<string, object> _codeLocks = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GoogleLoginCodeService"/> class.
    /// </summary>
    /// <param name="memoryCache">Memory cache used to store hashed one-time login codes.</param>
    /// <param name="authenticity">Hashing service used to hash login codes before caching or lookup.</param>
    public GoogleLoginCodeService(
        IMemoryCache memoryCache,
        IAuthenticity authenticity)
    {
        _memoryCache = memoryCache;
        _authenticity = authenticity;
    }

    /// <summary>
    /// Creates a cryptographically secure one-time login code and stores the
    /// supplied login context against the code hash for a short lifetime.
    /// </summary>
    /// <param name="loginContext">The minimum login context needed to complete token exchange.</param>
    /// <returns>The plaintext one-time code to send back to the browser redirect.</returns>
    public string CreateCode(
        GoogleLoginCodeCacheDto loginContext)
    {
        string code = CreateSecureCode();
        string codeHash = HashCode(code);

        _memoryCache.Set(
            codeHash,
            loginContext,
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CodeLifetime
            });

        return code;
    }

    /// <summary>
    /// Atomically consumes a valid one-time login code and removes it from
    /// memory so the same code cannot be exchanged twice.
    /// </summary>
    /// <param name="code">The plaintext one-time code supplied during exchange.</param>
    /// <returns>
    /// The cached login context when the code is valid and unused; otherwise,
    /// <see langword="null"/>.
    /// </returns>
    public GoogleLoginCodeCacheDto? ConsumeCode(
        string code)
    {
        string codeHash = HashCode(code.Trim());
        object codeLock = _codeLocks.GetOrAdd(
            codeHash,
            _ => new object());

        try
        {
            lock (codeLock)
            {
                if (!_memoryCache.TryGetValue(
                        codeHash,
                        out GoogleLoginCodeCacheDto? loginContext) ||
                    loginContext is null)
                {
                    return null;
                }

                _memoryCache.Remove(codeHash);

                return loginContext;
            }
        }
        finally
        {
            _codeLocks.TryRemove(
                codeHash,
                out _);
        }
    }

    /// <summary>
    /// Hashes a plaintext login code before it is stored or looked up.
    /// </summary>
    /// <param name="code">The plaintext login code.</param>
    /// <returns>The SHA-256 hash of the login code.</returns>
    private string HashCode(
        string code)
    {
        using SHA256 sha256 = SHA256.Create();

        return _authenticity.Hash(
            code,
            sha256);
    }

    /// <summary>
    /// Creates a URL-safe random code using a cryptographically secure random
    /// number generator.
    /// </summary>
    /// <returns>A URL-safe random login code.</returns>
    private static string CreateSecureCode()
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(32);

        return Convert
            .ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
