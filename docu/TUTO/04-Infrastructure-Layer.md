# 04 — Infrastructure Layer

The **Infrastructure Layer** implements the interfaces defined in the Application Layer. It knows **how** to talk to SQL Server, Redis, and any other external system. This is where frameworks and libraries live.

**Project**: `src/Infrastructure/StudentApi.Infrastructure.csproj`

---

## What Lives Here?

```
Infrastructure/
├── Persistence/
│   ├── ApplicationDbContext.cs          # EF Core DbContext
│   └── Migrations/                      # Database version history
│       ├── InitialCreate                # Students table
│       ├── AddUserAccounts              # UserAccounts table + seed
│       └── AddRefreshTokens             # RefreshTokens table
├── Configurations/
│   ├── StudentConfiguration.cs          # Task 7: entity config
│   ├── UserAccountConfiguration.cs      # Task 7 + Task 8: config + seeding
│   └── RefreshTokenConfiguration.cs     # Task 7: entity config
├── Repositories/
│   ├── StudentRepository.cs             # IStudentRepository implementation
│   ├── UserAuthRepository.cs            # IUserAuthRepository implementation
│   └── RefreshTokenRepository.cs        # IRefreshTokenRepository implementation
├── Caching/
│   ├── RedisStudentCacheService.cs      # IStudentCacheService with Redis
│   └── NoOpStudentCacheService.cs       # IStudentCacheService fallback (no cache)
└── DependencyInjection/
    └── InfrastructureServiceCollectionExtensions.cs  # DI registration
```

---

## ApplicationDbContext — The Database Gateway

**File**: `Persistence/ApplicationDbContext.cs`

```csharp
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Student> Students => Set<Student>();
    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
```

**Key design decisions**:

1. **`DbSet<T>` as expression-bodied properties**: Using `=> Set<T>()` instead of `{ get; set; }` avoids null warnings and is the modern .NET style.

2. **`ApplyConfigurationsFromAssembly`**: Instead of manually calling `modelBuilder.Entity<Student>(...)` for each entity, this scans the assembly for all `IEntityTypeConfiguration<T>` classes and applies them. Add a new entity config class → it's picked up automatically.

3. **No business logic here**: The DbContext is purely a data mapping concern.

---

## Entity Configurations (Task 7)

Each entity has its own configuration class implementing `IEntityTypeConfiguration<T>`. These live **outside** `OnModelCreating` — keeping the DbContext clean.

### StudentConfiguration

```csharp
public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.TenantId).IsRequired();
        builder.Property(s => s.Name).HasMaxLength(200).IsRequired();
        builder.Property(s => s.DateOfBirth).HasColumnType("date").IsRequired();

        builder.HasIndex(s => new { s.TenantId, s.Name });
    }
}
```

**What each line does**:

| Configuration | Purpose |
|--------------|---------|
| `ToTable("Students")` | Maps to a specific table name |
| `HasKey(s => s.Id)` | Sets `Id` as the primary key |
| `HasMaxLength(200)` | SQL column: `nvarchar(200)` — matches FluentValidation rule |
| `HasColumnType("date")` | Uses SQL `date` type instead of `datetime2` (no time component) |
| `HasIndex(TenantId, Name)` | Composite index for fast tenant-scoped name lookups |

### UserAccountConfiguration (+ Task 8: Seeding)

```csharp
public class UserAccountConfiguration : IEntityTypeConfiguration<UserAccount>
{
    public void Configure(EntityTypeBuilder<UserAccount> builder)
    {
        builder.ToTable("UserAccounts");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Username).HasMaxLength(100).IsRequired();
        builder.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Role).HasMaxLength(50).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();

        builder.HasIndex(x => x.Username).IsUnique();

        // Task 8: Database seeding — admin user
        builder.HasData(new UserAccount
        {
            Id = Guid.Parse("9f0e6c26-0ff2-4bb9-93dd-f2bf5074a9a3"),
            Username = "admin",
            PasswordHash = "100000.YgFY3Gm4EwL1lz+uDGx69g==.w8AMSr3pbnGTpY5ZDgOD+9gmwWknPiOYO4q512LezBE=",
            Role = "Admin",
            IsActive = true
        });
    }
}
```

