# 07 — Redis Cache vs Database

This document explains the caching strategy used in StudentAPI: when data is read from Redis vs the database, how cache keys are structured, how invalidation works, and how the system gracefully falls back when Redis is unavailable.

---

## Why Cache at All?

Database queries are relatively slow compared to in-memory lookups:

| Operation | Typical Latency |
|-----------|----------------|
| Redis GET (local) | ~0.1–1 ms |
| SQL Server query (local Docker) | ~5–50 ms |
| SQL Server query (network) | ~20–200 ms |

For data that's read frequently but changes infrequently (like student records), caching can reduce response times by 10–100x and decrease database load significantly.

---

## Cache Strategy: Cache-Aside (Lazy Loading)

StudentAPI uses the **cache-aside** pattern (also called "lazy loading"):

```
Read request comes in
        │
        ▼
  ┌─── Check cache ───┐
  │                    │
  ▼ HIT                ▼ MISS
Return cached      Query database
data immediately        │
                   ┌────┤
                   │    ▼
                   │  Store result
                   │  in cache
                   │    │
                   ▼    ▼
              Return data to client
```

### Cache-Aside vs Other Strategies

| Strategy | Description | Used Here? |
|----------|-------------|------------|
| **Cache-Aside** | Application checks cache, falls back to DB, populates cache | ✅ Yes |
| **Write-Through** | Every write goes to DB + cache simultaneously | ✅ Partial (on create/update) |
| **Write-Behind** | Write to cache only, DB synced asynchronously | ❌ No |
| **Read-Through** | Cache fetches from DB automatically on miss | ❌ No |

The project combines cache-aside reads with write-through updates — when data is created or updated, both the database and cache are written to immediately.

---

## Dual Cache Pattern

StudentAPI maintains **two types of cache entries** per tenant:

### 1. By-ID Cache

```
Key:   students:tenant:{tenantId}:id:{studentId}
Value: JSON of a single StudentDto
TTL:   30 days
```

Used by `GetByIdAsync` — fetches a single student.

### 2. List Cache

```
Key:   students:tenant:{tenantId}:all
Value: JSON array of all StudentDtos for the tenant
TTL:   30 days
```

Used by `GetAllAsync` — fetches all students for a tenant.

### Why Two Caches?

- **By-ID**: Fast lookups for a specific student (e.g., detail pages).
- **List**: Fast listing for all students (e.g., table views).

They serve different use cases and have different invalidation rules.

---

## Cache Key Design

```
students:tenant:{tenantId}:id:{studentId}
└──────┘ └────┘ └────────┘ └─┘ └─────────┘
 prefix  scope   tenant ID  type  student ID

students:tenant:{tenantId}:all
└──────┘ └────┘ └────────┘ └─┘
 prefix  scope   tenant ID  list marker
```

**Design principles**:
- **Prefix**: Groups all student-related keys under `students:`.
- **Tenant scoping**: Each tenant has its own cache entries — no cross-tenant data leaks.
- **Readable**: Keys are human-readable for debugging (`redis-cli KEYS "students:*"`).
- **Instance name prefix**: Redis is configured with `InstanceName = "StudentApi:"`, so actual keys in Redis are `StudentApi:students:tenant:...`.

---

## What Happens on Each Operation?

### Read by ID (`GetByIdAsync`)

```csharp
// 1. Check cache
var cachedStudent = await _studentCacheService.GetByIdAsync(id, tenantId);
if (cachedStudent is not null) return cachedStudent;    // ← CACHE HIT

// 2. Cache miss → query database
var student = await _studentRepository.GetByIdAsync(id, tenantId);
if (student is null) throw new NotFoundException(...);

// 3. Populate cache for next time
var studentDto = student.ToDto();
await _studentCacheService.SetByIdAsync(studentDto);   // ← CACHE SET

return studentDto;
```

### Read All (`GetAllAsync`)

Same pattern but with the list cache key.

### Create (`CreateAsync`)

```csharp
await _studentRepository.AddAsync(student);                              // DB write
await _studentCacheService.SetByIdAsync(studentDto);                     // Cache the new item
await _studentCacheService.InvalidateAllAsync(student.TenantId);         // Invalidate list cache
```

**Why invalidate the list?** After adding a new student, the cached list is stale (missing the new student). Instead of rebuilding it, we simply delete it. The next `GetAllAsync` call will query the database and rebuild the cache.

### Update (`UpdateAsync`)

```csharp
await _studentRepository.UpdateAsync(updatedStudent);                    // DB write
await _studentCacheService.SetByIdAsync(studentDto);                     // Update by-ID cache
await _studentCacheService.InvalidateAllAsync(tenantId);                 // Invalidate list cache
```

The by-ID cache is **overwritten** with the new data. The list cache is **invalidated**.

### Delete (`DeleteAsync`)

```csharp
await _studentRepository.DeleteAsync(request.Id, request.TenantId);     // DB delete
await _studentCacheService.InvalidateByIdAsync(request.Id, request.TenantId); // Remove from cache
await _studentCacheService.InvalidateAllAsync(request.TenantId);         // Invalidate list cache
```

Both cache entries for this student are removed.

### Summary Table

| Operation | By-ID Cache | List Cache |
|-----------|-------------|------------|
| **GetById** | Read (populate on miss) | — |
| **GetAll** | — | Read (populate on miss) |
| **Create** | Write (new entry) | Invalidate |
| **Update** | Write (overwrite) | Invalidate |
| **Delete** | Invalidate | Invalidate |

---

## Redis Implementation

**File**: `Infrastructure/Caching/RedisStudentCacheService.cs`

### Configuration

```csharp
private static readonly DistributedCacheEntryOptions CacheOptions = new()
{
    AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30)
};

private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
```

