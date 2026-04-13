º# Redis Code References

This document lists all source files that use Redis directly or are part of the Redis cache flow.

## 1) Cache Contract (Application Layer)

- File: `src/Application/Interfaces/IStudentCacheService.cs`
- Purpose: Defines the cache operations used by the application service.
- Methods:
  - `GetByIdAsync`
  - `SetByIdAsync`
  - `GetAllAsync`
  - `SetAllAsync`
  - `InvalidateByIdAsync`
  - `InvalidateAllAsync`

## 2) Cache Usage (Application Service)

- File: `src/Application/Students/Services/StudentService.cs`
- Purpose: Uses `IStudentCacheService` for read-through caching and invalidation.

StudentService in `StudentService.cs` is the application-layer orchestrator for student use cases.
It does not talk directly to EF Core or Redis internals. It depends on abstractions:

Student repository for persistence operations
Student cache service for cache operations
Those dependencies are injected through the constructor at`StudentService.cs:14`


### GET flow
- `GetByIdAsync`: tries cache first; on miss reads DB and then caches by id.
- `GetAllAsync`: tries cache first; on miss reads DB and then caches tenant list.

### Write flow
- `CreateAsync`: caches by id and invalidates tenant list cache.
- `UpdateAsync`: updates cache by id and invalidates tenant list cache.
- `DeleteAsync`: invalidates by-id cache and tenant list cache.

## 3) Redis Implementation (Infrastructure)

- File: `src/Infrastructure/Caching/RedisStudentCacheService.cs`
- Purpose: Real implementation backed by `IDistributedCache` (StackExchange.Redis provider).

### Behavior
- TTL: `AbsoluteExpirationRelativeToNow = 30 days`.
- Serialization: `System.Text.Json`.
- Cache keys:
  - `students:tenant:{tenantId}:id:{id}`
  - `students:tenant:{tenantId}:all`

### Logs emitted
- `REDIS MISS {CacheKey}`
- `REDIS HIT {CacheKey}`
- `REDIS SET {CacheKey}`
- `REDIS DEL {CacheKey}`

## 4) No-Op Fallback (Infrastructure)

- File: `src/Infrastructure/Caching/NoOpStudentCacheService.cs`
- Purpose: Fallback implementation when Redis is not configured.
- Behavior: returns null for reads and does nothing for writes/invalidation.

## 5) Dependency Injection Wiring

- File: `src/Infrastructure/DependencyInjection/InfrastructureServiceCollectionExtensions.cs`
- Purpose: Chooses Redis or fallback implementation based on config.

### Decision rule
- If `Redis:ConnectionString` exists:
  - registers `AddStackExchangeRedisCache`
  - registers `IStudentCacheService -> RedisStudentCacheService`
- Else:
  - registers `AddDistributedMemoryCache`
  - registers `IStudentCacheService -> NoOpStudentCacheService`

## 6) NuGet Package

- File: `src/Infrastructure/StudentApi.Infrastructure.csproj`
- Redis package:
  - `Microsoft.Extensions.Caching.StackExchangeRedis`

## 7) Configuration Files

- File: `src/Presentation/appsettings.json`
  - Contains `Redis` section with default values (empty connection string by default).

- File: `src/Presentation/appsettings.Development.json`
  - Contains local Redis values:
    - `ConnectionString = localhost:6379`
    - `InstanceName = StudentApi:`

## 8) Docker Compose Integration

- File: `docker-compose.yml`
- Redis service:
  - `redis:7-alpine`
  - port mapping `6379:6379`
- API container env vars:
  - `Redis__ConnectionString=redis:6379`
  - `Redis__InstanceName=StudentApi:`

## 9) Scope Note

This document includes source/runtime configuration references only. Generated build artifacts under `bin/` and `obj/` are intentionally excluded.
