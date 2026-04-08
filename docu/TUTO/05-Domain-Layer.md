# 05 — Domain Layer

The **Domain Layer** is the innermost layer of Clean Architecture. It defines the core business entities — the fundamental data structures that the entire application is built around. It has **zero dependencies** on any other layer or external library.

**Project**: `src/Domain/StudentApi.Domain.csproj`

---

## What Lives Here?

```
Domain/
└── Entities/
    ├── Student.cs           # Core business entity
    ├── UserAccount.cs       # Authentication entity
    └── RefreshToken.cs      # Token lifecycle entity
```

That's it. The Domain Layer is intentionally small and focused.

---

## Why Is It So Simple?

In Clean Architecture, the Domain Layer answers one question: **"What does the business data look like?"**

It does **not** contain:
- Database logic (that's Infrastructure)
- Validation rules (that's Application)
- HTTP concerns (that's Presentation)
- References to EF Core, ASP.NET, or any NuGet package

This isolation means the Domain Layer can be used by any consumer — a web API, a console app, a background worker — without dragging in web framework dependencies.

```
Domain Layer depends on: nothing
Everything else depends on: Domain Layer
```

---

## Entities

### Student

```csharp
namespace StudentApi.Domain.Entities;

public record Student
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string Name { get; init; } = string.Empty;
    public DateOnly DateOfBirth { get; init; }
}
```

| Property | Type | Purpose |
|----------|------|---------|
| `Id` | `Guid` | Unique identifier (primary key in DB) |
| `TenantId` | `Guid` | Multi-tenancy scope — isolates data per tenant |
| `Name` | `string` | Student's display name |
| `DateOfBirth` | `DateOnly` | Birth date (no time component needed) |

### UserAccount

```csharp
public record UserAccount
{
    public Guid Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string PasswordHash { get; init; } = string.Empty;
    public string Role { get; init; } = "User";
    public bool IsActive { get; init; } = true;
}
```

| Property | Type | Purpose |
|----------|------|---------|
| `Id` | `Guid` | Unique identifier |
| `Username` | `string` | Login credential (unique in DB) |
| `PasswordHash` | `string` | PBKDF2-SHA256 hash — never the plain password |
| `Role` | `string` | Authorization role (`"Admin"`, `"User"`, etc.) |
| `IsActive` | `bool` | Soft-disable flag — inactive accounts can't log in |

### RefreshToken

```csharp
public record RefreshToken
{
    public Guid Id { get; init; }
    public Guid UserAccountId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string TokenHash { get; init; } = string.Empty;
    public DateTime CreatedAtUtc { get; init; }
    public DateTime ExpiresAtUtc { get; init; }
    public DateTime? RevokedAtUtc { get; init; }
    public string? ReplacedByTokenHash { get; init; }
}
```

| Property | Type | Purpose |
|----------|------|---------|
| `Id` | `Guid` | Token record identifier |
| `UserAccountId` | `Guid` | Owner reference |
| `Username` | `string` | Snapshot of username at token creation time |
| `Role` | `string` | Snapshot of role at token creation time |
| `TokenHash` | `string` | SHA-256 hash of the actual token (never stored raw) |
| `CreatedAtUtc` | `DateTime` | When the token was issued |
| `ExpiresAtUtc` | `DateTime` | When it expires (7 days from creation) |
| `RevokedAtUtc` | `DateTime?` | When it was revoked (`null` = still active) |
| `ReplacedByTokenHash` | `string?` | Hash of the new token that replaced this one during rotation |

> **Why snapshot Username and Role?** If a user's role changes between token issuance and refresh, the snapshot records what role the token was originally issued for. This creates an audit trail.

---

## Records vs Classes — Why Records?

All three entities use C# **records** instead of **classes**. This is a deliberate design choice.

### What makes records different?

| Feature | `class` | `record` |
|---------|---------|----------|
| Equality | Reference equality (same object in memory) | Value equality (same data = equal) |
| `ToString()` | Type name only | All properties printed |
| `GetHashCode()` | Based on reference | Based on property values |
| `with` expression | Not supported | Creates a copy with modified properties |
| Mutability | Mutable by default | Immutable with `init` setters |

### The `with` expression in action

In `StudentService.UpdateAsync`, records enable clean updates:

```csharp
var updatedStudent = currentStudent with
{
    Name = request.Name,
    DateOfBirth = request.DateOfBirth
};
```

This creates a **new** `Student` instance with the modified properties. The original `currentStudent` is unchanged. This pattern avoids accidental mutation bugs.

### `init` vs `set`

```csharp
public Guid Id { get; init; }   // Can only be set during object initialization
public Guid Id { get; set; }    // Can be set at any time
```

Using `init` makes the properties settable only in the constructor or object initializer:

```csharp
// ✅ Allowed
var student = new Student { Id = Guid.NewGuid(), Name = "Alice" };

// ❌ Compile error — can't change after initialization
student.Name = "Bob";

// ✅ But you can create a copy with `with`
var updated = student with { Name = "Bob" };
```

---

## DateOnly vs DateTime

The `Student.DateOfBirth` property uses `DateOnly` instead of `DateTime`.

| Type | Stores | Example | Use case |
|------|--------|---------|----------|
| `DateTime` | Date + time + kind | `2000-01-15 14:30:00` | Timestamps, events |
| `DateOnly` | Date only | `2000-01-15` | Birth dates, holidays |

**Why `DateOnly` here?** A birth date has no time component. Using `DateTime` would introduce unnecessary precision and potential timezone bugs. The `StudentConfiguration` maps this to the SQL `date` column type.

---

## Design Principles in Action

### 1. No Framework Dependencies

The `.csproj` file has no `<PackageReference>` entries. The Domain Layer is a pure .NET class library.

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
</Project>
```

### 2. No Validation Here

You might be tempted to add `[Required]` attributes or validation logic to entities. Don't. Validation lives in the Application Layer (`FluentValidation`). The Domain Layer defines the shape of data, not the rules for accepting it.

### 3. No Navigation Properties

None of the entities have EF Core navigation properties (e.g., `public UserAccount User { get; }` on `RefreshToken`). This is intentional:
- Navigation properties create coupling to EF Core.
- Repositories handle joins explicitly when needed.
- Keeps domain entities framework-agnostic.

### 4. Strings with Defaults

```csharp
public string Name { get; init; } = string.Empty;
```

Default values like `string.Empty` prevent null reference issues and make the entities safe to instantiate without setting every property.

---

## How Other Layers Use Domain Entities

```
Domain Layer          Application Layer        Infrastructure Layer
  Student     →     StudentMappings.ToDto()     StudentRepository (EF Core)
  UserAccount →     IUserAuthRepository         UserAuthRepository (EF Core)
  RefreshToken →    IRefreshTokenRepository     RefreshTokenRepository (EF Core)
```

- **Application**: Uses entities as parameters and return types in repository interfaces. Converts them to DTOs before exposing to the Presentation Layer.
- **Infrastructure**: Maps entities to database tables using `IEntityTypeConfiguration<T>`. EF Core reads/writes them.
- **Presentation**: Never touches domain entities directly — it works with DTOs and request records.

---

## Key Takeaways

| Concept | Implementation |
|---------|---------------|
| **Zero dependencies** | No NuGet packages, no framework references |
| **Records for entities** | Value equality, `with` expressions, immutability |
| **`init` properties** | Set once during initialization, then frozen |
| **`DateOnly`** | Correct type for dates without time components |
| **String defaults** | `= string.Empty` prevents null reference issues |
| **No navigation properties** | Keeps entities framework-agnostic |
| **Multi-tenancy at entity level** | `TenantId` is a first-class property on `Student` |