- **30-day TTL**: Cache entries expire automatically after 30 days, even if not invalidated.
- **Web JSON defaults**: Uses camelCase serialization (consistent with API responses).

### Read (GET)

```csharp
public async Task<StudentDto?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken)
{
    var key = BuildByIdKey(id, tenantId);
    var payload = await _distributedCache.GetStringAsync(key, cancellationToken);

    if (string.IsNullOrWhiteSpace(payload))
    {
        _logger.LogInformation("REDIS MISS {CacheKey}", key);
        return null;    // Cache miss → service will query DB
    }

    _logger.LogInformation("REDIS HIT {CacheKey}", key);
    return JsonSerializer.Deserialize<StudentDto>(payload, SerializerOptions);
}
```

### Write (SET)

```csharp
public Task SetByIdAsync(StudentDto student, CancellationToken cancellationToken)
{
    var key = BuildByIdKey(student.Id, student.TenantId);
    var payload = JsonSerializer.Serialize(student, SerializerOptions);

    _logger.LogInformation("REDIS SET {CacheKey}", key);
    return _distributedCache.SetStringAsync(key, payload, CacheOptions, cancellationToken);
}
```

### Delete (DEL)

```csharp
public Task InvalidateByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken)
{
    var key = BuildByIdKey(id, tenantId);
    _logger.LogInformation("REDIS DEL {CacheKey}", key);
    return _distributedCache.RemoveAsync(key, cancellationToken);
}
```

### Logging

Every cache operation is logged at `Information` level:

```
info: REDIS HIT students:tenant:1111...:id:aaaa...
info: REDIS MISS students:tenant:1111...:all
info: REDIS SET students:tenant:1111...:id:aaaa...
info: REDIS DEL students:tenant:1111...:all
```

This makes it easy to verify cache behavior in development by watching the console output.

---

## NoOp Fallback — When Redis Is Not Available

**File**: `Infrastructure/Caching/NoOpStudentCacheService.cs`

```csharp
public sealed class NoOpStudentCacheService : IStudentCacheService
{
    public Task<StudentDto?> GetByIdAsync(Guid id, Guid tenantId, ...)
        => Task.FromResult<StudentDto?>(null);       // Always "cache miss"

    public Task SetByIdAsync(StudentDto student, ...)
        => Task.CompletedTask;                        // No-op write

    public Task InvalidateByIdAsync(Guid id, Guid tenantId, ...)
        => Task.CompletedTask;                        // No-op delete

    // ... same for GetAllAsync, SetAllAsync, InvalidateAllAsync
}
```

### When Is It Used?

In `InfrastructureServiceCollectionExtensions`:

```csharp
var redisConnectionString = configuration["Redis:ConnectionString"];

if (!string.IsNullOrWhiteSpace(redisConnectionString))
{
    services.AddStackExchangeRedisCache(...);
    services.AddScoped<IStudentCacheService, RedisStudentCacheService>();
}
else
{
    services.AddDistributedMemoryCache();  // Required by IDistributedCache contract
    services.AddScoped<IStudentCacheService, NoOpStudentCacheService>();
}
```

When `Redis:ConnectionString` is empty (or missing), the `NoOpStudentCacheService` is registered. The service layer code (`StudentService`) doesn't change at all — it still calls `_studentCacheService.GetByIdAsync(...)`, but always gets `null` back (cache miss), so every request goes to the database.

**This pattern is called the Null Object Pattern**: Instead of checking `if (cacheService != null)` everywhere, you provide an implementation that does nothing. The code stays clean and the behavior degrades gracefully.

---

## Debugging Cache Behavior

### Using Redis CLI

```bash
# Connect to Redis
docker exec -it <redis-container> redis-cli

# See all StudentAPI keys
KEYS "StudentApi:*"

# Read a cached student
GET "StudentApi:students:tenant:11111111-1111-1111-1111-111111111111:id:aaaaaaaa-..."

# Check TTL (seconds remaining)
TTL "StudentApi:students:tenant:11111111-1111-1111-1111-111111111111:all"

# Flush all cache (use carefully!)
FLUSHALL
```

### Using Application Logs

Watch for `REDIS HIT`, `REDIS MISS`, `REDIS SET`, and `REDIS DEL` in the console output to understand the cache flow for each request.

---

## Cache Consistency Trade-offs

| Scenario | Behavior | Impact |
|----------|----------|--------|
| Normal CRUD via API | Cache always consistent | ✅ No issues |
| Direct DB modification (SQL query) | Cache is stale | ⚠️ Stale reads until TTL expires (30 days) |
| Multiple API instances (horizontal scaling) | All instances share Redis | ✅ Consistent |
| Redis goes down | NoOp fallback if configured at startup | ⚠️ If Redis crashes mid-operation, errors may occur |

> **Important**: If you modify data directly in SQL Server (not through the API), the cache will not know. You would need to manually flush the Redis keys or wait for the 30-day TTL.

---

## Key Takeaways

| Concept | Implementation |
|---------|---------------|
| **Cache-aside pattern** | Read cache → miss → query DB → populate cache |
| **Write-through** | On create/update: write to DB + set cache + invalidate list |
| **Tenant-scoped keys** | `students:tenant:{tenantId}:...` prevents cross-tenant leaks |
| **Dual cache** | By-ID for single lookups, list for collection queries |
| **TTL-based expiry** | 30-day absolute expiration prevents stale data forever |
| **Null Object Pattern** | `NoOpStudentCacheService` when Redis is not configured |
| **Conditional DI** | Cache implementation chosen at startup based on config |
| **Observable caching** | Every operation logged with key name and HIT/MISS/SET/DEL |
