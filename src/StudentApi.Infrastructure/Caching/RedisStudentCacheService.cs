using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using StudentApi.Application.Interfaces;
using StudentApi.Application.Students;
using System.Text.Json;

namespace StudentApi.Infrastructure.Caching;

public sealed class RedisStudentCacheService : IStudentCacheService
{
    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
    };

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<RedisStudentCacheService> _logger;

    public RedisStudentCacheService(IDistributedCache distributedCache, ILogger<RedisStudentCacheService> logger)
    {
        _distributedCache = distributedCache;
        _logger = logger;
    }

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

    public Task SetByIdAsync(StudentDto student, CancellationToken cancellationToken = default)
    {
        var key = BuildByIdKey(student.Id, student.TenantId);
        var payload = JsonSerializer.Serialize(student, SerializerOptions);

        _logger.LogInformation("REDIS SET {CacheKey}", key);

        return _distributedCache.SetStringAsync(key, payload, CacheOptions, cancellationToken);
    }

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

    public Task SetAllAsync(Guid tenantId, IReadOnlyList<StudentDto> students, CancellationToken cancellationToken = default)
    {
        var key = BuildAllKey(tenantId);
        var payload = JsonSerializer.Serialize(students, SerializerOptions);

        _logger.LogInformation("REDIS SET {CacheKey}", key);

        return _distributedCache.SetStringAsync(key, payload, CacheOptions, cancellationToken);
    }

    public Task InvalidateByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var key = BuildByIdKey(id, tenantId);
        _logger.LogInformation("REDIS DEL {CacheKey}", key);
        return _distributedCache.RemoveAsync(key, cancellationToken);
    }

    public Task InvalidateAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var key = BuildAllKey(tenantId);
        _logger.LogInformation("REDIS DEL {CacheKey}", key);
        return _distributedCache.RemoveAsync(key, cancellationToken);
    }

    private static string BuildByIdKey(Guid id, Guid tenantId) => $"students:tenant:{tenantId}:id:{id}";

    private static string BuildAllKey(Guid tenantId) => $"students:tenant:{tenantId}:all";
}
