using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using StudentApi.Application.Interfaces;
using StudentApi.Application.Students;
using System.Text.Json;

namespace StudentApi.Infrastructure.Caching;


/// Redis-backed implementation of student cache operations.
public sealed class RedisStudentCacheService : IStudentCacheService
{
    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30)
    };

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<RedisStudentCacheService> _logger;

    
    /// Creates a Redis cache service.
    public RedisStudentCacheService(IDistributedCache distributedCache, ILogger<RedisStudentCacheService> logger)
    {
        _distributedCache = distributedCache;
        _logger = logger;
    }

    /// Gets a cached student by id and tenant.
    /// <returns>The cached student DTO or <c>null</c> on cache miss.</returns>
    public async Task<StudentDto?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var key = BuildByIdKey(id, tenantId);
        var payload = await _distributedCache.GetStringAsync(key, cancellationToken);

        if (string.IsNullOrWhiteSpace(payload))
        {
            _logger.LogInformation("REDIS MISS {CacheKey}", key);
            return null;
        }

        _logger.LogInformation("REDIS HIT {CacheKey}", key);

        return JsonSerializer.Deserialize<StudentDto>(payload, SerializerOptions);
    }

    /// Stores a student DTO in cache by id.
    /// <returns>A task that completes when cache write finishes.</returns>
    public Task SetByIdAsync(StudentDto student, CancellationToken cancellationToken = default)
    {
        var key = BuildByIdKey(student.Id, student.TenantId);
        var payload = JsonSerializer.Serialize(student, SerializerOptions);

        _logger.LogInformation("REDIS SET {CacheKey}", key);

        return _distributedCache.SetStringAsync(key, payload, CacheOptions, cancellationToken);
    }

   
    /// Gets cached student list by tenant.
    /// <returns>Cached student list or <c>null</c> on cache miss.</returns>
    public async Task<IReadOnlyList<StudentDto>?> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var key = BuildAllKey(tenantId);
        var payload = await _distributedCache.GetStringAsync(key, cancellationToken);

        if (string.IsNullOrWhiteSpace(payload))
        {
            _logger.LogInformation("REDIS MISS {CacheKey}", key);
            return null;
        }

        _logger.LogInformation("REDIS HIT {CacheKey}", key);

        return JsonSerializer.Deserialize<IReadOnlyList<StudentDto>>(payload, SerializerOptions);
    }

 
    /// Stores a tenant-scoped student list in cache.
    /// <returns>A task that completes when cache write finishes.</returns>
    public Task SetAllAsync(Guid tenantId, IReadOnlyList<StudentDto> students, CancellationToken cancellationToken = default)
    {
        var key = BuildAllKey(tenantId);
        var payload = JsonSerializer.Serialize(students, SerializerOptions);

        _logger.LogInformation("REDIS SET {CacheKey}", key);

        return _distributedCache.SetStringAsync(key, payload, CacheOptions, cancellationToken);
    }


    /// Removes cached student-by-id entry.
    /// <returns>A task that completes when cache delete finishes.</returns>
    public Task InvalidateByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var key = BuildByIdKey(id, tenantId);
        _logger.LogInformation("REDIS DEL {CacheKey}", key);
        return _distributedCache.RemoveAsync(key, cancellationToken);
    }

   
    /// Removes cached tenant student-list entry.
    /// <returns>A task that completes when cache delete finishes.</returns>
    public Task InvalidateAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var key = BuildAllKey(tenantId);
        _logger.LogInformation("REDIS DEL {CacheKey}", key);
        return _distributedCache.RemoveAsync(key, cancellationToken);
    }

  
    /// Builds a stable cache key for student-by-id entries.
    /// <returns>Redis key string.</returns>
    private static string BuildByIdKey(Guid id, Guid tenantId) => $"students:tenant:{tenantId}:id:{id}";

   
    /// Builds a stable cache key for tenant student-list entries.
    /// <returns>Redis key string.</returns>
    private static string BuildAllKey(Guid tenantId) => $"students:tenant:{tenantId}:all";
}