**Database seeding** uses EF Core's `HasData()` method:
- The seed data is included in the migration — it runs during `dotnet ef database update`.
- The seeded password hash corresponds to `admin123` (PBKDF2-SHA256 format: `iterations.salt.hash`).
- `HasData` is **idempotent**: running migrations again won't duplicate the admin user.
- The `Guid` is hardcoded so EF Core can track it across migrations.

### RefreshTokenConfiguration

```csharp
public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TokenHash).HasMaxLength(128).IsRequired();
        builder.HasIndex(x => x.TokenHash).IsUnique();

        builder.HasIndex(x => new { x.UserAccountId, x.ExpiresAtUtc });
    }
}
```

**Index design**:
- `TokenHash` unique index: ensures no duplicate tokens and enables fast hash lookups during refresh.
- `(UserAccountId, ExpiresAtUtc)` composite index: supports cleanup queries ("find expired tokens for user X").

---

## Repositories — Database Access

### StudentRepository

```csharp
public class StudentRepository : IStudentRepository
{
    private readonly ApplicationDbContext _dbContext;

    public async Task<Student?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId, cancellationToken);
    }

    public async Task<IReadOnlyList<Student>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Students
            .AsNoTracking()
            .Where(s => s.TenantId == tenantId)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Student student, CancellationToken cancellationToken = default)
    {
        _dbContext.Students.Add(student);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    // UpdateAsync and DeleteAsync follow the same pattern...
}
```

**Important patterns**:

| Pattern | Example | Why |
|---------|---------|-----|
| **`AsNoTracking()`** | Read queries | Better performance — EF doesn't track changes for objects we only read |
| **Tenant scoping** | `s.TenantId == tenantId` | Every query filters by tenant — no global data leaks |
| **`SaveChangesAsync` per operation** | `AddAsync`, `UpdateAsync`, `DeleteAsync` | Maps to one implicit DB transaction per call |
| **`FirstOrDefaultAsync`** | Returns `null` if not found | Service layer decides what to do (throw `NotFoundException`) |

### UserAuthRepository

```csharp
public async Task<UserAccount?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
{
    return await _dbContext.UserAccounts
        .AsNoTracking()
        .FirstOrDefaultAsync(u => u.Username == username && u.IsActive, cancellationToken);
}
```

**Note**: Only returns **active** users. Deactivated accounts can't log in.

### RefreshTokenRepository

```csharp
public async Task<RefreshToken?> GetActiveByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
{
    return await _dbContext.RefreshTokens
        .AsNoTracking()
        .FirstOrDefaultAsync(rt =>
            rt.TokenHash == tokenHash &&
            rt.RevokedAtUtc == null &&
            rt.ExpiresAtUtc > DateTime.UtcNow,
            cancellationToken);
}

public async Task RevokeAsync(Guid id, string replacedByTokenHash, CancellationToken cancellationToken = default)
{
    var token = await _dbContext.RefreshTokens.FindAsync(new object[] { id }, cancellationToken);
    if (token is not null)
    {
        var revokedToken = token with
        {
            RevokedAtUtc = DateTime.UtcNow,
            ReplacedByTokenHash = replacedByTokenHash
        };
        _dbContext.Entry(token).CurrentValues.SetValues(revokedToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
```

**Token lookup filters**: A token is "active" only if:
1. Its hash matches the lookup hash
2. It has not been revoked (`RevokedAtUtc == null`)
3. It has not expired (`ExpiresAtUtc > now`)

---

## Caching — Redis + NoOp Fallback

See [07-Redis-Cache-vs-Database.md](07-Redis-Cache-vs-Database.md) for the full deep-dive on caching strategy.

**Quick summary**: Two implementations of `IStudentCacheService`:

| Class | When Used | Behavior |
|-------|-----------|----------|
| `RedisStudentCacheService` | Redis connection string is configured | Full Redis cache with JSON serialization and 30-day TTL |
| `NoOpStudentCacheService` | Redis not configured | Returns `null` for all reads, no-ops for all writes |

This ensures the API works correctly even without Redis — it just always goes to the database.

---

## Dependency Injection Registration

**File**: `DependencyInjection/InfrastructureServiceCollectionExtensions.cs`

```csharp
public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
{
    // 1. Database
    var connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");
    services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

    // 2. Cache (conditional registration)
    var redisConnectionString = configuration["Redis:ConnectionString"];
    if (!string.IsNullOrWhiteSpace(redisConnectionString))
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = configuration["Redis:InstanceName"] ?? "StudentApi:";
        });
        services.AddScoped<IStudentCacheService, RedisStudentCacheService>();
    }
    else
    {
        services.AddDistributedMemoryCache();
        services.AddScoped<IStudentCacheService, NoOpStudentCacheService>();
    }

    // 3. Repositories
    services.AddScoped<IStudentRepository, StudentRepository>();
    services.AddScoped<IUserAuthRepository, UserAuthRepository>();
    services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

    return services;
}
```

**Key patterns**:

1. **Fail-fast**: Missing connection string throws immediately at startup.
2. **Conditional cache registration**: The same interface (`IStudentCacheService`) gets a different implementation based on configuration. The rest of the app doesn't know or care which one is active.
3. **Extension method**: All infrastructure DI is in one focused method, called from `Program.cs` as `builder.Services.AddInfrastructure(builder.Configuration)`.
4. **Scoped lifetime**: Repositories are `Scoped` — one instance per HTTP request. This matches `DbContext`'s default lifetime.

---

## Migrations — Database Version History

Migrations are auto-generated by EF Core. They represent the database schema at a point in time.

| # | Migration | Tables Created | Notable |
|---|-----------|----------------|---------|
| 1 | `InitialCreate` | `Students` | Composite index on `(TenantId, Name)` |
| 2 | `AddUserAccounts` | `UserAccounts` | Unique index on `Username` + admin seed data |
| 3 | `AddRefreshTokens` | `RefreshTokens` | Unique `TokenHash` + composite `(UserAccountId, ExpiresAtUtc)` |

### Common migration commands

```bash
# Apply all pending migrations
dotnet ef database update --project src/Infrastructure --startup-project src/Presentation

# Create a new migration after changing entity configs
dotnet ef migrations add MigrationName --project src/Infrastructure --startup-project src/Presentation

# Revert to a previous migration
dotnet ef database update PreviousMigrationName --project src/Infrastructure --startup-project src/Presentation
```

> **Note**: The `--project` flag points to where migrations live (Infrastructure). The `--startup-project` points to where the `DbContext` is configured (Presentation, via `Program.cs`).

---

## Database Schema (Final State)

```
┌──────────────────────────────┐
│         Students             │
├──────────────────────────────┤
│ Id           uniqueidentifier│ ← PK
│ TenantId     uniqueidentifier│
│ Name         nvarchar(200)   │
│ DateOfBirth   date            │
│ INDEX: (TenantId, Name)      │
└──────────────────────────────┘

┌──────────────────────────────┐
│       UserAccounts           │
├──────────────────────────────┤
│ Id           uniqueidentifier│ ← PK
│ Username     nvarchar(100)   │ ← UNIQUE
│ PasswordHash nvarchar(500)   │
│ Role         nvarchar(50)    │
│ IsActive     bit             │
│ SEED: admin / admin123       │
└──────────────────────────────┘

┌──────────────────────────────┐
│       RefreshTokens          │
├──────────────────────────────┤
│ Id                  uniqueidentifier│ ← PK
│ UserAccountId       uniqueidentifier│
│ Username            nvarchar(100)   │
│ Role                nvarchar(50)    │
│ TokenHash           nvarchar(128)   │ ← UNIQUE
│ CreatedAtUtc        datetime2       │
│ ExpiresAtUtc        datetime2       │
│ RevokedAtUtc        datetime2 NULL  │
│ ReplacedByTokenHash nvarchar(128) NULL│
│ INDEX: (UserAccountId, ExpiresAtUtc)│
└──────────────────────────────┘
```

---

## Key Takeaways

| Concept | Implementation |
|---------|---------------|
| **Framework isolation** | EF Core, Redis, SQL Server — all contained in Infrastructure |
| **Entity configuration classes** | One `IEntityTypeConfiguration<T>` per entity, clean DbContext |
| **Database seeding** | `HasData()` in configuration, idempotent, part of migrations |
| **Repository pattern** | Thin data-access layer, tenant-scoped queries |
| **Conditional DI** | Redis or NoOp cache based on configuration |
| **`AsNoTracking`** | Used in all read queries for performance |
| **Extension methods for DI** | `AddInfrastructure()` keeps `Program.cs` clean |
